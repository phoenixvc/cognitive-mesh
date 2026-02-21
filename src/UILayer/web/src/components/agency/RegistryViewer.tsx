import React, { useState, useEffect, useCallback, CSSProperties } from 'react';

// --- Mock Adapter Interfaces and Models (as defined in IAgencyWidgetAdapters.cs) ---
// In a real application, these would be imported from a shared library and injected.

interface ErrorEnvelope {
  errorCode: string;
  message: string;
  correlationId?: string;
  details?: any;
  canRetry?: boolean;
}

interface AgentViewModel {
  agentId: string;
  agentType: string;
  description: string;
  status: 'Active' | 'Deprecated' | 'Retired';
  version: string;
  defaultAutonomy: 'RecommendOnly' | 'ActWithConfirmation' | 'FullyAutonomous';
  capabilities: string[];
  versionHistory: { version: string; changes: string; date: string; }[];
  relationships: { type: string; targetAgentId: string; }[];
}

interface AuthorityScopeViewModel {
  allowedApiEndpoints: string[];
  maxResourceConsumption?: number;
  maxBudget?: number;
  dataAccessPolicies: string[];
}

interface WidgetState<T> {
  data: T | null;
  isStale: boolean;
  lastSyncTimestamp: Date;
  lastError: ErrorEnvelope | null;
}

interface TelemetryEvent {
  timestamp: Date;
  widgetId: string;
  panelId: string;
  userId: string;
  correlationId?: string;
  action: string;
  errorCode?: string;
  metadata: { [key: string]: any };
}

enum TelemetryEventTypes {
  RegistryViewed = 'RegistryViewed',
  AgentDetailsViewed = 'AgentDetailsViewed',
  RegistryFiltered = 'RegistryFiltered',
  RegistrySorted = 'RegistrySorted',
  RegistryExported = 'RegistryExported',
  ApiCallFailed = 'ApiCallFailed',
}

interface ThemeSettings {
  name: 'Light' | 'Dark';
  highContrastEnabled: boolean;
  languageCode: string;
}

// --- Adapter Port Interfaces ---
interface IDataAPIAdapterPort {
  getAgentRegistryAsync(includeRetired: boolean, tenantId: string): Promise<WidgetState<AgentViewModel[]>>;
  getAgentDetailsAsync(agentId: string, tenantId: string): Promise<WidgetState<AgentViewModel>>;
  getAuthorityScopeAsync(agentId: string, tenantId: string): Promise<WidgetState<AuthorityScopeViewModel>>;
}

interface ITelemetryAdapter {
  logEventAsync(event: TelemetryEvent): Promise<void>;
}

interface IThemeAdapter {
  getCurrentThemeAsync(): Promise<ThemeSettings>;
  onThemeChanged(callback: (settings: ThemeSettings) => void): void;
}

