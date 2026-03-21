###############################################################################
# Cognitive Mesh — Frontend Hosting Module
# Provisions Azure App Service (Linux, Node.js) for the Next.js SSR frontend.
# Dev: B1 | Staging/Prod: S1+
###############################################################################

locals {
  is_production = var.environment == "prod" || var.environment == "staging"
  sku_name      = local.is_production ? "S1" : var.app_service_plan_sku
  always_on     = local.is_production ? true : var.always_on
}

# ---------- App Service Plan ----------

resource "azurerm_service_plan" "this" {
  name                = "${var.project_name}-frontend-plan-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = local.sku_name

  tags = merge(var.common_tags, {
    Module = "frontend-hosting"
  })
}

# ---------- App Service ----------

resource "azurerm_linux_web_app" "this" {
  name                = "${var.project_name}-frontend-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = azurerm_service_plan.this.id

  https_only = true

  site_config {
    always_on         = local.always_on
    health_check_path = var.health_check_path

    application_stack {
      node_version = var.node_version
    }

    # Security headers
    default_documents = []
  }

  app_settings = {
    "NEXT_PUBLIC_API_BASE_URL"        = var.api_base_url
    "WEBSITE_NODE_DEFAULT_VERSION"    = "~22"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE" = "false"
    "SCM_DO_BUILD_DURING_DEPLOYMENT"  = "true"
  }

  # VNet integration (if subnet provided)
  virtual_network_subnet_id = var.subnet_id

  tags = merge(var.common_tags, {
    Module = "frontend-hosting"
  })
}

# ---------- Managed Certificate + Custom Domain ----------

resource "azurerm_app_service_custom_hostname_binding" "this" {
  count               = var.custom_domain != null ? 1 : 0
  hostname            = var.custom_domain
  app_service_name    = azurerm_linux_web_app.this.name
  resource_group_name = var.resource_group_name
}

resource "azurerm_app_service_managed_certificate" "this" {
  count                      = var.custom_domain != null ? 1 : 0
  custom_hostname_binding_id = azurerm_app_service_custom_hostname_binding.this[0].id
}

resource "azurerm_app_service_certificate_binding" "this" {
  count               = var.custom_domain != null ? 1 : 0
  hostname_binding_id = azurerm_app_service_custom_hostname_binding.this[0].id
  certificate_id      = azurerm_app_service_managed_certificate.this[0].id
  ssl_state           = "SniEnabled"
}
