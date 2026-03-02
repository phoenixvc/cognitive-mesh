###############################################################################
# Cognitive Mesh — Dev Environment Terragrunt Configuration
###############################################################################

include "root" {
  path = find_in_parent_folders()
}

terraform {
  source = "${get_parent_terragrunt_dir()}//"
}

inputs = {
  environment         = "dev"
  location            = "westeurope"
  resource_group_name = "cognitive-mesh-dev-rg"

  # CosmosDB — dev settings
  cosmosdb_consistency_level = "Session"

  # Redis — dev uses Basic C0 (auto-selected by module)

  # Qdrant — smaller container for dev
  qdrant_cpu_cores = 1
  qdrant_memory_gb = 2

  # OpenAI — lower capacity for dev
  openai_model_deployments = {
    "gpt-4o" = {
      model_name    = "gpt-4o"
      model_version = "2024-11-20"
      sku_name      = "GlobalStandard"
      sku_capacity  = 5
    }
    "text-embedding-3-large" = {
      model_name    = "text-embedding-3-large"
      model_version = "1"
      sku_name      = "Standard"
      sku_capacity  = 5
    }
  }

  # AI Search — free tier for dev
  search_sku = "free"

  # Monitoring — shorter retention for dev
  log_retention_days         = 30
  appinsights_retention_days = 30

  # Networking — smaller address space for dev
  vnet_address_space = ["10.0.0.0/16"]

  common_tags = {
    Project     = "CognitiveMesh"
    Environment = "dev"
    ManagedBy   = "terraform"
  }
}
