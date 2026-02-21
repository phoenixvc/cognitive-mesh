###############################################################################
# Cognitive Mesh — Redis Module Variables
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

variable "prod_sku_name" {
  description = "SKU name for staging/prod environments."
  type        = string
  default     = "Standard"
}

variable "prod_family" {
  description = "Redis family for staging/prod environments."
  type        = string
  default     = "C"
}

variable "prod_capacity" {
  description = "Redis capacity for staging/prod environments."
  type        = number
  default     = 1
}

variable "maxmemory_policy" {
  description = "Max memory eviction policy."
  type        = string
  default     = "allkeys-lru"
}

variable "common_tags" {
  description = "Common tags applied to all resources."
  type        = map(string)
  default     = {}
}
