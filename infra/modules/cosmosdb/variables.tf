###############################################################################
# Cognitive Mesh — CosmosDB Module Variables
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

variable "database_name" {
  description = "Name of the Cosmos DB SQL database."
  type        = string
  default     = "cognitive-mesh-db"
}

variable "consistency_level" {
  description = "The consistency level for the Cosmos DB account."
  type        = string
  default     = "Session"
  validation {
    condition     = contains(["BoundedStaleness", "Eventual", "Session", "Strong", "ConsistentPrefix"], var.consistency_level)
    error_message = "Must be one of: BoundedStaleness, Eventual, Session, Strong, ConsistentPrefix."
  }
}

variable "max_staleness_interval" {
  description = "Max staleness interval in seconds (only for BoundedStaleness)."
  type        = number
  default     = 5
}

variable "max_staleness_prefix" {
  description = "Max staleness prefix (only for BoundedStaleness)."
  type        = number
  default     = 100
}

variable "enable_automatic_failover" {
  description = "Enable automatic failover for the Cosmos DB account."
  type        = bool
  default     = false
}

variable "secondary_locations" {
  description = "List of secondary geo-locations for replication."
  type = list(object({
    location          = string
    failover_priority = number
  }))
  default = []
}

variable "containers" {
  description = "Map of Cosmos DB SQL containers to create."
  type = map(object({
    partition_key_paths = list(string)
    default_ttl         = optional(number, -1)
  }))
  default = {
    "workflows" = {
      partition_key_paths = ["/tenantId"]
    }
    "checkpoints" = {
      partition_key_paths = ["/workflowId"]
    }
    "agents" = {
      partition_key_paths = ["/agentType"]
    }
    "reasoning-sessions" = {
      partition_key_paths = ["/sessionId"]
    }
  }
}

variable "common_tags" {
  description = "Common tags applied to all resources."
  type        = map(string)
  default     = {}
}
