###############################################################################
# Cognitive Mesh — Provider Configuration
###############################################################################

terraform {
  required_version = "1.15.3"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.73.0"
    }
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = false
    }

    resource_group {
      prevent_deletion_if_contains_resources = true
    }

    cognitive_account {
      purge_soft_delete_on_destroy = false
    }
  }
}
