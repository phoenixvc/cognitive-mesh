###############################################################################
# Cognitive Mesh — Redis Module
# Provisions Azure Cache for Redis.
# Dev: Basic C0 | Staging/Prod: Standard C1+
###############################################################################

locals {
  is_production = var.environment == "prod" || var.environment == "staging"
  sku_name      = local.is_production ? var.prod_sku_name : "Basic"
  family        = local.is_production ? var.prod_family : "C"
  capacity      = local.is_production ? var.prod_capacity : 0
}

resource "azurerm_redis_cache" "this" {
  name                = "${var.project_name}-redis-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  capacity            = local.capacity
  family              = local.family
  sku_name            = local.sku_name

  minimum_tls_version = "1.2"

  redis_configuration {
    maxmemory_policy = var.maxmemory_policy
  }

  tags = merge(var.common_tags, {
    Module = "redis"
  })
}
