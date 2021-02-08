
#'
#' Cerberus: monitoring arbitrage opportunities
#'


# Import dependencies ----

options(max.print = 1e3, scipen = 999, width = 1e2)
options(stringsAsFactors = F)

suppressPackageStartupMessages({
  library(dplyr)
  library(tibble)
  library(tidyr)
  library(magrittr)
  library(purrr)
  library(data.table)
  
  library(stringr)
  library(lubridate)
  
  library(skimr)

  library(ggplot2)
})

theme_set(theme_bw())



# Get configs and set params ----

job <- list(
  config = config::get(),
  secrets = config::get(file = "secrets.yml")
)

stopifnot(
  is.list(job),
  !is.null(job$config)
)

print(job$config)



# Core functions ----

#' Normalize column names of dataframe
#' 
#' @param dt 
normalize_col_names <- function(dt) {
  require(dplyr)
  stopifnot(is.data.frame(dt))
  
  names(dt) <- dt %>% names %>% tolower
  
  return(dt)
}


#' Round datetime and return timestamp
#'
#' @param x 
#' @param .unit 
#'
to_timestamp <- function(x, .unit = "second") {
  require(lubridate)
  require(dplyr)
  
  x %>% floor_date(.unit) %>% as.integer
}


#' Get name for key
#'
#' @param .key 
#' @param .dictionary 
#'
get_name_by_key <- function(.key, .dictionary) {
  stopifnot(
    !is.null(.key),
    is.vector(.dictionary)
  )
  
  names(.dictionary)[ which(.dictionary == .key) ]
}



# Load dataset ----

## Open DB connections
market_info_db_con <- dbConnect(odbc::odbc(), .connection_string = job$secrets$market_info_conn_string)
public_market_data_db_con <- dbConnect(odbc::odbc(), .connection_string = job$secrets$public_market_data_conn_string)


## Get history
orderbook_timestamps_dt <- dbGetQuery(market_info_db_con, "select * from orderbook_timestamps_vw")
orderbook_timestamps_dt %<>% normalize_col_names

stopifnot(
  nrow(orderbook_timestamps_dt) > 0,
  nrow(orderbook_timestamps_dt) == n_unique(orderbook_timestamps_dt$symbol)
)


orderbook_timestamps_dt %>% 
  select(symbol, ends_with("_timestamp")) %>% 
  pivot_longer(cols = ends_with("_timestamp")) %>% 
  
  ggplot(aes(x = value, y = symbol)) +
    geom_line()



# Experiment Functions ----

## Get orderbook for specific pair

get_settings <- function(dt) {
  stopifnot(
    is.data.frame(dt),
    nrow(dt) == 1
  )
  
  list(
    symbol = dt[1, ]$symbol,
    from_time = dt[1, ]$last_orderbook_timestamp - minutes(job$config$depth_in_minutes),
    to_time = dt[1, ]$last_orderbook_timestamp
  )
}



# Data and Clients API Functions ----

#' Execute SQL procedure
#'
#' @param .con 
#' @param .proc_name 
#' @param .settings 
#'
exec_proc <- function(.con, .proc_name, .settings) {
  require(odbc)
  require(tictoc)
  
  stopifnot(
    is.character(.proc_name),
    is.list(.settings) && length(.settings) == 3
  )
  
  tic(sprintf("[TRACE] Executing %s...", .proc_name))
  
  dt <- tryCatch({
    dbGetQuery(.con,
               sprintf("exec %s @symbol = '%s', @fromTime = '%s', @toTime = '%s'", .proc_name, .settings$symbol, .settings$from_time, .settings$to_time))
  }, warning = function(w) {
    warning(w)
  }, error = function(e) {
    stop(e)
  }, finally = {
    toc()
  })
  
  return(dt)
}



#' Get orderbooks
#'
#' @param .settings 
#'
get_orderbooks <- function(.settings) {
  exec_proc(market_info_db_con, "get_orderbook_sp", .settings) %>% 
    normalize_col_names
}