// --- Mock Implementations ---
const mockAgents: AgentViewModel[] = [
  {
    agentId: 'agent-001', agentType: 'ChampionNudger', description: 'Nudges potential champions based on contribution data.', status: 'Active', version: '1.2.0', defaultAutonomy: 'ActWithConfirmation',
    capabilities: ['Data Analysis', 'Notification', 'Recommendation'],
    versionHistory: [
      { version: '1.2.0', changes: 'Improved nudging algorithm.', date: '2024-06-15' },
      { version: '1.1.0', changes: 'Added new data sources.', date: '2024-03-01' },
      { version: '1.0.0', changes: 'Initial release.', date: '2023-12-01' },
    ],
    relationships: [{ type: 'depends_on', targetAgentId: 'agent-006' }]
  },
  {
    agentId: 'agent-002', agentType: 'VelocityRecalibrator', description: 'Analyzes project velocity and suggests recalibrations.', status: 'Active', version: '1.0.5', defaultAutonomy: 'RecommendOnly',
    capabilities: ['Project Management', 'Analytics'],
    versionHistory: [
      { version: '1.0.5', changes: 'Bug fixes and performance improvements.', date: '2024-07-01' },
      { version: '1.0.0', changes: 'Initial release.', date: '2024-01-10' },
    ],
    relationships: []
  },
  {
    agentId: 'agent-003', agentType: 'DataCleansingBot', description: 'Automatically cleans and prepares datasets for analysis.', status: 'Deprecated', version: '0.9.0', defaultAutonomy: 'FullyAutonomous',
    capabilities: ['Data Transformation', 'Automation'],
    versionHistory: [
      { version: '0.9.0', changes: 'Deprecated due to new platform capabilities.', date: '2024-05-20' },
      { version: '0.8.0', changes: 'Added support for new data types.', date: '2024-02-10' },
    ],
    relationships: []
  },
  {
    agentId: 'agent-004', agentType: 'ComplianceAuditor', description: 'Performs routine compliance checks on system configurations.', status: 'Active', version: '2.0.0', defaultAutonomy: 'ActWithConfirmation',
    capabilities: ['Security', 'Audit', 'Compliance'],
    versionHistory: [
      { version: '2.0.0', changes: 'Major update with new compliance standards.', date: '2024-06-01' },
      { version: '1.0.0', changes: 'Initial release.', date: '2023-10-15' },
    ],
    relationships: []
  },
  {
    agentId: 'agent-005', agentType: 'LegacyReportGenerator', description: 'Generates reports from a legacy system.', status: 'Retired', version: '1.0.0', defaultAutonomy: 'FullyAutonomous',
    capabilities: ['Reporting', 'Data Extraction'],
    versionHistory: [
      { version: '1.0.0', changes: 'Retired due to system migration.', date: '2024-04-01' },
    ],
    relationships: []
  },
  {
    agentId: 'agent-006', agentType: 'DataIngestionService', description: 'Ingests data from various sources.', status: 'Active', version: '1.0.0', defaultAutonomy: 'FullyAutonomous',
    capabilities: ['Data Ingestion', 'ETL'],
    versionHistory: [{ version: '1.0.0', changes: 'Initial release.', date: '2023-09-01' }],
    relationships: [{ type: 'dependency_of', targetAgentId: 'agent-001' }]
  }
];

const mockDataAPIAdapter: IDataAPIAdapterPort = {
  getAgentRegistryAsync: async (includeRetired, tenantId) => {
    console.log(`Mock API: Fetching agent registry (includeRetired: ${includeRetired})`);
    return new Promise(resolve => setTimeout(() => {
      const data = includeRetired ? mockAgents : mockAgents.filter(a => a.status !== 'Retired');
      resolve({ data, isStale: false, lastSyncTimestamp: new Date(), lastError: null });
    }, 1000));
  },
  getAgentDetailsAsync: async (agentId, tenantId) => {
    console.log(`Mock API: Fetching details for agent ${agentId}`);
    return new Promise(resolve => setTimeout(() => {
      const agent = mockAgents.find(a => a.agentId === agentId);
      resolve({ data: agent || null, isStale: false, lastSyncTimestamp: new Date(), lastError: null });
    }, 500));
  },
  getAuthorityScopeAsync: async (agentId, tenantId) => {
    console.log(`Mock API: Fetching authority scope for agent ${agentId}`);
    return new Promise(resolve => setTimeout(() => {
      resolve({
        data: {
          allowedApiEndpoints: ['/data/read', '/report/generate'],
          maxResourceConsumption: 100,
          maxBudget: 500,
          dataAccessPolicies: ['read:public', 'read:operational-data'],
        },
        isStale: false,
        lastSyncTimestamp: new Date(),
        lastError: null,
      });
    }, 500));
  },
};

const mockTelemetryAdapter: ITelemetryAdapter = {
  logEventAsync: async (event) => console.log('Telemetry Event:', event),
};

const mockThemeAdapter: IThemeAdapter = {
  getCurrentThemeAsync: async () => ({ name: 'Light', highContrastEnabled: false, languageCode: 'en-US' }),
  onThemeChanged: (callback) => {},
};

