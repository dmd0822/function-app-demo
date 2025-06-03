output "vnet_id" {
  value = azurerm_virtual_network.main.id
}

output "private_endpoints_subnet_id" {
  value = azurerm_subnet.private_endpoints.id
}

output "workload_subnet_id" {
  value = azurerm_subnet.workload.id
}

output "function_app_private_ip" {
  value = azurerm_private_endpoint.function_app.private_service_connection[0].private_ip_address
}
