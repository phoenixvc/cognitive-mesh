/**
 * Registry of lazily-loadable widgets for the Cognitive Mesh UILayer.
 *
 * Each export wraps a widget panel component with {@link createLazyWidget},
 * enabling automatic code splitting, loading skeletons, and error boundaries.
 */

import React from 'react';
import { createLazyWidget } from '../LazyWidgetLoader';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
type AnyImport = Promise<{ default: React.ComponentType<any> }>;

export const LazyAgentControlCenter = createLazyWidget(
  () => import('@/components/agency/AgentControlCenter') as AnyImport
);

export const LazyValueDiagnosticDashboard = createLazyWidget(
  () => import('@/components/agency/ValueDiagnosticDashboard') as AnyImport
);

export const LazyAuditEventLogOverlay = createLazyWidget(
  () => import('@/components/agency/AuditEventLogOverlay') as AnyImport
);

export const LazyRegistryViewer = createLazyWidget(
  () => import('@/components/agency/RegistryViewer') as AnyImport
);

export const LazyAgentStatusBanner = createLazyWidget(
  () => import('@/components/agency/AgentStatusBanner') as AnyImport
);

export const LazyAuthorityConsentModal = createLazyWidget(
  () => import('@/components/agency/AuthorityConsentModal') as AnyImport
);

export const LazyEmployabilityScoreWidget = createLazyWidget(
  () => import('@/components/agency/EmployabilityScoreWidget') as AnyImport
);

export const LazyOrgBlindnessTrends = createLazyWidget(
  () => import('@/components/agency/OrgBlindnessTrends') as AnyImport
);

export const LazyTwoHundredDollarTestWidget = createLazyWidget(
  () => import('@/components/agency/TwoHundredDollarTestWidget') as AnyImport
);

export const LazyAuditTimeline = createLazyWidget(
  () => import('@/components/visualizations/AuditTimeline') as AnyImport
);

export const LazyMetricsChart = createLazyWidget(
  () => import('@/components/visualizations/MetricsChart') as AnyImport
);

export const LazyAgentNetworkGraph = createLazyWidget(
  () => import('@/components/visualizations/AgentNetworkGraph') as AnyImport
);

// Phase 15b — PRD Widget Dashboards

export const LazyNistComplianceDashboard = createLazyWidget(
  () => import('@/components/widgets/NistCompliance/NistComplianceDashboard') as AnyImport
);

export const LazyAdaptiveBalanceDashboard = createLazyWidget(
  () => import('@/components/widgets/AdaptiveBalance/AdaptiveBalanceDashboard') as AnyImport
);

export const LazyValueGenerationDashboard = createLazyWidget(
  () => import('@/components/widgets/ValueGeneration/ValueGenerationDashboard') as AnyImport
);

export const LazyImpactMetricsDashboard = createLazyWidget(
  () => import('@/components/widgets/ImpactMetrics/ImpactMetricsDashboard') as AnyImport
);

export const LazyCognitiveSandwichDashboard = createLazyWidget(
  () => import('@/components/widgets/CognitiveSandwich/CognitiveSandwichDashboard') as AnyImport
);
