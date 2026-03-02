###############################################################################
# Cognitive Mesh — AI Search Module Variables
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

variable "sku" {
  description = "SKU tier for the search service (free, basic, standard, standard2, standard3)."
  type        = string
  default     = "basic"
  validation {
    condition     = contains(["free", "basic", "standard", "standard2", "standard3", "storage_optimized_l1", "storage_optimized_l2"], var.sku)
    error_message = "Must be one of: free, basic, standard, standard2, standard3, storage_optimized_l1, storage_optimized_l2."
  }
}

variable "replica_count" {
  description = "Number of replicas (1-12 depending on SKU)."
  type        = number
  default     = 1
}

variable "partition_count" {
  description = "Number of partitions (1, 2, 3, 4, 6, or 12)."
  type        = number
  default     = 1
}

variable "public_network_access_enabled" {
  description = "Whether public network access is enabled."
  type        = bool
  default     = true
}

variable "local_authentication_enabled" {
  description = "Whether API key authentication is enabled."
  type        = bool
  default     = true
}

variable "common_tags" {
  description = "Common tags applied to all resources."
  type        = map(string)
  default     = {}
}
