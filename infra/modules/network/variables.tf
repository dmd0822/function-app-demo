variable "location" {
  description = "Azure region"
  type        = string
}

variable "resource_group_name" {
  description = "Resource group name"
  type        = string
}

variable "vnet_name" {
  description = "Virtual network name"
  type        = string
  default     = "vnet-func-demo"
}

variable "private_endpoints_subnet_name" {
  description = "Subnet for private endpoints"
  type        = string
  default     = "snet-private-endpoints"
}

variable "workload_subnet_name" {
  description = "Subnet for workload"
  type        = string
  default     = "snet-workload"
}

variable "key_vault_id" {
  description = "Key Vault resource ID"
  type        = string
}

variable "function_app_id" {
  description = "Function App resource ID"
  type        = string
}

variable "storage_account_id" {
  description = "Storage Account resource ID"
  type        = string
}
