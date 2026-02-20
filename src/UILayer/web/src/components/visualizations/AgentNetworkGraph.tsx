import React, { useEffect, useCallback, useMemo, useRef, useState } from 'react';
import * as d3 from 'd3';
import { AgentNode, AgentConnection, VisualizationTheme } from '../types/visualization';
import { useD3 } from '../hooks/useD3';
import { defaultTheme } from '../themes/defaultTheme';

/**
 * Props for the AgentNetworkGraph component.
 */
export interface AgentNetworkGraphProps {
  /** Array of agent nodes to display in the graph. */
  agents: AgentNode[];
  /** Array of connections (edges) between agents. */
  connections: AgentConnection[];
  /** Width of the SVG canvas in pixels. */
  width: number;
  /** Height of the SVG canvas in pixels. */
  height: number;
  /** Callback fired when a user clicks on an agent node. */
  onNodeClick?: (agent: AgentNode) => void;
  /** Visualization theme for light/dark mode support. */
  theme?: VisualizationTheme;
}

/** Internal node type extending AgentNode with D3 simulation coordinates. */
interface SimulationNode extends AgentNode, d3.SimulationNodeDatum {}

/** Internal link type with D3 simulation source/target references. */
interface SimulationLink extends d3.SimulationLinkDatum<SimulationNode> {
  weight: number;
  type: AgentConnection['type'];
}

/** Minimum node radius. */
const MIN_NODE_RADIUS = 12;

/** Maximum node radius. */
const MAX_NODE_RADIUS = 30;

/** Minimum edge stroke width. */
const MIN_EDGE_WIDTH = 1;

/** Maximum edge stroke width. */
const MAX_EDGE_WIDTH = 6;

/**
 * Returns a fill color for an agent node based on its operational status.
 */
function getStatusColor(status: AgentNode['status'], theme: VisualizationTheme): string {
  switch (status) {
    case 'active':
      return theme.success;
    case 'idle':
      return theme.secondary;
    case 'error':
      return theme.error;
    default:
      return theme.secondary;
  }
}

/**
 * Returns a stroke style for connection edges based on their type.
 */
function getEdgeDashArray(type: AgentConnection['type']): string {
  switch (type) {
    case 'data':
      return 'none';
    case 'control':
      return '6,3';
    case 'feedback':
      return '2,4';
    default:
      return 'none';
  }
}

/**
 * Force-directed graph visualization showing agent relationships.
 *
 * Renders a network graph where each agent is a node and connections form
 * edges between them. Uses D3's force simulation with charge, link, center,
 * and collision forces for layout.
 *
 * Visual encodings:
 * - Node size is scaled by the agent's activity level (0-1).
 * - Node color is determined by agent status (active=green, idle=gray, error=red).
 * - Edge thickness is scaled by communication weight (0-1).
 * - Edge dash pattern varies by connection type (data=solid, control=dashed, feedback=dotted).
 *
 * Interactions:
 * - Hover over a node to reveal its label and details.
 * - Click a node to select/highlight it and trigger the onNodeClick callback.
 * - Drag nodes to reposition them within the simulation.
 */
