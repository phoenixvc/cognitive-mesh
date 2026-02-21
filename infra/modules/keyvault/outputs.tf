###############################################################################
# Cognitive Mesh — Key Vault Module Outputs
###############################################################################

output "key_vault_id" {
  description = "The ID of the Key Vault."
  value       = azurerm_key_vault.this.id
}

output "key_vault_name" {
  description = "The name of the Key Vault."
  value       = azurerm_key_vault.this.name
}

output "key_vault_uri" {
  description = "The URI of the Key Vault."
  value       = azurerm_key_vault.this.vault_uri
}

output "tenant_id" {
  description = "The tenant ID associated with the Key Vault."
  value       = azurerm_key_vault.this.tenant_id
}
