variable "resource_group_name" {
  description = "Resource group name"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "key_vault_name" {
  description = "Key Vault name"
  type        = string
}

variable "storage_account_name" {
  description = "Storage account name to store as a secret in Key Vault"
  type        = string
}

variable "function_app_identity_object_id" {
  description = "Object ID of the Function App's managed identity"
  type        = string
}
