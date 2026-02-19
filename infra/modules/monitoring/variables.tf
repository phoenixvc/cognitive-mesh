###############################################################################
# Cognitive Mesh — Monitoring Module Variables
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

variable "log_analytics_sku" {
  description = "SKU for the Log Analytics Workspace."
  type        = string
  default     = "PerGB2018"
}

variable "retention_in_days" {
  description = "Log retention in days for Log Analytics Workspace."
  type        = number
  default     = 30
}

variable "daily_quota_gb" {
  description = "Daily data ingestion quota in GB (-1 for unlimited)."
  type        = number
  default     = -1
}

variable "appinsights_retention_days" {
  description = "Application Insights data retention in days."
  type        = number
  default     = 90
}

variable "sampling_percentage" {
  description = "Percentage of telemetry data to sample (0-100)."
  type        = number
  default     = 100
}

variable "common_tags" {
  description = "Common tags applied to all resources."
  type        = map(string)
  default     = {}
}
