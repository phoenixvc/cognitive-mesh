###############################################################################
# Cognitive Mesh — Frontend Hosting Module Outputs
###############################################################################

output "app_service_id" {
  description = "The ID of the frontend App Service."
  value       = azurerm_linux_web_app.this.id
}

output "app_service_name" {
  description = "The name of the frontend App Service."
  value       = azurerm_linux_web_app.this.name
}

output "default_hostname" {
  description = "The default hostname of the frontend App Service."
  value       = azurerm_linux_web_app.this.default_hostname
}

output "app_service_url" {
  description = "The default URL of the frontend App Service."
  value       = "https://${azurerm_linux_web_app.this.default_hostname}"
}

output "app_service_plan_id" {
  description = "The ID of the App Service Plan."
  value       = azurerm_service_plan.this.id
}

output "outbound_ip_addresses" {
  description = "The outbound IP addresses of the frontend App Service."
  value       = azurerm_linux_web_app.this.outbound_ip_addresses
}
