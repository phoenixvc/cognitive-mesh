###############################################################################
# Cognitive Mesh — AI Search Module Outputs
###############################################################################

output "search_service_id" {
  description = "The ID of the Azure AI Search service."
  value       = azurerm_search_service.this.id
}

output "search_service_name" {
  description = "The name of the Azure AI Search service."
  value       = azurerm_search_service.this.name
}

output "primary_key" {
  description = "The primary admin key for the search service."
  value       = azurerm_search_service.this.primary_key
  sensitive   = true
}

output "secondary_key" {
  description = "The secondary admin key for the search service."
  value       = azurerm_search_service.this.secondary_key
  sensitive   = true
}

output "query_keys" {
  description = "Query keys for the search service."
  value       = azurerm_search_service.this.query_keys
  sensitive   = true
}
