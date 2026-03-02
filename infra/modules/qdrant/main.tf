###############################################################################
# Cognitive Mesh — Qdrant Module
# Provisions Qdrant vector database as an Azure Container Instance.
###############################################################################

resource "azurerm_container_group" "qdrant" {
  name                = "${var.project_name}-qdrant-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  ip_address_type     = var.ip_address_type
  subnet_ids          = var.subnet_ids

  container {
    name   = "qdrant"
    image  = var.qdrant_image
    cpu    = var.cpu_cores
    memory = var.memory_gb

    ports {
      port     = 6333
      protocol = "TCP"
    }

    ports {
      port     = 6334
      protocol = "TCP"
    }

    environment_variables = {
      QDRANT__SERVICE__GRPC_PORT = "6334"
    }

    volume {
      name       = "qdrant-storage"
      mount_path = "/qdrant/storage"

      empty_dir = var.use_persistent_storage ? false : true

      storage_account_name = var.use_persistent_storage ? var.storage_account_name : null
      storage_account_key  = var.use_persistent_storage ? var.storage_account_key : null
      share_name           = var.use_persistent_storage ? var.file_share_name : null
    }
  }

  tags = merge(var.common_tags, {
    Module = "qdrant"
  })
}
