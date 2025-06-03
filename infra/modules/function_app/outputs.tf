output "function_app_id" {
  value = azurerm_linux_function_app.fa.id
}

output "function_app_name" {
  value = azurerm_linux_function_app.fa.name
}

output "function_app_default_hostname" {
  value = azurerm_linux_function_app.fa.default_hostname
}