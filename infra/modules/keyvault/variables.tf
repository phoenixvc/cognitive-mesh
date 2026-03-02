###############################################################################
# Cognitive Mesh — Key Vault Module Variables
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

variable "sku_name" {
  description = "SKU name for the Key Vault (standard or premium)."
  type        = string
  default     = "standard"
  validation {
    condition     = contains(["standard", "premium"], var.sku_name)
    error_message = "Must be one of: standard, premium."
  }
}

variable "soft_delete_retention_days" {
  description = "Number of days to retain soft-deleted vaults."
  type        = number
  default     = 90
}

variable "purge_protection_enabled" {
  description = "Enable purge protection to prevent permanent deletion."
  type        = bool
  default     = true
}

variable "enabled_for_deployment" {
  description = "Allow Azure VMs to retrieve certificates."
  type        = bool
  default     = false
}

variable "enabled_for_disk_encryption" {
  description = "Allow Azure Disk Encryption to retrieve secrets."
  type        = bool
  default     = false
}

variable "enabled_for_template_deployment" {
  description = "Allow ARM templates to retrieve secrets."
  type        = bool
  default     = false
}

variable "network_default_action" {
  description = "Default network action (Allow or Deny)."
  type        = string
  default     = "Allow"
}

variable "allowed_ip_ranges" {
  description = "List of allowed IP ranges for network ACL."
  type        = list(string)
  default     = []
}

variable "allowed_subnet_ids" {
  description = "List of allowed subnet IDs for network ACL."
  type        = list(string)
  default     = []
}

variable "access_policies" {
  description = "Map of additional access policies to create."
  type = map(object({
    object_id               = string
    key_permissions         = optional(list(string), ["Get", "List"])
    secret_permissions      = optional(list(string), ["Get", "List"])
    certificate_permissions = optional(list(string), ["Get", "List"])
  }))
  default = {}
}

variable "common_tags" {
  description = "Common tags applied to all resources."
  type        = map(string)
  default     = {}
}
