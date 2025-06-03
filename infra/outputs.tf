output "function_app_id" {
  value = module.function_app.function_app_id
}
output "app_service_plan_id" {
  value = module.app_service_plan.app_service_plan_id
}
output "application_insights_instrumentation_key" {
  value = module.application_insights.instrumentation_key
}
output "key_vault_id" {
  value = module.key_vault.key_vault_id
}