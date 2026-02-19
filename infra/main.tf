###############################################################################
# Cognitive Mesh — Root Module
# Orchestrates all infrastructure sub-modules.
###############################################################################

locals {
  tags = merge(var.common_tags, {
    Environment = var.environment
  })
}

# ---------- Resource Group ----------

resource "azurerm_resource_group" "this" {
  name     = var.resource_group_name
  location = var.location

  tags = local.tags
}

# ---------- Networking ----------

module "networking" {
  source = "./modules/networking"

  project_name        = var.project_name
  environment         = var.environment
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  vnet_address_space  = var.vnet_address_space
  common_tags         = local.tags
}

# ---------- Monitoring (deploy early — other modules reference Log Analytics) ----------

module "monitoring" {
  source = "./modules/monitoring"

  project_name        = var.project_name
  environment         = var.environment
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  retention_in_days   = var.log_retention_days
  appinsights_retention_days = var.appinsights_retention_days
  common_tags         = local.tags
}

# ---------- Key Vault ----------

module "keyvault" {
  source = "./modules/keyvault"

  project_name        = var.project_name
  environment         = var.environment
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  common_tags         = local.tags
}

# ---------- Storage ----------

module "storage" {
  source = "./modules/storage"

  project_name        = var.project_name
  environment         = var.environment
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  common_tags         = local.tags
}

# ---------- CosmosDB ----------

module "cosmosdb" {
  source = "./modules/cosmosdb"

  project_name        = var.project_name
  environment         = var.environment
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  database_name       = var.cosmosdb_database_name
  consistency_level   = var.cosmosdb_consistency_level
  common_tags         = local.tags
}

# ---------- Redis ----------

module "redis" {
  source = "./modules/redis"

  project_name        = var.project_name
  environment         = var.environment
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  prod_sku_name       = var.redis_prod_sku_name
  prod_capacity       = var.redis_prod_capacity
  common_tags         = local.tags
}

# ---------- Qdrant (Vector DB) ----------

module "qdrant" {
  source = "./modules/qdrant"

  project_name        = var.project_name
  environment         = var.environment
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  qdrant_image        = var.qdrant_image
  cpu_cores           = var.qdrant_cpu_cores
  memory_gb           = var.qdrant_memory_gb
  subnet_ids          = [module.networking.subnet_ids["containers"]]
  ip_address_type     = "Private"
  common_tags         = local.tags
}

# ---------- Azure OpenAI ----------

module "openai" {
  source = "./modules/openai"

  project_name        = var.project_name
  environment         = var.environment
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  model_deployments   = var.openai_model_deployments
  common_tags         = local.tags
}

# ---------- AI Search ----------

module "ai_search" {
  source = "./modules/ai-search"

  project_name        = var.project_name
  environment         = var.environment
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  sku                 = var.search_sku
  common_tags         = local.tags
}

# ---------- Store secrets in Key Vault ----------

resource "azurerm_key_vault_secret" "cosmosdb_connection" {
  name         = "cosmosdb-connection-string"
  value        = module.cosmosdb.connection_strings[0]
  key_vault_id = module.keyvault.key_vault_id

  depends_on = [module.keyvault]
}

resource "azurerm_key_vault_secret" "redis_connection" {
  name         = "redis-connection-string"
  value        = module.redis.primary_connection_string
  key_vault_id = module.keyvault.key_vault_id

  depends_on = [module.keyvault]
}

resource "azurerm_key_vault_secret" "storage_connection" {
  name         = "storage-connection-string"
  value        = module.storage.primary_connection_string
  key_vault_id = module.keyvault.key_vault_id

  depends_on = [module.keyvault]
}

resource "azurerm_key_vault_secret" "openai_key" {
  name         = "openai-api-key"
  value        = module.openai.primary_access_key
  key_vault_id = module.keyvault.key_vault_id

  depends_on = [module.keyvault]
}

resource "azurerm_key_vault_secret" "appinsights_connection" {
  name         = "appinsights-connection-string"
  value        = module.monitoring.application_insights_connection_string
  key_vault_id = module.keyvault.key_vault_id

  depends_on = [module.keyvault]
}

resource "azurerm_key_vault_secret" "search_key" {
  name         = "search-admin-key"
  value        = module.ai_search.primary_key
  key_vault_id = module.keyvault.key_vault_id

  depends_on = [module.keyvault]
}
