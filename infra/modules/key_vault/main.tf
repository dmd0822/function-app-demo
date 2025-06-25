data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "kv" {
  name                        = var.key_vault_name
  location                    = var.location
  resource_group_name         = var.resource_group_name
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  sku_name                    = "standard"
  purge_protection_enabled    = false
  enable_rbac_authorization   = true # Enable RBAC for secret management
}

# Grant the Terraform service principal RBAC access to manage secrets
resource "azurerm_role_assignment" "tf_sp_kv_secrets" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azurerm_client_config.current.object_id
}

# Grant the Function App's managed identity RBAC access to get secrets
resource "azurerm_role_assignment" "function_app_kv_secrets" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = var.function_app_identity_object_id
}

resource "azurerm_key_vault_secret" "storage_account_name" {
  name         = "storage-account-name"
  value        = var.storage_account_name
  key_vault_id = azurerm_key_vault.kv.id
  depends_on = [
    azurerm_role_assignment.tf_sp_kv_secrets,
    azurerm_role_assignment.function_app_kv_secrets
  ]
}
