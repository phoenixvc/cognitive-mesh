###############################################################################
# Cognitive Mesh — Root Module Variables
###############################################################################

# ---------- General ----------

variable "project_name" {
  description = "Name of the project, used as a prefix for all resource names."
  type        = string
  default     = "cognitive-mesh"
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
  description = "Primary Azure region for resource deployment."
  type        = string
  default     = "westeurope"
}

variable "resource_group_name" {
  description = "Name of the resource group to deploy all resources into."
  type        = string
}

variable "common_tags" {
  description = "Common tags applied to all resources."
  type        = map(string)
  default = {
    Project   = "CognitiveMesh"
    ManagedBy = "terraform"
  }
}

# ---------- CosmosDB ----------

variable "cosmosdb_consistency_level" {
  description = "Consistency level for the Cosmos DB account."
  type        = string
  default     = "Session"
}

variable "cosmosdb_database_name" {
  description = "Name of the Cosmos DB SQL database."
  type        = string
  default     = "cognitive-mesh-db"
}

# ---------- Redis ----------

variable "redis_prod_sku_name" {
  description = "Redis SKU for staging/prod environments."
  type        = string
  default     = "Standard"
}

variable "redis_prod_capacity" {
  description = "Redis capacity for staging/prod environments."
  type        = number
  default     = 1
}

# ---------- Qdrant ----------

variable "qdrant_cpu_cores" {
  description = "CPU cores for the Qdrant container."
  type        = number
  default     = 1
}

variable "qdrant_memory_gb" {
  description = "Memory in GB for the Qdrant container."
  type        = number
  default     = 2
}

variable "qdrant_image" {
  description = "Docker image for Qdrant."
  type        = string
  default     = "qdrant/qdrant:v1.12.5"
}

# ---------- OpenAI ----------

variable "openai_model_deployments" {
  description = "Map of Azure OpenAI model deployments."
  type = map(object({
    model_name    = string
    model_version = string
    sku_name      = optional(string, "Standard")
    sku_capacity  = optional(number, 10)
  }))
  default = {
    "gpt-4o" = {
      model_name    = "gpt-4o"
      model_version = "2024-11-20"
      sku_name      = "GlobalStandard"
      sku_capacity  = 10
    }
    "text-embedding-3-large" = {
      model_name    = "text-embedding-3-large"
      model_version = "1"
      sku_name      = "Standard"
      sku_capacity  = 10
    }
  }
}

# ---------- AI Search ----------

variable "search_sku" {
  description = "SKU tier for Azure AI Search."
  type        = string
  default     = "basic"
}

# ---------- Monitoring ----------

variable "log_retention_days" {
  description = "Log Analytics data retention in days."
  type        = number
  default     = 30
}

variable "appinsights_retention_days" {
  description = "Application Insights data retention in days."
  type        = number
  default     = 90
}

# ---------- Networking ----------

variable "vnet_address_space" {
  description = "Address space for the virtual network."
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

variable "enable_private_endpoints" {
  description = "Whether to create private endpoints for services."
  type        = bool
  default     = false
}
