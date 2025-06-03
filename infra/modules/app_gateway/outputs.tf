output "app_gateway_public_ip" {
  value = azurerm_public_ip.appgw.ip_address
}

output "app_gateway_id" {
  value = azurerm_application_gateway.appgw.id
}
