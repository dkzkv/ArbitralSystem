
#'
#' Obsolete
#'



##

heartbeat_plot <- data %>% 
  #filter(symbol %in% unique(books$symbol)[1:3]) %>% 
  
  filter(direction == 0) %>% 
  group_by(symbol, exchange, timestamp_round) %>% 
  summarise(best_bid = max(price)) %>% 
  
  ggplot(aes(x = timestamp_round, color = exchange)) +
    geom_line(aes(y = best_bid)) +
    facet_grid(symbol ~ ., scales = "free")


png(
  sprintf("output/heartbeat.%s.png", Sys.Date()),
  width = 1920, height = 100*n_unique(books$symbol), units = "px"
)
print(heartbeat_plot)
dev.off()



## 


candles_exchanges <- books %>% 
  group_by(
    exchange, symbol, direction, timestamp_round
  ) %>% 
  get_candle_1m


one_vs_all <- candles_exchanges %>% 
  ungroup %>% 
  select(timestamp_round, exchange, best_bid, best_ask) %>% 
  pivot_wider(names_from = exchange, values_from = c(best_bid, best_ask))

one_vs_all


names(one_vs_all)


ggplot(one_vs_all %>% filter(timestamp_round > ymd("2020-10-11")), aes(x = timestamp_round)) +
  geom_line(aes(y = best_ask_1), color = "red") +
  
  geom_line(aes(y = best_bid_3), color = "green") +
  geom_line(aes(y = best_bid_4), color = "blue") +
  geom_line(aes(y = best_bid_6), color = "black")

