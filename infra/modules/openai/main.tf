###############################################################################
# Cognitive Mesh — Azure OpenAI Module
# Provisions Azure Cognitive Services account (OpenAI kind) with deployments.
###############################################################################

resource "azurerm_cognitive_account" "openai" {
  name                = "${var.project_name}-openai-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  kind                = "OpenAI"
  sku_name            = var.sku_name

  custom_subdomain_name = "${var.project_name}-openai-${var.environment}"

  network_acls {
    default_action = var.network_default_action
    ip_rules       = var.allowed_ip_ranges

    dynamic "virtual_network_rules" {
      for_each = var.allowed_subnet_ids
      content {
        subnet_id = virtual_network_rules.value
      }
    }
  }

  tags = merge(var.common_tags, {
    Module = "openai"
  })
}

resource "azurerm_cognitive_deployment" "deployments" {
  for_each = var.model_deployments

  name                 = each.key
  cognitive_account_id = azurerm_cognitive_account.openai.id

  model {
    format  = "OpenAI"
    name    = each.value.model_name
    version = each.value.model_version
  }

  sku {
    name     = each.value.sku_name
    capacity = each.value.sku_capacity
  }
}