#' Get bot statuses
#'
#' @param .settings 
#' @param .exchanges_list 
#'
get_bot_statuses <- function(.settings, .exchanges_list) {
  
  .settings$from_time <- .settings$from_time - months(1) #! HACK
  
  dt <- exec_proc(market_info_db_con, "get_bot_statuses_history_sp", .settings) %>% 
    normalize_col_names %>% 
    #
    mutate(timestamp = to_timestamp(timestamp)) %>% 
    #
    group_by(exchange, symbol, timestamp) %>% 
    summarise(
      status = dplyr::last(status, order_by = timestamp),
      .groups = "drop"
    )
  
  
  if (nrow(dt) == 0) {
    print("[WARN] Records not found")
    
    exchanges_n <- length(.exchanges_list)
    
    dt %<>%
      rbind(tibble(
        exchange = .exchanges_list,
        symbol = rep(.settings$symbol, exchanges_n),
        status = rep(4L, exchanges_n),
        timestamp = rep(.settings$from_time, exchanges_n)
      ))
  } else {
    print(sprintf(
      "[INFO] Bots detected for following exchanges: %s", paste(unique(dt$exchange), collapse = ", ")
    ))
  }
  
  
  expand.grid(
      exchange = unique(dt$exchange),
      symbol = unique(dt$symbol),
      timestamp = seq(.settings$from_time, .settings$to_time, by = 1) %>% to_timestamp
    ) %>% 
    left_join(
      dt, by = c("exchange", "symbol", "timestamp")
    ) %>% 
    group_by(exchange, symbol) %>% 
    arrange(timestamp) %>% 
    fill(status, .direction = "down") %>% 
    ungroup
}


#' Get exchange rates
#'
#' @param .settings 
#'
get_1D_candles <- function(.settings) {
  
  dt <- exec_proc(public_market_data_db_con, "get_symbol_price_sp", .settings)

  if (nrow(dt) == 0)
    stop(sprintf("[ERROR] Prices not found for %s", .settings$symbols))
  
  dt %>% 
    ##
    normalize_col_names %>% 
    mutate(
      symbol = str_to_upper(symbol),
      date = as.Date(timestamp)
    ) %>% 
    
    ## calculate OHLC
    group_by(symbol, exchange, date) %>% 
    summarise(
      open = first(price, order_by = timestamp),
      high = max(price, na.rm = T),
      low = min(price, na.rm = T),
      close = last(price, order_by = timestamp),
      time = max(timestamp),
      
      .groups = "drop"
    )
}



# Finance functions ----

#' Calculate candle
#'
#' @param dt 
#'
calc_candle <- function(dt) {
  require(tidyr)
  require(dplyr)
  
  stopifnot(is.grouped_df(dt))
  
  dt %>% 
    ## calc best price
    summarise(
      min_price = min(price),
      max_price = max(price),
      .groups = "drop"
    ) %>% 
    mutate(
      best_price = if_else(direction == 0, max_price, min_price, NA_real_)
    ) %>% 
    select(-min_price, -max_price) %>% 
    spread(direction, best_price) %>% 
    rename(
      best_bid = `0`, best_ask = `1`
    ) %>% 
    
    ## calc mid price and spread
    mutate(
      mid_price = best_bid + (best_ask - best_bid)/2,
      spread = abs((best_ask - best_bid)/best_ask * 100)
    ) %>% 
    
    # add arbitrage opportunity flag
    mutate(
      arbitrage_flag = (best_ask - best_bid) < 0
    )
}



# Get settings for jobs ----

jobs_settings <- orderbook_timestamps_dt %>% 
  ## business rules
  filter(exchanges_n >= job$config$min_exchange_n) %>% 
  ## prepare
  group_by(symbol) %>% 
  group_split %>% 
  ## start
  map(get_settings)

print(jobs_settings)


# Remove pair with stable coins
not_usdt_indx <- jobs_settings %>% 
  map_lgl(~ str_ends(.x$symbol, job$config$stable_coin_symbol, negate = T)) %>% 
  which


