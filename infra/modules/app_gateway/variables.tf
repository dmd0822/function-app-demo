variable "location" {
  description = "Azure region"
  type        = string
}

variable "resource_group_name" {
  description = "Resource group name"
  type        = string
}

variable "public_ip_name" {
  description = "Name for the public IP"
  type        = string
  default     = "pip-appgw"
}

variable "app_gateway_name" {
  description = "Name for the Application Gateway"
  type        = string
  default     = "agw-func-demo"
}

variable "subnet_id" {
  description = "Subnet ID for the Application Gateway"
  type        = string
}

variable "backend_fqdn" {
  description = "FQDN or private IP of the backend (Function App private endpoint)"
  type        = string
}
