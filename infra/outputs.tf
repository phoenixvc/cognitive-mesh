###############################################################################
# Cognitive Mesh — Root Module Outputs
###############################################################################

# ---------- Resource Group ----------

output "resource_group_name" {
  description = "The name of the resource group."
  value       = azurerm_resource_group.this.name
}

output "resource_group_id" {
  description = "The ID of the resource group."
  value       = azurerm_resource_group.this.id
}

# ---------- Networking ----------

output "vnet_id" {
  description = "The ID of the virtual network."
  value       = module.networking.vnet_id
}

output "subnet_ids" {
  description = "Map of subnet names to their IDs."
  value       = module.networking.subnet_ids
}

# ---------- CosmosDB ----------

output "cosmosdb_endpoint" {
  description = "The Cosmos DB account endpoint."
  value       = module.cosmosdb.account_endpoint
}

output "cosmosdb_database_name" {
  description = "The Cosmos DB database name."
  value       = module.cosmosdb.database_name
}

# ---------- Storage ----------

output "storage_account_name" {
  description = "The storage account name."
  value       = module.storage.storage_account_name
}

output "storage_blob_endpoint" {
  description = "The primary blob endpoint."
  value       = module.storage.primary_blob_endpoint
}

# ---------- Redis ----------

output "redis_hostname" {
  description = "The Redis cache hostname."
  value       = module.redis.hostname
}

output "redis_ssl_port" {
  description = "The Redis cache SSL port."
  value       = module.redis.ssl_port
}

# ---------- Qdrant ----------

output "qdrant_http_endpoint" {
  description = "The Qdrant REST API endpoint."
  value       = module.qdrant.http_endpoint
}

output "qdrant_grpc_endpoint" {
  description = "The Qdrant gRPC endpoint."
  value       = module.qdrant.grpc_endpoint
}

# ---------- Azure OpenAI ----------

output "openai_endpoint" {
  description = "The Azure OpenAI endpoint."
  value       = module.openai.endpoint
}

output "openai_deployment_ids" {
  description = "Map of OpenAI deployment names to IDs."
  value       = module.openai.deployment_ids
}

# ---------- AI Search ----------

output "search_service_name" {
  description = "The AI Search service name."
  value       = module.ai_search.search_service_name
}

# ---------- Key Vault ----------

output "key_vault_uri" {
  description = "The Key Vault URI."
  value       = module.keyvault.key_vault_uri
}

output "key_vault_name" {
  description = "The Key Vault name."
  value       = module.keyvault.key_vault_name
}

# ---------- Monitoring ----------

output "application_insights_connection_string" {
  description = "The Application Insights connection string."
  value       = module.monitoring.application_insights_connection_string
  sensitive   = true
}

output "application_insights_instrumentation_key" {
  description = "The Application Insights instrumentation key."
  value       = module.monitoring.application_insights_instrumentation_key
  sensitive   = true
}

output "log_analytics_workspace_id" {
  description = "The Log Analytics Workspace ID."
  value       = module.monitoring.log_analytics_workspace_id
}
