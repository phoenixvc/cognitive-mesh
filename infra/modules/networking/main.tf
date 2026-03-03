###############################################################################
# Cognitive Mesh — Networking Module
# Provisions VNet, subnets, NSGs, and private endpoints.
###############################################################################

resource "azurerm_virtual_network" "this" {
  name                = "${var.project_name}-vnet-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  address_space       = var.vnet_address_space

  tags = merge(var.common_tags, {
    Module = "networking"
  })
}

# ---------- Subnets ----------

resource "azurerm_subnet" "subnets" {
  for_each = var.subnets

  name                 = each.key
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.this.name
  address_prefixes     = each.value.address_prefixes

  dynamic "delegation" {
    for_each = each.value.delegation != null ? [each.value.delegation] : []
    content {
      name = delegation.value.name
      service_delegation {
        name    = delegation.value.service_name
        actions = delegation.value.actions
      }
    }
  }

  service_endpoints = each.value.service_endpoints
}

# ---------- Network Security Groups ----------

resource "azurerm_network_security_group" "subnets" {
  for_each = { for k, v in var.subnets : k => v if v.create_nsg }

  name                = "${var.project_name}-nsg-${each.key}-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name

  tags = merge(var.common_tags, {
    Module = "networking"
    Subnet = each.key
  })
}

resource "azurerm_subnet_network_security_group_association" "subnets" {
  for_each = { for k, v in var.subnets : k => v if v.create_nsg }

  subnet_id                 = azurerm_subnet.subnets[each.key].id
  network_security_group_id = azurerm_network_security_group.subnets[each.key].id
}

# ---------- Private DNS Zones ----------

resource "azurerm_private_dns_zone" "zones" {
  for_each = var.private_dns_zones

  name                = each.value
  resource_group_name = var.resource_group_name

  tags = merge(var.common_tags, {
    Module = "networking"
  })
}

resource "azurerm_private_dns_zone_virtual_network_link" "links" {
  for_each = var.private_dns_zones

  name                  = "${each.key}-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.zones[each.key].name
  virtual_network_id    = azurerm_virtual_network.this.id
  registration_enabled  = false

  tags = merge(var.common_tags, {
    Module = "networking"
  })
}

# ---------- Private Endpoints ----------

resource "azurerm_private_endpoint" "endpoints" {
  for_each = var.private_endpoints

  name                = "${var.project_name}-pe-${each.key}-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = azurerm_subnet.subnets[each.value.subnet_key].id

  private_service_connection {
    name                           = "${each.key}-connection"
    private_connection_resource_id = each.value.resource_id
    subresource_names              = each.value.subresource_names
    is_manual_connection           = false
  }

  dynamic "private_dns_zone_group" {
    for_each = each.value.dns_zone_key != null ? [each.value.dns_zone_key] : []
    content {
      name                 = "default"
      private_dns_zone_ids = [azurerm_private_dns_zone.zones[private_dns_zone_group.value].id]
    }
  }

  tags = merge(var.common_tags, {
    Module = "networking"
  })
}
