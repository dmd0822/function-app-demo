output "function_app_id" {
  value = module.function_app.function_app_id
}
output "app_service_plan_id" {
  value = module.app_service_plan.app_service_plan_id
}
output "application_insights_instrumentation_key" {
  value     = module.application_insights.instrumentation_key
  sensitive = true
}
output "key_vault_id" {
  value = module.key_vault.key_vault_id
}

output "vnet_id" {
  value = module.network.vnet_id
}

output "private_endpoints_subnet_id" {
  value = module.network.private_endpoints_subnet_id
}

output "workload_subnet_id" {
  value = module.network.workload_subnet_id
}

output "storage_account_id" {
  value = module.function_app.storage_account_id
}