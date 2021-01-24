

azuremlsdk::install_azureml()

library(azuremlsdk)



ws <- load_workspace_from_config(file_name = "aml.secrets.json")
ws


exp <- experiment(ws, "crypto-cerberus-experiment")

run <- start_logging_run(exp)

log_
log_metric_to_run("Accuracy", 0.9)

complete_run(run)


