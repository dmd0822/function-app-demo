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

  identity {
    type = "SystemAssigned"
  }
  site_config {
    application_insights_key = var.application_insights_instrumentation_key
    application_stack {
      dotnet_version = "8.0"
      use_dotnet_isolated_runtime = true 
    }
    scm_ip_restriction {
      action   = "Allow"
      priority = 100
      name     = "AllowAll"
      ip_address = "0.0.0.0/0"
    }
    cors {
      allowed_origins = ["*"]
      support_credentials = false
    }
  }
  app_settings = {
    "AzureWebJobsStorage__accountName" = azurerm_storage_account.sa.name
    "AzureWebJobsStorage__credential"  = "managedidentity"
    "FUNCTIONS_EXTENSION_VERSION"      = "~4"
    "FUNCTIONS_WORKER_RUNTIME"         = "dotnet-isolated"
    "STORAGE_ACCOUNT_NAME"             = "@Microsoft.KeyVault(SecretUri=${var.key_vault_uri}secrets/storage-account-name)"
  }
}
