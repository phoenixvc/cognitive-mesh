###############################################################################
# Cognitive Mesh — Root Terragrunt Configuration
# Shared settings inherited by all environment configurations.
###############################################################################

# Generate the Azure provider block in each child module
generate "provider" {
  path      = "provider.tf"
  if_exists = "overwrite_terragrunt"
  contents  = <<-EOF
    terraform {
      required_version = ">= 1.7.0"

      required_providers {
        azurerm = {
          source  = "hashicorp/azurerm"
          version = "~> 4.0"
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
      }
    }
  EOF
}

# Configure remote state storage in Azure Blob
remote_state {
  backend = "azurerm"
  generate = {
    path      = "backend.tf"
    if_exists = "overwrite_terragrunt"
  }
  config = {
    resource_group_name  = "cognitive-mesh-tfstate-rg"
    storage_account_name = "cognitivemeshtfstate"
    container_name       = "tfstate"
    key                  = "${path_relative_to_include()}/terraform.tfstate"
  }
}

# Common input variables inherited by all environments
inputs = {
  project_name = "cognitive-mesh"

  common_tags = {
    Project   = "CognitiveMesh"
    ManagedBy = "terraform"
  }
}
