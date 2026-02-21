###############################################################################
# Cognitive Mesh — Storage Module
# Provisions an Azure Storage Account (StorageV2, LRS) with containers.
###############################################################################

resource "azurerm_storage_account" "this" {
  name                     = replace("${var.project_name}st${var.environment}", "-", "")
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = var.account_tier
  account_replication_type = var.replication_type
  account_kind             = "StorageV2"
  min_tls_version          = "TLS1_2"

  https_traffic_only_enabled = true

  blob_properties {
    versioning_enabled = var.enable_versioning

    delete_retention_policy {
      days = var.soft_delete_retention_days
    }

    container_delete_retention_policy {
      days = var.soft_delete_retention_days
    }
  }

  tags = merge(var.common_tags, {
    Module = "storage"
  })
}

resource "azurerm_storage_container" "containers" {
  for_each = var.containers

  name                  = each.key
  storage_account_id    = azurerm_storage_account.this.id
  container_access_type = each.value.access_type
}
