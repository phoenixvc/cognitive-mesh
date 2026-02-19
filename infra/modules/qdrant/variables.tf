###############################################################################
# Cognitive Mesh — Qdrant Module Variables
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

variable "qdrant_image" {
  description = "Docker image for Qdrant."
  type        = string
  default     = "qdrant/qdrant:v1.12.5"
}

variable "cpu_cores" {
  description = "Number of CPU cores for the Qdrant container."
  type        = number
  default     = 1
}

variable "memory_gb" {
  description = "Memory in GB for the Qdrant container."
  type        = number
  default     = 2
}

variable "ip_address_type" {
  description = "IP address type for the container group (Public or Private)."
  type        = string
  default     = "Private"
}

variable "subnet_ids" {
  description = "List of subnet IDs for private networking."
  type        = list(string)
  default     = []
}

variable "use_persistent_storage" {
  description = "Whether to use persistent Azure File Share storage."
  type        = bool
  default     = false
}

variable "storage_account_name" {
  description = "Storage account name for persistent volume (required if use_persistent_storage=true)."
  type        = string
  default     = null
}

variable "storage_account_key" {
  description = "Storage account key for persistent volume (required if use_persistent_storage=true)."
  type        = string
  default     = null
  sensitive   = true
}

variable "file_share_name" {
  description = "Azure File Share name for persistent volume."
  type        = string
  default     = "qdrant-data"
}

variable "common_tags" {
  description = "Common tags applied to all resources."
  type        = map(string)
  default     = {}
}
