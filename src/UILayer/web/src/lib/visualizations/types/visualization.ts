/**
 * Shared TypeScript types for the Cognitive Mesh visualization components.
 * These types define the data contracts for audit timelines, metrics charts,
 * and agent network graphs used across the UILayer.
 */

/**
 * Represents a single audit event to be rendered on the timeline.
 * Events are positioned by timestamp and color-coded by severity.
 */
export interface AuditEvent {
  /** Unique identifier for the audit event. */
  id: string;
  /** When the event occurred. */
  timestamp: Date;
  /** Severity classification determining the visual color encoding. */
  severity: 'info' | 'warning' | 'error' | 'critical';
  /** Short display title for the event. */
  title: string;
  /** Detailed description shown in tooltips and expanded views. */
  description: string;
  /** Optional identifier of the agent that produced the event. */
  agentId?: string;
}

/**
 * A single data point for time-series metrics visualization.
 */
export interface MetricDataPoint {
  /** The timestamp of the measurement. */
  timestamp: Date;
  /** The numeric value of the metric at this point in time. */
  value: number;
  /** Optional label for the data point (e.g., annotation text). */
  label?: string;
}

/**
 * Represents a single agent node in the force-directed network graph.
 */
export interface AgentNode {
  /** Unique identifier for the agent. */
  id: string;
  /** Human-readable display name. */
  name: string;
  /** The agent's type classification (e.g., 'DataProcessor', 'ComplianceAuditor'). */
  type: string;
  /** Current operational status of the agent. */
  status: 'active' | 'idle' | 'error';
  /** Numeric activity level (0-1) used to scale node size. */
  activityLevel: number;
}

/**
 * Represents a directional connection between two agents in the network graph.
 */
export interface AgentConnection {
  /** The source agent identifier. */
  source: string;
  /** The target agent identifier. */
  target: string;
  /** Connection weight (0-1) used to scale edge thickness. */
  weight: number;
  /** The type of communication channel between agents. */
  type: 'data' | 'control' | 'feedback';
}

/**
 * Color and styling tokens for visualization theming.
 * Supports both light and dark mode rendering.
 */
export interface VisualizationTheme {
  /** Background color for the SVG canvas. */
  background: string;
  /** Primary text color. */
  text: string;
  /** Grid line and axis color. */
  grid: string;
  /** Primary accent color. */
  primary: string;
  /** Secondary accent color. */
  secondary: string;
  /** Color for success / positive indicators. */
  success: string;
  /** Color for warning indicators. */
  warning: string;
  /** Color for error / negative indicators. */
  error: string;
}

/**
 * Severity-to-color mapping used by the AuditTimeline component.
 */
export interface SeverityColorMap {
  /** Blue for informational events. */
  info: string;
  /** Amber for warning events. */
  warning: string;
  /** Red for error events. */
  error: string;
  /** Purple for critical events. */
  critical: string;
}

/**
 * Configuration for horizontal threshold lines on the MetricsChart.
 */
export interface ThresholdLine {
  /** The y-axis value where the threshold line is drawn. */
  value: number;
  /** Display label for the threshold. */
  label: string;
  /** Line color. */
  color: string;
}
