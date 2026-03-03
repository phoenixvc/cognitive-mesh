###############################################################################
# Cognitive Mesh — Networking Module Variables
###############################################################################

variable "project_name" {
  description = "Name of the project, used as a prefix for resource naming."
  type        = string
}

variable "environment" {
  description = "Deployment environment (dev, staging, prod)."
  type        = string
  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be one of: dev, staging, prod."
  }
}

variable "location" {
  description = "Azure region for resource deployment."
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group to deploy into."
  type        = string
}

variable "vnet_address_space" {
  description = "Address space for the virtual network."
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

variable "subnets" {
  description = "Map of subnets to create within the VNet."
  type = map(object({
    address_prefixes  = list(string)
    service_endpoints = optional(list(string), [])
    create_nsg        = optional(bool, true)
    delegation = optional(object({
      name         = string
      service_name = string
      actions      = list(string)
    }), null)
  }))
  default = {
    "app" = {
      address_prefixes  = ["10.0.1.0/24"]
      service_endpoints = ["Microsoft.KeyVault", "Microsoft.Storage", "Microsoft.AzureCosmosDB"]
    }
    "data" = {
      address_prefixes  = ["10.0.2.0/24"]
      service_endpoints = ["Microsoft.Storage", "Microsoft.AzureCosmosDB"]
    }
    "containers" = {
      address_prefixes  = ["10.0.3.0/24"]
      service_endpoints = []
      delegation = {
        name         = "aci-delegation"
        service_name = "Microsoft.ContainerInstance/containerGroups"
        actions      = ["Microsoft.Network/virtualNetworks/subnets/action"]
      }
    }
    "private-endpoints" = {
      address_prefixes = ["10.0.4.0/24"]
      create_nsg       = false
    }
  }
}

variable "private_dns_zones" {
  description = "Map of private DNS zones to create (key = logical name, value = zone FQDN)."
  type        = map(string)
  default = {
    "cosmosdb" = "privatelink.documents.azure.com"
    "keyvault" = "privatelink.vaultcore.azure.net"
    "storage"  = "privatelink.blob.core.windows.net"
    "redis"    = "privatelink.redis.cache.windows.net"
    "search"   = "privatelink.search.windows.net"
    "openai"   = "privatelink.openai.azure.com"
  }
}

variable "private_endpoints" {
  description = "Map of private endpoints to create."
  type = map(object({
    subnet_key        = string
    resource_id       = string
    subresource_names = list(string)
    dns_zone_key      = optional(string, null)
  }))
  default = {}
}

variable "common_tags" {
  description = "Common tags applied to all resources."
  type        = map(string)
  default     = {}
}