arbitrage_stats <- jobs_settings[not_usdt_indx] %>% 
  map_dfr(
    function(.settings) {
      
      ## Start
      print(sprintf("[INFO] %s | Start processing...", .settings$symbol))
      
      
      ## Set lists
      #' Exchange list codes
      exchanges_list <- c(1:6)
      names(exchanges_list) <- c("Binance", "Bittrex", "CoinEx", "Huobi", "Kraken", "Kucoin")
      
      
      ## Get orderbook
      orderbooks_dt <- get_orderbooks(.settings)
      
      orderbooks_dt %<>% 
        mutate(date = as.Date(timestamp))
      
      stopifnot(
        nrow(orderbooks_dt) > 0,
        !anyNA(orderbooks_dt)
      )
      
      orderbooks_dt %>% as_tibble
      
      orderbooks_dt %>% 
        mutate(hour_of_day = hour(timestamp)) %>% 
        count(exchange, symbol, hour_of_day) %>% 
        spread(exchange, n, fill = 0) %>% 
        arrange(hour_of_day) %>% 
        as_tibble
      
      
      ## Get bot statuses
      bot_statuses_dt <- get_bot_statuses(.settings, exchanges_list)
      
      bot_statuses_dt %<>% 
        filter(!is.na(status)) %>% 
        rename(timestamp_round = timestamp)
      
      stopifnot(
        nrow(bot_statuses_dt) > 0,
        !anyNA(bot_statuses_dt),
        bot_statuses_dt$exchange %>% n_unique > 1,
        bot_statuses_dt %>% distinct(exchange, symbol, timestamp_round) %>% nrow == nrow(bot_statuses_dt)
      )
      
      bot_statuses_dt %>% as_tibble
      
      
      ## Get symbol to stable coin rate 
      usdt_settings <- .settings
      usdt_settings$symbol <- sprintf("%s%s", str_split(.settings$symbol, "/")[[1]][2], job$config$stable_coin_symbol) # TODO: get unified pair name
      
      print(sprintf("Get %s rate", usdt_settings$symbol))
      
      trading_pair_candles_dt <- get_1D_candles(usdt_settings)
      
      trading_pair_candles_dt %<>% 
        ##
        right_join(
          expand.grid(
            symbol = unique(trading_pair_candles_dt$symbol),
            exchange = orderbooks_dt %>% filter(symbol == .settings$symbol) %>% pull(exchange) %>% unique,
            date = orderbooks_dt %>% filter(symbol == .settings$symbol) %>% pull(date) %>% unique
          ),
          by = c("symbol", "exchange", "date")
        )
      
      
      get_min_close_price <- function(.date) {
        trading_pair_candles_dt %>% 
          filter(date == .date) %>% 
          pull(close) %>% 
          min(., na.rm = T)
      }
      
      trading_pair_candles_dt$min_close <- sapply(trading_pair_candles_dt$date, get_min_close_price)
      
      trading_pair_candles_dt %<>% 
        mutate(close = if_else(is.na(close), min_close, close, NA_real_)) %>% 
        select(-min_close)
      
      
      stopifnot(
        nrow(trading_pair_candles_dt) > 0,
        !anyNA(trading_pair_candles_dt %>% select(symbol, exchange, date, close))
      )
      
      trading_pair_candles_dt %>% as_tibble
      
      
      ## Join all together
      
      data <- orderbooks_dt %>% 
        ## filter exchanges with revert in base/trading currency
        filter(exchange != 2) %>% # TODO: get unified names of symbol
        ##
        mutate(
          timestamp_round = to_timestamp(timestamp)
        ) %>% 
        ##
        left_join(
          bot_statuses_dt, 
          by = c("exchange", "symbol", "timestamp_round")
        ) %>% 
        left_join(
          trading_pair_candles_dt %>% transmute(exchange, date, trading_pair_to_usdt_rate = close), 
          by = c("exchange", "date")
        ) %>% 
        ##
        mutate_at(
          all_of(c("symbol", "exchange", "direction", "status")),
          as.factor
        )
      
      stopifnot(
        nrow(data) > 0,
        nrow(data) <= nrow(orderbooks_dt),
        !anyNA(data)
      )
      
      
      data %>% 
        count(symbol, date, exchange) %>% 
        spread(exchange, n)
      
      
      print(
        sprintf("[INFO] Missed values for %s seconds", 
                data %>% filter(status != job$config$connected_bot_status) %>% pull(timestamp_round) %>% n_unique)
      )
      
      data %>% 
        sample_frac(.25) %>% 
        mutate(
          bot_status = if_else(status == job$config$connected_bot_status, "Connected", "Not connected", "Unknown"),
          direction_name = if_else(direction == 0, "Bid", "Ask", "Invalid")
        ) %>% 
        
        ggplot(aes(x = timestamp, y = price, color = exchange)) +
          geom_point(alpha = .25, size = .15) +
        
          scale_x_datetime(date_breaks = "4 hours", date_labels = "%H:%M %b %d") +
          facet_grid(direction_name ~ bot_status)
      
      ggsave(
        filename = sprintf("orders.%s.%s.png", str_replace(.settings$symbol, "/", "-"), Sys.Date()), path = "output",
        plot = last_plot(),
        width = 1920/320, height = 1280/320, 
        units = "in", dpi = 320
      )
      
      #
      data %<>% filter(status == job$config$connected_bot_status)
      
      
      ## Best prices and total volume (all exchanges)
      
      prices_all_exchanges_dt <- data %>% 
        group_by(symbol, direction, timestamp_round) %>% 
        calc_candle
      
      stopifnot(
        nrow(prices_all_exchanges_dt) > 0,
        !anyNA(prices_all_exchanges_dt),
        prices_all_exchanges_dt %>% distinct(timestamp_round, symbol) %>% nrow == nrow(prices_all_exchanges_dt),
        all(prices_all_exchanges_dt$best_ask > 0),
        all(prices_all_exchanges_dt$best_bid > 0)
      )
      
      prices_all_exchanges_dt
      
      
      ## Refine volume for trade
      
      volume_all_exchanges_dt <- data %>% 
        ##
        inner_join(
          prices_all_exchanges_dt,
          by = c("symbol", "timestamp_round")
        ) %>% 
        mutate(
          has_price_delta = if_else(direction == 0, price - best_ask, best_bid - price, NA_real_)
        ) %>% 
        filter(has_price_delta > 0) %>% 
        
        ## volume in 1m candles (all exchanges)
        group_by(
          symbol, direction, timestamp_round
        ) %>% 
        summarise(
          total_volume = sum(volume),
          .groups = "drop"
        ) 
      
      if(nrow(volume_all_exchanges_dt) == 0) {
        print("[WARN] There was no any arbitrage")
        return(NULL)
      }
      
      volume_all_exchanges_dt %<>% 
        
        ## liqudity volume
        spread(direction, total_volume) %>% 
        rename(
          total_bid = `0`, total_ask = `1`
        ) %>% 
        mutate(
          market_volume = if_else(total_bid > total_ask, total_ask, total_bid, NA_real_)
        )
      
      stopifnot(
        nrow(volume_all_exchanges_dt) > 0,
        !anyNA(volume_all_exchanges_dt),
        volume_all_exchanges_dt %>% distinct(timestamp_round, symbol) %>% nrow == nrow(volume_all_exchanges_dt),
        volume_all_exchanges_dt$market_volume > 0
      )
      
      volume_all_exchanges_dt
      
      
      
      ## Searching arbitrage cases
      
      arbitrage_cases_dt <- data %>% 
        
        ## get only quotes w/ arbitrage opportunity
        inner_join(
          prices_all_exchanges_dt,
          by = c("symbol", "timestamp_round")
        ) %>% 
        inner_join(
          volume_all_exchanges_dt,
          by = c("symbol", "timestamp_round")
        ) %>% 
        
        ## TODO: correct volume to job$config$risks$max_order_volume_in_usdt
        
        ## calculate weighted delta
        mutate(
          price_delta = if_else(direction == 0, price - best_ask, best_bid - price, NA_real_),
          w_price_delta = if_else(volume < market_volume, volume*price_delta, market_volume*price_delta),
          usdt_delta = w_price_delta*trading_pair_to_usdt_rate 
        ) %>% 
        
        ## update profit in USDT
        mutate(
          usdt_delta_refined = usdt_delta * (1 - 2*job$config$exchange_fee_pcnt),
          usdt_delta_refined = if_else(usdt_delta_refined < job$config$risks$min_order_volume_in_usdt, 0, usdt_delta_refined)
        )
      
      stopifnot(
        nrow(arbitrage_cases_dt) > 0,
        !anyNA(arbitrage_cases_dt$usdt_delta_refined),
        arbitrage_cases_dt$usdt_delta_refined >= 0
      )
      
      
      ## Set arbitrage cases
      arbitrage_cases_dt %<>% 
        filter(direction == 1) %>% 
        mutate(
          target = if_else(usdt_delta_refined > job$config$risks$min_order_volume_in_usdt, T, F) 
          #target = if_else(target & usdt_delta_refined < job$config$risks$max_order_volume_in_usdt, T, F), # TODO: add max volume restiction
        )
      
      stopifnot(
        nrow(arbitrage_cases_dt) > 0,
        !anyNA(arbitrage_cases_dt)
      )
      
      # DEBUG ONLY
      #print(
      #  arbitrage_cases_dt %>% 
      #    filter(target) %>% 
      #    sample_n(10) %>% 
      #    select(timestamp_round, symbol, exchange, direction, price, volume, best_bid, best_ask, market_volume, usdt_delta_refined) %>% 
      #    arrange(timestamp_round) %>% 
      #    as_tibble
      #)
      
      arbitrage_cases_dt %>%
        filter(target) %>% 
        
        ggplot(aes(x = exchange, y = usdt_delta_refined, color = exchange)) +
          geom_violin() +
          scale_y_log10()
      
      ggsave(
        filename = sprintf("arbitrage-distibution.%s.%s.png", str_replace(.settings$symbol, "/", "-"), Sys.Date()), path = "output",
        plot = last_plot(),
        width = 1920/320, height = 1280/320, 
        units = "in", dpi = 320
      )
      
      
      ## Calc and return final stats
      
      arbitrage_cases_dt$exchange_name <- lapply(arbitrage_cases_dt$exchange, get_name_by_key, .dictionary = exchanges_list)
      
      arbitrage_cases_dt %>% 
        mutate(
          exchange = factor(exchange, levels = exchanges_list, labels = names(exchanges_list)),
          timestamp_1h = floor_date(timestamp, "1 hours")
        ) %>% 
        
        group_by(
          timestamp_1h, symbol, exchange
        ) %>% 
        summarise(
          n = n(),
          total_volume = sum(price*volume*trading_pair_to_usdt_rate),
          total_usdt_delta = max(usdt_delta_refined),
          .groups = "drop"
        )
      
      
      # DEBUG ONLY
      #View(
      #  orderbooks_dt %>% 
      #    #filter(to_timestamp(timestamp) == 1610211368)
      #    sample_n(1) %>% 
      #    inner_join(
      #      arbitrage_cases_dt %>% select(timestamp, symbol),
      #      by = c("timestamp", "symbol")
      #    )
      #)

    })


