###############################################################################
# Cognitive Mesh — Terraform Backend Configuration
# State is stored in Azure Blob Storage.
###############################################################################

terraform {
  backend "azurerm" {
    resource_group_name  = "cognitive-mesh-tfstate-rg"
    storage_account_name = "cognitivemeshtfstate"
    container_name       = "tfstate"
    key                  = "cognitive-mesh.tfstate"
  }
}
