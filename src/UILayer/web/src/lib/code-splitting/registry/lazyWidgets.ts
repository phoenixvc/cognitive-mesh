/**
 * Registry of lazily-loadable widgets for the Cognitive Mesh UILayer.
 *
 * Each export wraps a widget panel component with {@link createLazyWidget},
 * enabling automatic code splitting, loading skeletons, and error boundaries.
 *
 * Import these lazy variants instead of the direct panel components when
 * rendering widgets that are not immediately visible (e.g., behind tabs,
 * in secondary views, or loaded on demand).
 *
 * @example
 * ```tsx
 * import { LazyAgentControlCenter } from '@/lib/code-splitting/registry/lazyWidgets';
 *
 * function Dashboard() {
 *   return <LazyAgentControlCenter userId="admin" tenantId="t1" />;
 * }
 * ```
 */

import { createLazyWidget } from '../LazyWidgetLoader';

/**
 * Lazy-loaded Agent Control Center panel.
 * Provides the agent registry view with authority scope management.
 */
export const LazyAgentControlCenter = createLazyWidget(
  () => import('@/components/agency/AgentControlCenter')
);

/**
 * Lazy-loaded Value Diagnostic Dashboard panel.
 * Shows personal value diagnostics and organizational blindness trends.
 */
export const LazyValueDiagnosticDashboard = createLazyWidget(
  () => import('@/components/agency/ValueDiagnosticDashboard')
);

/**
 * Lazy-loaded Audit Event Log Overlay panel.
 * Modal overlay for browsing and filtering agent audit events.
 */
export const LazyAuditEventLogOverlay = createLazyWidget(
  () => import('@/components/agency/AuditEventLogOverlay')
);

/**
 * Lazy-loaded Registry Viewer panel.
 * Browse and inspect the widget and agent registry.
 */
export const LazyRegistryViewer = createLazyWidget(
  () => import('@/components/agency/RegistryViewer')
);

/**
 * Lazy-loaded Agent Status Banner component.
 * Displays the current status of monitored agents.
 */
export const LazyAgentStatusBanner = createLazyWidget(
  () => import('@/components/agency/AgentStatusBanner')
);

/**
 * Lazy-loaded Authority Consent Modal component.
 * Handles authority scope consent flows for agents.
 */
export const LazyAuthorityConsentModal = createLazyWidget(
  () => import('@/components/agency/AuthorityConsentModal')
);

/**
 * Lazy-loaded Employability Score Widget.
 * Displays individual employability scoring and analysis.
 */
export const LazyEmployabilityScoreWidget = createLazyWidget(
  () => import('@/components/agency/EmployabilityScoreWidget')
);

/**
 * Lazy-loaded Organizational Blindness Trends panel.
 * Visualizes organizational blindness risk trends over time.
 */
export const LazyOrgBlindnessTrends = createLazyWidget(
  () => import('@/components/agency/OrgBlindnessTrends')
);

/**
 * Lazy-loaded $200 Test Widget.
 * Experimental widget for value estimation testing.
 */
export const LazyTwoHundredDollarTestWidget = createLazyWidget(
  () => import('@/components/agency/TwoHundredDollarTestWidget')
);

/**
 * Lazy-loaded Audit Timeline visualization.
 * D3-powered timeline of audit events.
 */
export const LazyAuditTimeline = createLazyWidget(
  () => import('@/components/visualizations/AuditTimeline')
);

/**
 * Lazy-loaded Metrics Chart visualization.
 * D3-powered real-time metrics line chart.
 */
export const LazyMetricsChart = createLazyWidget(
  () => import('@/components/visualizations/MetricsChart')
);

/**
 * Lazy-loaded Agent Network Graph visualization.
 * D3-powered force-directed graph of agent relationships.
 */
export const LazyAgentNetworkGraph = createLazyWidget(
  () => import('@/components/visualizations/AgentNetworkGraph')
);