stopifnot(
  nrow(arbitrage_stats) > 0,
  jobs_settings[not_usdt_indx] %>% map_chr(~ .x$symbol) %in% arbitrage_stats$symbol %>% unique,
  arbitrage_stats %>% group_by(symbol) %>% filter(n() < 1) %>% nrow == 0
)

arbitrage_stats



ggplot(arbitrage_stats) +
    geom_point(
      aes(y = total_usdt_delta, x = timestamp_1h, size = total_usdt_delta),
      alpha = .4
    ) +
    
    scale_x_datetime(date_breaks = "6 hours", date_labels = "%H:%M") +
    scale_y_log10() +
    
    facet_grid(symbol ~ exchange, scales = "free") +
    
    labs(
      title = "Arbitrage Volume",
      subtitle = sprintf("From %s to %s", min(arbitrage_stats$timestamp_1h), max(arbitrage_stats$timestamp_1h)),
      x = "", y = "Total Volume, USDT",
      caption = "(c) 2021, github.com/dkzkv/ArbitralSystem."
    ) +
  
    theme(
      legend.position = "top"
    )


ggsave(
  filename = sprintf("arbitrage_cases.%s.png", Sys.Date()), path = "output",
  plot = last_plot(),
  width = 1920/320, height = 400*n_unique(arbitrage_stats$symbol)/320, 
  units = "in", dpi = 320, 
  limitsize = F
)



# Estimate arbitrage volume ----


