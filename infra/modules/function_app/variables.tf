variable "resource_group_name" {
  description = "Resource group name"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "function_app_name" {
  description = "Function App name"
  type        = string
}

variable "app_service_plan_id" {
  description = "App Service Plan resource ID"
  type        = string
}

variable "application_insights_instrumentation_key" {
  description = "Instrumentation key for Application Insights"
  type        = string
}

variable "key_vault_id" {
  description = "Key Vault resource ID"
  type        = string
}