const AgentNetworkGraph: React.FC<AgentNetworkGraphProps> = ({
  agents,
  connections,
  width,
  height,
  onNodeClick,
  theme = defaultTheme,
}) => {
  const [dimensions, setDimensions] = useState({ width, height });
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
  const tooltipRef = useRef<HTMLDivElement | null>(null);

  const handleResize = useCallback((newWidth: number, newHeight: number) => {
    setDimensions({ width: newWidth, height: newHeight });
  }, []);

  const { svgRef, getSelection } = useD3(handleResize);

  // Update dimensions when props change
  useEffect(() => {
    setDimensions({ width, height });
  }, [width, height]);

  /** Screen reader summary of the network. */
  const screenReaderSummary = useMemo(() => {
    const activeCount = agents.filter((a) => a.status === 'active').length;
    const idleCount = agents.filter((a) => a.status === 'idle').length;
    const errorCount = agents.filter((a) => a.status === 'error').length;
    return (
      `Agent network graph with ${agents.length} agents and ${connections.length} connections. ` +
      `${activeCount} active, ${idleCount} idle, ${errorCount} in error state.`
    );
  }, [agents, connections]);

  // Main D3 render effect
  useEffect(() => {
    const svg = getSelection();
    if (!svg || agents.length === 0) {
      return;
    }

    const { width: w, height: h } = dimensions;

    // Clear previous render
    svg.selectAll('*').remove();

    // Background
    svg
      .append('rect')
      .attr('width', w)
      .attr('height', h)
      .attr('fill', theme.background);

    const g = svg.append('g');

    // Prepare simulation data (deep copy to avoid mutating props)
    const nodes: SimulationNode[] = agents.map((agent) => ({ ...agent }));
    const nodeMap = new Map(nodes.map((n) => [n.id, n]));

    const links: SimulationLink[] = connections
      .filter((c) => nodeMap.has(c.source) && nodeMap.has(c.target))
      .map((c) => ({
        source: c.source,
        target: c.target,
        weight: c.weight,
        type: c.type,
      }));

    // Radius scale based on activity level
    const radiusScale = d3
      .scaleLinear()
      .domain([0, 1])
      .range([MIN_NODE_RADIUS, MAX_NODE_RADIUS])
      .clamp(true);

    // Edge width scale based on weight
    const edgeWidthScale = d3
      .scaleLinear()
      .domain([0, 1])
      .range([MIN_EDGE_WIDTH, MAX_EDGE_WIDTH])
      .clamp(true);

    // Arrow marker definitions
    const defs = svg.append('defs');
    ['data', 'control', 'feedback'].forEach((edgeType) => {
      defs
        .append('marker')
        .attr('id', `arrow-${edgeType}`)
        .attr('viewBox', '0 -5 10 10')
        .attr('refX', 20)
        .attr('refY', 0)
        .attr('markerWidth', 6)
        .attr('markerHeight', 6)
        .attr('orient', 'auto')
        .append('path')
        .attr('d', 'M0,-5L10,0L0,5')
        .attr('fill', theme.grid);
    });

    // Force simulation
    const simulation = d3
      .forceSimulation<SimulationNode>(nodes)
      .force(
        'link',
        d3
          .forceLink<SimulationNode, SimulationLink>(links)
          .id((d) => d.id)
          .distance(120)
      )
      .force('charge', d3.forceManyBody().strength(-300))
      .force('center', d3.forceCenter(w / 2, h / 2))
      .force(
        'collision',
        d3.forceCollide<SimulationNode>().radius((d) => radiusScale(d.activityLevel) + 5)
      );

    // Draw edges
    const linkElements = g
      .selectAll<SVGLineElement, SimulationLink>('line.link')
      .data(links)
      .enter()
      .append('line')
      .attr('class', 'link')
      .attr('stroke', theme.grid)
      .attr('stroke-width', (d) => edgeWidthScale(d.weight))
      .attr('stroke-dasharray', (d) => getEdgeDashArray(d.type))
      .attr('marker-end', (d) => `url(#arrow-${d.type})`)
      .attr('opacity', 0.6);

    // Draw node groups
    const nodeElements = g
      .selectAll<SVGGElement, SimulationNode>('g.node')
      .data(nodes, (d) => d.id)
      .enter()
      .append('g')
      .attr('class', 'node')
      .style('cursor', 'pointer')
      .attr('tabindex', '0')
      .attr('role', 'button')
      .attr('aria-label', (d) => `Agent ${d.name}, type ${d.type}, status ${d.status}, activity level ${(d.activityLevel * 100).toFixed(0)}%`);

    // Node circles
    nodeElements
      .append('circle')
      .attr('r', (d) => radiusScale(d.activityLevel))
      .attr('fill', (d) => getStatusColor(d.status, theme))
      .attr('stroke', (d) => (d.id === selectedNodeId ? theme.primary : theme.background))
      .attr('stroke-width', (d) => (d.id === selectedNodeId ? 3 : 2));

    // Node text labels (visible on hover via CSS-like behavior)
    const labelElements = nodeElements
      .append('text')
      .text((d) => d.name)
      .attr('text-anchor', 'middle')
      .attr('dy', (d) => radiusScale(d.activityLevel) + 14)
      .attr('fill', theme.text)
      .style('font-size', '11px')
      .style('font-weight', 'bold')
      .style('pointer-events', 'none')
      .attr('opacity', 0);

    // Tooltip
    const tooltip = d3
      .select(tooltipRef.current)
      .style('position', 'absolute')
      .style('pointer-events', 'none')
      .style('opacity', 0)
      .style('background', theme.background)
      .style('color', theme.text)
      .style('border', `1px solid ${theme.grid}`)
      .style('border-radius', '6px')
      .style('padding', '8px 12px')
      .style('font-size', '12px')
      .style('box-shadow', '0 2px 8px rgba(0,0,0,0.15)')
      .style('z-index', '1000');

    // Hover interactions
    nodeElements
      .on('mouseenter', function (mouseEvent: MouseEvent, d: SimulationNode) {
        // Show label
        d3.select(this).select('text').transition().duration(150).attr('opacity', 1);

        // Highlight connected edges
        linkElements
          .attr('opacity', (l) => {
            const src = typeof l.source === 'object' ? (l.source as SimulationNode).id : l.source;
            const tgt = typeof l.target === 'object' ? (l.target as SimulationNode).id : l.target;
            return src === d.id || tgt === d.id ? 1 : 0.15;
          });

        // Show tooltip
        tooltip
          .html(
            `<strong>${d.name}</strong><br/>` +
            `Type: ${d.type}<br/>` +
            `Status: <span style="color:${getStatusColor(d.status, theme)}; text-transform:capitalize;">${d.status}</span><br/>` +
            `Activity: ${(d.activityLevel * 100).toFixed(0)}%`
          )
          .style('left', `${mouseEvent.offsetX + 15}px`)
          .style('top', `${mouseEvent.offsetY - 10}px`)
          .transition()
          .duration(200)
          .style('opacity', 1);
      })
      .on('mouseleave', function () {
        // Hide label
        d3.select(this).select('text').transition().duration(150).attr('opacity', 0);

        // Reset edge opacity
        linkElements.attr('opacity', 0.6);

        // Hide tooltip
        tooltip.transition().duration(200).style('opacity', 0);
      })
      .on('click', function (_mouseEvent: MouseEvent, d: SimulationNode) {
        setSelectedNodeId((prev) => (prev === d.id ? null : d.id));
        if (onNodeClick) {
          onNodeClick(d);
        }
      });

    // Drag behavior
    const dragBehavior = d3
      .drag<SVGGElement, SimulationNode>()
      .on('start', (dragEvent, d) => {
        if (!dragEvent.active) {
          simulation.alphaTarget(0.3).restart();
        }
        d.fx = d.x;
        d.fy = d.y;
      })
      .on('drag', (dragEvent, d) => {
        d.fx = dragEvent.x;
        d.fy = dragEvent.y;
      })
      .on('end', (dragEvent, d) => {
        if (!dragEvent.active) {
          simulation.alphaTarget(0);
        }
        d.fx = null;
        d.fy = null;
      });

    nodeElements.call(dragBehavior);

    // Tick update
    simulation.on('tick', () => {
      linkElements
        .attr('x1', (d) => ((d.source as SimulationNode).x ?? 0))
        .attr('y1', (d) => ((d.source as SimulationNode).y ?? 0))
        .attr('x2', (d) => ((d.target as SimulationNode).x ?? 0))
        .attr('y2', (d) => ((d.target as SimulationNode).y ?? 0));

      nodeElements.attr('transform', (d) => `translate(${d.x ?? 0},${d.y ?? 0})`);
    });

    // Cleanup simulation on unmount
    return () => {
      simulation.stop();
    };
  }, [agents, connections, dimensions, theme, onNodeClick, selectedNodeId, getSelection]);

  return (
    <div style={{ position: 'relative', display: 'inline-block' }}>
      <svg
        ref={svgRef}
        width={dimensions.width}
        height={dimensions.height}
        role="img"
        aria-label={`Force-directed graph showing ${agents.length} agents and their communication connections.`}
        style={{ display: 'block' }}
      >
        <title>Agent Network Graph</title>
        <desc>{screenReaderSummary}</desc>
      </svg>
      <div ref={tooltipRef} aria-hidden="true" />
    </div>
  );
};

export default AgentNetworkGraph;
