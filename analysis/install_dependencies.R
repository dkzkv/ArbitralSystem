
#'
#' Install R packages
#'



#' Install packages if its not installed yet
#'
#' @param x A character vector of installing packages
#' @param repos A repositories to use
#' 
#' @return The result of installation
#' 
packages_install <- function(x, repos = getOption("repos")) {
  stopifnot(is.character(x))
  
  missing_packages <- x[!(x %in% installed.packages()[, "Package"])]
  
  if (length(missing_packages) > 0)
  {
    install.packages(missing_packages, repos = repos)
    print(paste("Successfully installed packages:", paste(missing_packages, collapse = ", ")))
  } else {
    print("All packages have already installed.")
  }
}


local({
  # packages list
  packages <- c(
    "odbc", # data retrieve
    "data.table", "dplyr", "purrr", "tidyr", "magrittr", # data transform
    "stringr", "lubridate", # data processing
    "skimr", # descriptive stats and EDA
    "ggplot2", # visualization
    "config", # tools
    "azuremlsdk" # SDKs
  )
  
  # view packages repository 
  print(getOption("repos"))
  
  # install packages
  packages_install(packages)
})


gc()

