###############################################################################
# Cognitive Mesh — CosmosDB Module Outputs
###############################################################################

output "account_id" {
  description = "The ID of the Cosmos DB account."
  value       = azurerm_cosmosdb_account.this.id
}

output "account_name" {
  description = "The name of the Cosmos DB account."
  value       = azurerm_cosmosdb_account.this.name
}

output "account_endpoint" {
  description = "The endpoint of the Cosmos DB account."
  value       = azurerm_cosmosdb_account.this.endpoint
}

output "primary_key" {
  description = "The primary key for the Cosmos DB account."
  value       = azurerm_cosmosdb_account.this.primary_key
  sensitive   = true
}

output "connection_strings" {
  description = "Connection strings for the Cosmos DB account."
  value       = azurerm_cosmosdb_account.this.connection_strings
  sensitive   = true
}

output "database_name" {
  description = "The name of the Cosmos DB SQL database."
  value       = azurerm_cosmosdb_sql_database.this.name
}
