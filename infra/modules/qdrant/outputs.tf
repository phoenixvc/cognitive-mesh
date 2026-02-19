###############################################################################
# Cognitive Mesh — Qdrant Module Outputs
###############################################################################

output "container_group_id" {
  description = "The ID of the container group."
  value       = azurerm_container_group.qdrant.id
}

output "container_group_name" {
  description = "The name of the container group."
  value       = azurerm_container_group.qdrant.name
}

output "ip_address" {
  description = "The IP address of the Qdrant instance."
  value       = azurerm_container_group.qdrant.ip_address
}

output "http_endpoint" {
  description = "The HTTP endpoint for the Qdrant REST API."
  value       = "http://${azurerm_container_group.qdrant.ip_address}:6333"
}

output "grpc_endpoint" {
  description = "The gRPC endpoint for the Qdrant API."
  value       = "${azurerm_container_group.qdrant.ip_address}:6334"
}
