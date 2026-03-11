###############################################################################
# Cognitive Mesh — Frontend Hosting Module Variables
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

variable "app_service_plan_sku" {
  description = "SKU for the App Service Plan. B1 for dev, S1 for prod."
  type        = string
  default     = "B1"
}

variable "node_version" {
  description = "Node.js runtime version for the App Service."
  type        = string
  default     = "22-lts"
}

variable "api_base_url" {
  description = "Base URL for the backend API (used as NEXT_PUBLIC_API_BASE_URL)."
  type        = string
}

variable "custom_domain" {
  description = "Custom domain name for the frontend. Set to null to skip."
  type        = string
  default     = null
}

variable "health_check_path" {
  description = "Path for the App Service health check."
  type        = string
  default     = "/"
}

variable "always_on" {
  description = "Whether the App Service should be always on."
  type        = bool
  default     = false
}

variable "subnet_id" {
  description = "Subnet ID for VNet integration. Set to null to skip VNet integration."
  type        = string
  default     = null
}

variable "common_tags" {
  description = "Common tags applied to all resources."
  type        = map(string)
  default     = {}
}
