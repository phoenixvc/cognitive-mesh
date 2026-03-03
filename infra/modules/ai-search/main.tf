###############################################################################
# Cognitive Mesh — AI Search Module
# Provisions Azure AI Search (formerly Azure Cognitive Search).
###############################################################################

resource "azurerm_search_service" "this" {
  name                = "${var.project_name}-search-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.sku

  replica_count   = var.replica_count
  partition_count = var.partition_count

  public_network_access_enabled = var.public_network_access_enabled

  local_authentication_enabled = var.local_authentication_enabled

  tags = merge(var.common_tags, {
    Module = "ai-search"
  })
}
