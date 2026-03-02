###############################################################################
# Cognitive Mesh — CosmosDB Module
# Provisions a serverless Cosmos DB account with SQL API and a database.
###############################################################################

resource "azurerm_cosmosdb_account" "this" {
  name                = "${var.project_name}-cosmos-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  automatic_failover_enabled = var.enable_automatic_failover

  capabilities {
    name = "EnableServerless"
  }

  consistency_policy {
    consistency_level       = var.consistency_level
    max_interval_in_seconds = var.consistency_level == "BoundedStaleness" ? var.max_staleness_interval : null
    max_staleness_prefix    = var.consistency_level == "BoundedStaleness" ? var.max_staleness_prefix : null
  }

  geo_location {
    location          = var.location
    failover_priority = 0
  }

  dynamic "geo_location" {
    for_each = var.secondary_locations
    content {
      location          = geo_location.value.location
      failover_priority = geo_location.value.failover_priority
    }
  }

  tags = merge(var.common_tags, {
    Module = "cosmosdb"
  })
}

resource "azurerm_cosmosdb_sql_database" "this" {
  name                = var.database_name
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.this.name
}

resource "azurerm_cosmosdb_sql_container" "containers" {
  for_each = var.containers

  name                = each.key
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.this.name
  database_name       = azurerm_cosmosdb_sql_database.this.name
  partition_key_paths = each.value.partition_key_paths

  default_ttl = lookup(each.value, "default_ttl", -1)

  indexing_policy {
    indexing_mode = "consistent"

    included_path {
      path = "/*"
    }

    excluded_path {
      path = "/\"_etag\"/?"
    }
  }
}
