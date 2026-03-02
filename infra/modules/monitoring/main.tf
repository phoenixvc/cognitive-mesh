###############################################################################
# Cognitive Mesh — Monitoring Module
# Provisions Log Analytics Workspace and Application Insights.
###############################################################################

resource "azurerm_log_analytics_workspace" "this" {
  name                = "${var.project_name}-law-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.log_analytics_sku
  retention_in_days   = var.retention_in_days
  daily_quota_gb      = var.daily_quota_gb

  tags = merge(var.common_tags, {
    Module = "monitoring"
  })
}

resource "azurerm_application_insights" "this" {
  name                = "${var.project_name}-appinsights-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = azurerm_log_analytics_workspace.this.id
  application_type    = "web"

  retention_in_days = var.appinsights_retention_days
  sampling_percentage = var.sampling_percentage

  tags = merge(var.common_tags, {
    Module = "monitoring"
  })
}
