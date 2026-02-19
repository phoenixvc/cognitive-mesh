###############################################################################
# Cognitive Mesh — Key Vault Module
# Provisions Azure Key Vault with access policies.
###############################################################################

data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "this" {
  name                = "${var.project_name}-kv-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = var.sku_name

  soft_delete_retention_days = var.soft_delete_retention_days
  purge_protection_enabled   = var.purge_protection_enabled

  enabled_for_deployment          = var.enabled_for_deployment
  enabled_for_disk_encryption     = var.enabled_for_disk_encryption
  enabled_for_template_deployment = var.enabled_for_template_deployment

  network_acls {
    default_action             = var.network_default_action
    bypass                     = "AzureServices"
    ip_rules                   = var.allowed_ip_ranges
    virtual_network_subnet_ids = var.allowed_subnet_ids
  }

  tags = merge(var.common_tags, {
    Module = "keyvault"
  })
}

# Access policy for the Terraform service principal
resource "azurerm_key_vault_access_policy" "terraform" {
  key_vault_id = azurerm_key_vault.this.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  key_permissions = [
    "Get", "List", "Create", "Delete", "Update", "Recover", "Purge",
  ]

  secret_permissions = [
    "Get", "List", "Set", "Delete", "Recover", "Purge",
  ]

  certificate_permissions = [
    "Get", "List", "Create", "Delete", "Update", "Recover", "Purge",
  ]
}

# Additional access policies for application identities
resource "azurerm_key_vault_access_policy" "additional" {
  for_each = var.access_policies

  key_vault_id = azurerm_key_vault.this.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = each.value.object_id

  key_permissions         = each.value.key_permissions
  secret_permissions      = each.value.secret_permissions
  certificate_permissions = each.value.certificate_permissions
}