// --- Component Styling ---
const getStyles = (theme: ThemeSettings) => {
  const isDark = theme?.name === 'Dark';
  
  return {
    container: { fontFamily: 'Arial, sans-serif', padding: '20px', borderRadius: '8px', boxShadow: '0 2px 8px rgba(0,0,0,0.1)', maxWidth: '1200px', margin: '20px auto', backgroundColor: isDark ? '#2c2c2c' : '#fff', color: isDark ? '#f0f0f0' : '#333' },
    header: { borderBottom: `1px solid ${isDark ? '#444' : '#eee'}`, paddingBottom: '10px', marginBottom: '20px', color: isDark ? '#87ceeb' : '#005a9e' },
    filterBar: { display: 'flex', gap: '15px', marginBottom: '20px', alignItems: 'center', flexWrap: 'wrap' as 'wrap' },
    input: { padding: '8px', borderRadius: '4px', border: `1px solid ${isDark ? '#666' : '#ccc'}`, backgroundColor: isDark ? '#444' : '#f9f9f9', color: isDark ? '#f0f0f0' : '#333' },
    select: { padding: '8px', borderRadius: '4px', border: `1px solid ${isDark ? '#666' : '#ccc'}`, backgroundColor: isDark ? '#444' : '#f9f9f9', color: isDark ? '#f0f0f0' : '#333' },
    button: { padding: '8px 15px', borderRadius: '5px', border: 'none', cursor: 'pointer', fontSize: '14px', marginRight: '10px', backgroundColor: '#007bff', color: 'white' },
    secondaryButton: { backgroundColor: '#6c757d', color: 'white' },
    table: { width: '100%', borderCollapse: 'collapse' },
    th: { borderBottom: `2px solid ${isDark ? '#555' : '#ddd'}`, padding: '12px', textAlign: 'left', fontWeight: 'bold', color: isDark ? '#bbb' : '#333' },
    td: { borderBottom: `1px solid ${isDark ? '#444' : '#eee'}`, padding: '12px', color: isDark ? '#f0f0f0' : '#333' },
    tr: { cursor: 'pointer', transition: 'background-color 0.2s' },
    trHover: { backgroundColor: isDark ? '#3a3a3a' : '#f5f5f5' },
    statusBadge: { padding: '4px 8px', borderRadius: '12px', fontSize: '12px', fontWeight: 'bold', color: 'white' },
    loading: { textAlign: 'center' as 'center', padding: '50px', fontSize: '18px', color: isDark ? '#87ceeb' : '#005a9e' },
    error: { color: '#d9534f', backgroundColor: isDark ? '#5c2c2c' : '#f2dede', border: `1px solid ${isDark ? '#8a4c4c' : '#ebccd1'}`, padding: '15px', borderRadius: '4px', marginBottom: '15px' },
    modalOverlay: { position: 'fixed' as 'fixed', top: 0, left: 0, width: '100%', height: '100%', backgroundColor: 'rgba(0,0,0,0.6)', display: 'flex', justifyContent: 'center', alignItems: 'center', zIndex: 1000 },
    modalContent: { backgroundColor: isDark ? '#333' : 'white', padding: '30px', borderRadius: '8px', width: '90%', maxWidth: '800px', boxShadow: '0 5px 15px rgba(0,0,0,0.3)', color: isDark ? '#eee' : '#333', position: 'relative' as 'relative' },
    modalCloseButton: { position: 'absolute' as 'absolute', top: '10px', right: '10px', background: 'none', border: 'none', fontSize: '24px', cursor: 'pointer', color: isDark ? '#bbb' : '#666' },
    detailGrid: { display: 'grid', gridTemplateColumns: '150px 1fr', gap: '10px', marginBottom: '15px' },
    detailLabel: { fontWeight: 'bold' as 'bold', color: isDark ? '#bbb' : '#555' },
    sectionHeader: { borderBottom: `1px solid ${isDark ? '#555' : '#eee'}`, paddingBottom: '5px', marginBottom: '10px', marginTop: '20px', color: isDark ? '#87ceeb' : '#005a9e' },
    list: { listStyleType: 'disc', marginLeft: '20px' },
    pagination: { display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '10px', marginTop: '20px' },
  };
};

// --- Helper Functions ---
const getStatusColor = (status: AgentViewModel['status']) => {
  switch (status) {
    case 'Active': return '#28a745'; // Green
    case 'Deprecated': return '#ffc107'; // Yellow
    case 'Retired': return '#6c757d'; // Gray
    default: return '#000';
  }
};

