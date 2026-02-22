###############################################################################
# Cognitive Mesh — Storage Module Variables
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

variable "account_tier" {
  description = "Performance tier of the storage account (Standard or Premium)."
  type        = string
  default     = "Standard"
}

variable "replication_type" {
  description = "Replication type for the storage account."
  type        = string
  default     = "LRS"
  validation {
    condition     = contains(["LRS", "GRS", "RAGRS", "ZRS", "GZRS", "RAGZRS"], var.replication_type)
    error_message = "Must be one of: LRS, GRS, RAGRS, ZRS, GZRS, RAGZRS."
  }
}

variable "enable_versioning" {
  description = "Enable blob versioning."
  type        = bool
  default     = true
}

variable "soft_delete_retention_days" {
  description = "Number of days to retain soft-deleted blobs."
  type        = number
  default     = 7
}

variable "containers" {
  description = "Map of storage containers to create."
  type = map(object({
    access_type = optional(string, "private")
  }))
  default = {
    "workflow-checkpoints" = {
      access_type = "private"
    }
    "agent-artifacts" = {
      access_type = "private"
    }
    "reasoning-outputs" = {
      access_type = "private"
    }
    "tfstate" = {
      access_type = "private"
    }
  }
}

variable "common_tags" {
  description = "Common tags applied to all resources."
  type        = map(string)
  default     = {}
}
