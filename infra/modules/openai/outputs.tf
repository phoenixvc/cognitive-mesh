###############################################################################
# Cognitive Mesh — Azure OpenAI Module Outputs
###############################################################################

output "cognitive_account_id" {
  description = "The ID of the Azure OpenAI account."
  value       = azurerm_cognitive_account.openai.id
}

output "cognitive_account_name" {
  description = "The name of the Azure OpenAI account."
  value       = azurerm_cognitive_account.openai.name
}

output "endpoint" {
  description = "The endpoint of the Azure OpenAI account."
  value       = azurerm_cognitive_account.openai.endpoint
}

output "primary_access_key" {
  description = "The primary access key for the Azure OpenAI account."
  value       = azurerm_cognitive_account.openai.primary_access_key
  sensitive   = true
}

output "deployment_ids" {
  description = "Map of deployment names to their IDs."
  value       = { for k, v in azurerm_cognitive_deployment.deployments : k => v.id }
}
