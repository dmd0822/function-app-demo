output "vnet_id" {
  value = azurerm_virtual_network.main.id
}

output "private_endpoints_subnet_id" {
  value = azurerm_subnet.private_endpoints.id
}

output "workload_subnet_id" {
  value = azurerm_subnet.workload.id
}

output "app_gateway_public_ip" {
  value = azurerm_public_ip.appgw.ip_address
}

output "app_gateway_id" {
  value = azurerm_application_gateway.appgw.id
}
