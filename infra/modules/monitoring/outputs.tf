###############################################################################
# Cognitive Mesh — Monitoring Module Outputs
###############################################################################

output "log_analytics_workspace_id" {
  description = "The ID of the Log Analytics Workspace."
  value       = azurerm_log_analytics_workspace.this.id
}

output "log_analytics_workspace_name" {
  description = "The name of the Log Analytics Workspace."
  value       = azurerm_log_analytics_workspace.this.name
}

output "log_analytics_workspace_primary_key" {
  description = "The primary shared key for the Log Analytics Workspace."
  value       = azurerm_log_analytics_workspace.this.primary_shared_key
  sensitive   = true
}

output "application_insights_id" {
  description = "The ID of the Application Insights resource."
  value       = azurerm_application_insights.this.id
}

output "application_insights_name" {
  description = "The name of the Application Insights resource."
  value       = azurerm_application_insights.this.name
}

output "application_insights_instrumentation_key" {
  description = "The instrumentation key for Application Insights."
  value       = azurerm_application_insights.this.instrumentation_key
  sensitive   = true
}

output "application_insights_connection_string" {
  description = "The connection string for Application Insights."
  value       = azurerm_application_insights.this.connection_string
  sensitive   = true
}

output "application_insights_app_id" {
  description = "The App ID for Application Insights."
  value       = azurerm_application_insights.this.app_id
}
