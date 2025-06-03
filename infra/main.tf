provider "azurerm" {
  features {}
  subscription_id = var.subscription_id
}

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 3.0.0"
    }
  } 
}

data "azurerm_resource_group" "main" {
  name = var.resource_group_name
}

module "app_service_plan" {
  source              = "./modules/app_service_plan"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  app_service_plan_name = var.app_service_plan_name
}

module "application_insights" {
  source                   = "./modules/application_insights"
  resource_group_name      = data.azurerm_resource_group.main.name
  location                 = data.azurerm_resource_group.main.location
  application_insights_name = var.application_insights_name
}

module "key_vault" {
  source              = "./modules/key_vault"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  key_vault_name      = var.key_vault_name
}

module "function_app" {
  source                = "./modules/function_app"
  resource_group_name   = data.azurerm_resource_group.main.name
  location              = data.azurerm_resource_group.main.location
  function_app_name     = var.function_app_name
  app_service_plan_id   = module.app_service_plan.app_service_plan_id
  application_insights_instrumentation_key = module.application_insights.instrumentation_key
  key_vault_id         = module.key_vault.key_vault_id
}

module "network" {
  source = "./modules/network"
  location = data.azurerm_resource_group.main.location
  resource_group_name = data.azurerm_resource_group.main.name
  key_vault_id = module.key_vault.key_vault_id
  function_app_id = module.function_app.function_app_id
  storage_account_id = module.function_app.storage_account_id
}

module "app_gateway" {
  source              = "./modules/app_gateway"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = module.network.workload_subnet_id
  backend_fqdn        = module.network.function_app_private_ip
}
