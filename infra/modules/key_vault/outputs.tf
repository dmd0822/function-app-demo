output "key_vault_uri" {
  value = azurerm_key_vault.kv.vault_uri
}

output "key_vault_id" {
  value = azurerm_key_vault.kv.id
  sensitive = true
}