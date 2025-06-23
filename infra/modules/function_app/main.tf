resource "azurerm_storage_account" "sa" {
  name                     = substr(replace(var.function_app_name, "-", ""), 0, 24)
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_linux_function_app" "fa" {
  name                       = var.function_app_name
  location                   = var.location
  resource_group_name        = var.resource_group_name
  service_plan_id            = var.app_service_plan_id
  storage_account_name       = azurerm_storage_account.sa.name
  storage_account_access_key = azurerm_storage_account.sa.primary_access_key
  site_config {
    application_insights_key = var.application_insights_instrumentation_key
s    application_stack {
      python_version = "3.11"
    }
  }
  app_settings = {
    FUNCTIONS_WORKER_RUNTIME = "python"
  }
}