const getAutonomyColor = (autonomy: AgentViewModel['defaultAutonomy']) => {
  switch (autonomy) {
    case 'FullyAutonomous': return '#007bff'; // Blue
    case 'ActWithConfirmation': return '#fd7e14'; // Orange
    case 'RecommendOnly': return '#6c757d'; // Gray
    default: return '#000';
  }
};

const formatTimestamp = (dateString: string) => {
  return new Date(dateString).toLocaleString();
};

// --- Sub-components ---
interface AgentDetailsModalProps {
  agent: AgentViewModel;
  scope: AuthorityScopeViewModel | null;
  onClose: () => void;
  theme: ThemeSettings;
}

const AgentDetailsModal: React.FC<AgentDetailsModalProps> = ({ agent, scope, onClose, theme }) => {
  const styles = getStyles(theme);
  return (
    <div style={styles.modalOverlay} role="dialog" aria-modal="true" aria-labelledby="agent-details-title">
      <div style={styles.modalContent}>
        <button style={styles.modalCloseButton} onClick={onClose} aria-label="Close agent details">&times;</button>
        <h2 id="agent-details-title" style={styles.header}>{agent.agentType} Details</h2>
        
        <div style={styles.detailGrid}>
          <span style={styles.detailLabel}>Agent ID:</span><span>{agent.agentId}</span>
          <span style={styles.detailLabel}>Version:</span><span>{agent.version}</span>
          <span style={styles.detailLabel}>Status:</span>
          <span>
            <span style={{ ...styles.statusBadge, backgroundColor: getStatusColor(agent.status) }}>
              {agent.status}
            </span>
          </span>
          <span style={styles.detailLabel}>Default Autonomy:</span>
          <span>
            <span style={{ ...styles.statusBadge, backgroundColor: getAutonomyColor(agent.defaultAutonomy) }}>
              {agent.defaultAutonomy}
            </span>
          </span>
          <span style={styles.detailLabel}>Description:</span><span>{agent.description}</span>
        </div>

        <h3 style={styles.sectionHeader}>Capabilities</h3>
        {agent.capabilities && agent.capabilities.length > 0 ? (
          <ul style={styles.list}>
            {agent.capabilities.map((cap, i) => <li key={i}>{cap}</li>)}
          </ul>
        ) : <p>No capabilities listed.</p>}

        <h3 style={styles.sectionHeader}>Current Authority Scope</h3>
        {scope ? (
          <div style={styles.detailGrid}>
            <span style={styles.detailLabel}>Allowed APIs:</span><span>{scope.allowedApiEndpoints.join(', ') || 'None'}</span>
            <span style={styles.detailLabel}>Max Resources:</span><span>{scope.maxResourceConsumption || 'Unlimited'}</span>
            <span style={styles.detailLabel}>Max Budget:</span><span>${scope.maxBudget || 'Unlimited'}</span>
            <span style={styles.detailLabel}>Data Policies:</span><span>{scope.dataAccessPolicies.join(', ') || 'None'}</span>
          </div>
        ) : <p>Loading authority scope...</p>}

        <h3 style={styles.sectionHeader}>Version History</h3>
        {agent.versionHistory && agent.versionHistory.length > 0 ? (
          <table style={styles.table}>
            <thead>
              <tr>
                <th style={styles.th}>Version</th>
                <th style={styles.th}>Date</th>
                <th style={styles.th}>Changes</th>
              </tr>
            </thead>
            <tbody>
              {agent.versionHistory.map((v, i) => (
                <tr key={i}>
                  <td style={styles.td}>{v.version}</td>
                  <td style={styles.td}>{formatTimestamp(v.date)}</td>
                  <td style={styles.td}>{v.changes}</td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : <p>No version history available.</p>}

        <h3 style={styles.sectionHeader}>Relationships & Dependencies</h3>
        {agent.relationships && agent.relationships.length > 0 ? (
          <ul style={styles.list}>
            {agent.relationships.map((rel, i) => (
              <li key={i}>{rel.type.replace(/_/g, ' ')}: {rel.targetAgentId}</li>
            ))}
          </ul>
        ) : <p>No explicit relationships or dependencies listed.</p>}
      </div>
    </div>
  );
};

// --- Main Component ---
interface RegistryViewerProps {
  userId: string;
  tenantId: string;
  dataAPIAdapter?: IDataAPIAdapterPort;
  telemetryAdapter?: ITelemetryAdapter;
  themeAdapter?: IThemeAdapter;
}

const RegistryViewer: React.FC<RegistryViewerProps> = ({
  userId,
  tenantId,
  dataAPIAdapter = mockDataAPIAdapter,
  telemetryAdapter = mockTelemetryAdapter,
  themeAdapter = mockThemeAdapter,
}) => {
  const [agents, setAgents] = useState<AgentViewModel[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<ErrorEnvelope | null>(null);
  const [filters, setFilters] = useState({ searchText: '', status: 'all', includeRetired: false });
  const [sortBy, setSortBy] = useState<keyof AgentViewModel>('agentType');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [selectedAgent, setSelectedAgent] = useState<AgentViewModel | null>(null);
  const [selectedAgentScope, setSelectedAgentScope] = useState<AuthorityScopeViewModel | null>(null);
  const [currentTheme, setCurrentTheme] = useState<ThemeSettings | null>(null);

  const widgetId = 'registry-viewer';
  const panelId = 'RegistryViewerPanel';

  const fetchAgents = useCallback(async () => {
    setLoading(true);
    setError(null);
    telemetryAdapter.logEventAsync({ timestamp: new Date(), widgetId, panelId, userId, action: TelemetryEventTypes.RegistryViewed, metadata: { tenantId, filters } });
    try {
      const result = await dataAPIAdapter.getAgentRegistryAsync(filters.includeRetired, tenantId);
      if (result.data) {
        setAgents(result.data);
      } else if (result.lastError) {
        setError(result.lastError);
        telemetryAdapter.logEventAsync({ timestamp: new Date(), widgetId, panelId, userId, action: TelemetryEventTypes.ApiCallFailed, errorCode: result.lastError.errorCode, metadata: { error: result.lastError.message } });
      }
    } catch (err) {
      const errEnv = { errorCode: 'FETCH_ERROR', message: 'Failed to load agent registry.', canRetry: true };
      setError(errEnv);
      telemetryAdapter.logEventAsync({ timestamp: new Date(), widgetId, panelId, userId, action: TelemetryEventTypes.ApiCallFailed, errorCode: errEnv.errorCode, metadata: { error: (err as Error).message } });
    } finally {
      setLoading(false);
    }
  }, [filters, tenantId, dataAPIAdapter, telemetryAdapter, userId]);

  useEffect(() => {
    themeAdapter.getCurrentThemeAsync().then(setCurrentTheme);
    themeAdapter.onThemeChanged(setCurrentTheme);
    fetchAgents();
  }, [fetchAgents, themeAdapter]);

  const handleFilterChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    const isCheckbox = type === 'checkbox';
    const checked = (e.target as HTMLInputElement).checked;
    
    setFilters(prev => ({ ...prev, [name]: isCheckbox ? checked : value }));
    setCurrentPage(1); // Reset pagination on filter change
    telemetryAdapter.logEventAsync({ timestamp: new Date(), widgetId, panelId, userId, action: TelemetryEventTypes.RegistryFiltered, metadata: { filter: name, value: isCheckbox ? checked : value } });
  };

  const handleSort = (column: keyof AgentViewModel) => {
    const newSortOrder = sortBy === column && sortOrder === 'asc' ? 'desc' : 'asc';
    setSortBy(column);
    setSortOrder(newSortOrder);
    telemetryAdapter.logEventAsync({ timestamp: new Date(), widgetId, panelId, userId, action: TelemetryEventTypes.RegistrySorted, metadata: { sortBy: column, sortOrder: newSortOrder } });
  };

  const handleAgentSelect = async (agent: AgentViewModel) => {
    setSelectedAgent(agent);
    setSelectedAgentScope(null); // Clear previous scope
    telemetryAdapter.logEventAsync({ timestamp: new Date(), widgetId, panelId, userId, action: TelemetryEventTypes.AgentDetailsViewed, metadata: { agentId: agent.agentId, agentType: agent.agentType } });
    const scopeResult = await dataAPIAdapter.getAuthorityScopeAsync(agent.agentId, tenantId);
    if (scopeResult.data) {
      setSelectedAgentScope(scopeResult.data);
    } else if (scopeResult.lastError) {
      setError(scopeResult.lastError);
    }
  };

  const handleExport = () => {
    const dataToExport = filteredAndSortedAgents.map(agent => ({
      ...agent,
      capabilities: agent.capabilities.join(', '),
      versionHistory: agent.versionHistory.map(v => `${v.version} (${v.date}): ${v.changes}`).join('; '),
      relationships: agent.relationships.map(r => `${r.type}:${r.targetAgentId}`).join('; ')
    }));
    const jsonString = JSON.stringify(dataToExport, null, 2);
    const blob = new Blob([jsonString], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `agent_registry_${new Date().toISOString()}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);

    telemetryAdapter.logEventAsync({ timestamp: new Date(), widgetId, panelId, userId, action: TelemetryEventTypes.RegistryExported, metadata: { format: 'JSON', count: dataToExport.length } });
    alert('Agent registry data exported successfully!');
  };

  const handleRetry = () => {
    setError(null);
    fetchAgents();
  };

  const filteredAgents = agents.filter(agent =>
    (agent.agentType.toLowerCase().includes(filters.searchText.toLowerCase()) ||
     agent.description.toLowerCase().includes(filters.searchText.toLowerCase()) ||
     agent.capabilities.some(cap => cap.toLowerCase().includes(filters.searchText.toLowerCase()))) &&
    (filters.status === 'all' || agent.status.toLowerCase() === filters.status)
  );

  const sortedAgents = [...filteredAgents].sort((a, b) => {
    const aValue = a[sortBy];
    const bValue = b[sortBy];

    if (typeof aValue === 'string' && typeof bValue === 'string') {
      return sortOrder === 'asc' ? aValue.localeCompare(bValue) : bValue.localeCompare(aValue);
    }
    if (typeof aValue === 'number' && typeof bValue === 'number') {
      return sortOrder === 'asc' ? aValue - bValue : bValue - aValue;
    }
    // Fallback for other types or if values are not comparable
    return 0;
  });

  const paginatedAgents = sortedAgents.slice(
    (currentPage - 1) * pageSize,
    currentPage * pageSize
  );

  const totalPages = Math.ceil(sortedAgents.length / pageSize);
  const filteredAndSortedAgents = sortedAgents;

  const styles = getStyles(currentTheme || { name: 'Light', highContrastEnabled: false, languageCode: 'en-US' });

  return (
    <div style={styles.container} role="region" aria-labelledby="registry-viewer-title">
      <h2 id="registry-viewer-title" style={styles.header}>Agent Registry Viewer</h2>
      <p>Comprehensive view of all registered agents, their capabilities, status, and history.</p>
      
      <div style={styles.filterBar}>
        <input
          type="text"
          name="searchText"
          placeholder="Search by type, description, or capability..."
          value={filters.searchText}
          onChange={handleFilterChange}
          style={styles.input}
          aria-label="Search agents"
        />
        <select name="status" value={filters.status} onChange={handleFilterChange} style={styles.select} aria-label="Filter by status">
          <option value="all">All Statuses</option>
          <option value="active">Active</option>
          <option value="deprecated">Deprecated</option>
          <option value="retired">Retired</option>
        </select>
        <label style={{ display: 'flex', alignItems: 'center', gap: '5px' }}>
          <input
            type="checkbox"
            name="includeRetired"
            checked={filters.includeRetired}
            onChange={handleFilterChange}
          />
          Include Retired
        </label>
        <button style={styles.button} onClick={fetchAgents}>Refresh</button>
        <button style={{...styles.button, ...styles.secondaryButton}} onClick={handleExport}>Export Data</button>
      </div>

      {loading ? (
        <p style={styles.loading} aria-live="polite">Loading agent registry...</p>
      ) : error ? (
        <div style={styles.error} role="alert">
          <p>Error: {error.message}</p>
          {error.canRetry && <button style={styles.button} onClick={handleRetry}>Retry</button>}
        </div>
      ) : (
        <>
          <table style={styles.table} aria-label="Agent Registry Data">
            <thead>
              <tr>
                <th style={styles.th} onClick={() => handleSort('agentType')} scope="col" aria-sort={sortBy === 'agentType' ? sortOrder : 'none'}>
                  Agent Type {sortBy === 'agentType' && (sortOrder === 'asc' ? '▲' : '▼')}
                </th>
                <th style={styles.th} onClick={() => handleSort('status')} scope="col" aria-sort={sortBy === 'status' ? sortOrder : 'none'}>
                  Status {sortBy === 'status' && (sortOrder === 'asc' ? '▲' : '▼')}
                </th>
                <th style={styles.th} onClick={() => handleSort('version')} scope="col" aria-sort={sortBy === 'version' ? sortOrder : 'none'}>
                  Version {sortBy === 'version' && (sortOrder === 'asc' ? '▲' : '▼')}
                </th>
                <th style={styles.th}>Default Autonomy</th>
                <th style={styles.th}>Description</th>
                <th style={styles.th}>Capabilities</th>
              </tr>
            </thead>
            <tbody>
              {paginatedAgents.length > 0 ? (
                paginatedAgents.map(agent => (
                  <tr
                    key={agent.agentId}
                    onClick={() => handleAgentSelect(agent)}
                    onMouseOver={(e) => e.currentTarget.style.backgroundColor = styles.trHover.backgroundColor}
                    onMouseOut={(e) => e.currentTarget.style.backgroundColor = 'transparent'}
                    style={styles.tr}
                    tabIndex={0}
                    onKeyPress={(e) => e.key === 'Enter' && handleAgentSelect(agent)}
                  >
                    <td style={styles.td}>{agent.agentType}</td>
                    <td style={styles.td}>
                      <span style={{ ...styles.statusBadge, backgroundColor: getStatusColor(agent.status) }}>
                        {agent.status}
                      </span>
                    </td>
                    <td style={styles.td}>{agent.version}</td>
                    <td style={styles.td}>
                      <span style={{ ...styles.statusBadge, backgroundColor: getAutonomyColor(agent.defaultAutonomy) }}>
                        {agent.defaultAutonomy}
                      </span>
                    </td>
                    <td style={styles.td}>{agent.description}</td>
                    <td style={styles.td}>{agent.capabilities.join(', ')}</td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={6} style={{ ...styles.td, textAlign: 'center' }}>No agents found matching your criteria.</td>
                </tr>
              )}
            </tbody>
          </table>

          <div style={styles.pagination}>
            <button
              style={styles.button}
              onClick={() => setCurrentPage(prev => Math.max(1, prev - 1))}
              disabled={currentPage === 1 || loading}
              aria-label="Previous page"
            >
              Previous
            </button>
            <span aria-live="polite">Page {currentPage} of {totalPages}</span>
            <button
              style={styles.button}
              onClick={() => setCurrentPage(prev => Math.min(totalPages, prev + 1))}
              disabled={currentPage === totalPages || loading}
              aria-label="Next page"
            >
              Next
            </button>
            <select
              value={pageSize}
              onChange={(e) => {
                setPageSize(Number(e.target.value));
                setCurrentPage(1); // Reset to first page when changing page size
              }}
              style={styles.select}
              aria-label="Items per page"
            >
              <option value={5}>5 per page</option>
              <option value={10}>10 per page</option>
              <option value={20}>20 per page</option>
              <option value={50}>50 per page</option>
            </select>
          </div>
        </>
      )}

      {selectedAgent && (
        <AgentDetailsModal
          agent={selectedAgent}
          scope={selectedAgentScope}
          onClose={() => setSelectedAgent(null)}
          theme={currentTheme || { name: 'Light', highContrastEnabled: false, languageCode: 'en-US' }}
        />
      )}
    </div>
  );
};

export default RegistryViewer;
