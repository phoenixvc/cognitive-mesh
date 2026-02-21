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
  AgentRegistryViewed = 'AgentRegistryViewed',
  AgentDetailsViewed = 'AgentDetailsViewed',
  AgentFilterChanged = 'AgentFilterChanged',
  AuthorityOverrideAttempted = 'AuthorityOverrideAttempted',
  AuthorityOverrideSucceeded = 'AuthorityOverrideSucceeded',
  AgentEscalated = 'AgentEscalated',
  ApiCallFailed = 'ApiCallFailed',
}

interface ThemeSettings {
  name: 'Light' | 'Dark';
  highContrastEnabled: boolean;
  languageCode: string;
}

interface IDataAPIAdapterPort {
  getAgentRegistryAsync(includeRetired: boolean, tenantId: string): Promise<WidgetState<AgentViewModel[]>>;
  getAgentDetailsAsync(agentId: string, tenantId: string): Promise<WidgetState<AgentViewModel>>;
  getAuthorityScopeAsync(agentId: string, tenantId: string): Promise<WidgetState<AuthorityScopeViewModel>>;
  updateAuthorityScopeAsync(agentId: string, newScope: AuthorityScopeViewModel, tenantId: string, reason: string): Promise<boolean>;
}

interface ITelemetryAdapter {
  logEventAsync(event: TelemetryEvent): Promise<void>;
}

interface IThemeAdapter {
  getCurrentThemeAsync(): Promise<ThemeSettings>;
  onThemeChanged(callback: (settings: ThemeSettings) => void): void;
}

// --- Mock Implementations for Demonstration ---

const mockAgents: AgentViewModel[] = [
  { agentId: 'agent-001', agentType: 'ChampionNudger', description: 'Nudges potential champions based on contribution data.', status: 'Active', version: '1.2.0', defaultAutonomy: 'ActWithConfirmation' },
  { agentId: 'agent-002', agentType: 'VelocityRecalibrator', description: 'Analyzes project velocity and suggests recalibrations.', status: 'Active', version: '1.0.5', defaultAutonomy: 'RecommendOnly' },
  { agentId: 'agent-003', agentType: 'DataCleansingBot', description: 'Automatically cleans and prepares datasets for analysis.', status: 'Deprecated', version: '0.9.0', defaultAutonomy: 'FullyAutonomous' },
  { agentId: 'agent-004', agentType: 'ComplianceAuditor', description: 'Performs routine compliance checks on system configurations.', status: 'Active', version: '2.0.0', defaultAutonomy: 'ActWithConfirmation' },
  { agentId: 'agent-005', agentType: 'LegacyReportGenerator', description: 'Generates reports from a legacy system.', status: 'Retired', version: '1.0.0', defaultAutonomy: 'FullyAutonomous' },
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
  updateAuthorityScopeAsync: async (agentId, newScope, tenantId, reason) => {
    console.log(`Mock API: Updating authority for ${agentId}. Reason: ${reason}`, newScope);
    return new Promise(resolve => setTimeout(() => resolve(true), 1000));
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
const widgetStyles: { [key: string]: CSSProperties } = {
    container: { fontFamily: 'Arial, sans-serif', padding: '20px', borderRadius: '8px', boxShadow: '0 2px 8px rgba(0,0,0,0.1)', maxWidth: '1200px', margin: '20px auto', backgroundColor: '#fff', color: '#333' },
    darkContainer: { backgroundColor: '#2c2c2c', color: '#f0f0f0' },
    header: { borderBottom: '1px solid #eee', paddingBottom: '10px', marginBottom: '20px', color: '#005a9e' },
    darkHeader: { borderColor: '#444', color: '#87ceeb' },
    filterBar: { display: 'flex', gap: '15px', marginBottom: '20px', alignItems: 'center' },
    input: { padding: '8px', borderRadius: '4px', border: '1px solid #ccc' },
    darkInput: { backgroundColor: '#444', borderColor: '#666', color: '#f0f0f0' },
    table: { width: '100%', borderCollapse: 'collapse' },
    th: { borderBottom: '2px solid #ddd', padding: '12px', textAlign: 'left', fontWeight: 'bold' },
    darkTh: { borderColor: '#555' },
    td: { borderBottom: '1px solid #eee', padding: '12px' },
    darkTd: { borderColor: '#444' },
    tr: { cursor: 'pointer', transition: 'background-color 0.2s' },
    trHover: { backgroundColor: '#f5f5f5' },
    darkTrHover: { backgroundColor: '#3a3a3a' },
    statusBadge: { padding: '4px 8px', borderRadius: '12px', fontSize: '12px', fontWeight: 'bold', color: 'white' },
    loading: { textAlign: 'center', padding: '50px', fontSize: '18px', color: '#005a9e' },
    error: { color: '#d9534f', backgroundColor: '#f2dede', border: '1px solid #ebccd1', padding: '15px', borderRadius: '4px', marginBottom: '15px' },
    button: { padding: '10px 15px', borderRadius: '5px', border: 'none', cursor: 'pointer', fontSize: '14px', marginRight: '10px' },
    primaryButton: { backgroundColor: '#007bff', color: 'white' },
    secondaryButton: { backgroundColor: '#6c757d', color: 'white' },
    modalOverlay: { position: 'fixed', top: 0, left: 0, width: '100%', height: '100%', backgroundColor: 'rgba(0,0,0,0.6)', display: 'flex', justifyContent: 'center', alignItems: 'center', zIndex: 1000 },
    modalContent: { backgroundColor: 'white', padding: '30px', borderRadius: '8px', width: '90%', maxWidth: '600px', boxShadow: '0 5px 15px rgba(0,0,0,0.3)' },
    darkModalContent: { backgroundColor: '#333', color: 'white' },
    modalHeader: { fontSize: '24px', marginBottom: '20px' },
    modalCloseButton: { float: 'right', background: 'none', border: 'none', fontSize: '24px', cursor: 'pointer' },
    detailGrid: { display: 'grid', gridTemplateColumns: '150px 1fr', gap: '10px' },
    detailLabel: { fontWeight: 'bold' },
};

// --- Sub-components ---

const AgentDetailsModal = ({ agent, scope, onClose, onOverride, onEscalate, theme }) => (
    <div style={widgetStyles.modalOverlay} role="dialog" aria-modal="true" aria-labelledby="details-title">
        <div style={theme.name === 'Dark' ? { ...widgetStyles.modalContent, ...widgetStyles.darkModalContent } : widgetStyles.modalContent}>
            <button style={{ ...widgetStyles.modalCloseButton, color: theme.name === 'Dark' ? 'white' : 'black' }} onClick={onClose}>&times;</button>
            <h3 id="details-title" style={widgetStyles.modalHeader}>{agent.agentType} Details</h3>
            <div style={widgetStyles.detailGrid}>
                <span style={widgetStyles.detailLabel}>Agent ID:</span><span>{agent.agentId}</span>
                <span style={widgetStyles.detailLabel}>Version:</span><span>{agent.version}</span>
                <span style={widgetStyles.detailLabel}>Status:</span><span>{agent.status}</span>
                <span style={widgetStyles.detailLabel}>Description:</span><span>{agent.description}</span>
                <span style={widgetStyles.detailLabel}>Default Autonomy:</span><span>{agent.defaultAutonomy}</span>
            </div>
            <h4 style={{ marginTop: '20px' }}>Current Authority Scope</h4>
            {scope ? (
                <div style={widgetStyles.detailGrid}>
                    <span style={widgetStyles.detailLabel}>Allowed APIs:</span><span>{scope.allowedApiEndpoints.join(', ')}</span>
                    <span style={widgetStyles.detailLabel}>Max Budget:</span><span>${scope.maxBudget}</span>
                    <span style={widgetStyles.detailLabel}>Data Policies:</span><span>{scope.dataAccessPolicies.join(', ')}</span>
                </div>
            ) : <p>Loading scope...</p>}
            <div style={{ marginTop: '30px', textAlign: 'right' }}>
                <button style={{ ...widgetStyles.button, ...widgetStyles.secondaryButton }} onClick={() => onEscalate(agent)}>Escalate for Review</button>
                <button style={{ ...widgetStyles.button, ...widgetStyles.primaryButton }} onClick={() => onOverride(agent)}>Override Authority</button>
            </div>
        </div>
    </div>
);

// --- Main Component ---

interface AgentControlCenterProps {
  userId: string;
  tenantId: string;
  dataAPIAdapter?: IDataAPIAdapterPort;
  telemetryAdapter?: ITelemetryAdapter;
  themeAdapter?: IThemeAdapter;
}

const AgentControlCenter: React.FC<AgentControlCenterProps> = ({
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
  const [selectedAgent, setSelectedAgent] = useState<AgentViewModel | null>(null);
  const [selectedAgentScope, setSelectedAgentScope] = useState<AuthorityScopeViewModel | null>(null);
  const [currentTheme, setCurrentTheme] = useState<ThemeSettings | null>(null);

  const widgetId = 'agent-control-center';
  const panelId = 'AgentControlCenterPanel';

  const fetchAgents = useCallback(async () => {
    setLoading(true);
    setError(null);
    telemetryAdapter.logEventAsync({ timestamp: new Date(), widgetId, panelId, userId, action: 'FetchAgentRegistryAttempt', metadata: { filters } });
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
    telemetryAdapter.logEventAsync({ timestamp: new Date(), widgetId, panelId, userId, action: 'WidgetViewed', metadata: { tenantId } });
    fetchAgents();
  }, [fetchAgents, themeAdapter, telemetryAdapter, userId, tenantId]);

  const handleFilterChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    const isCheckbox = type === 'checkbox';
    const checked = (e.target as HTMLInputElement).checked;
    
    setFilters(prev => ({ ...prev, [name]: isCheckbox ? checked : value }));
    telemetryAdapter.logEventAsync({ timestamp: new Date(), widgetId, panelId, userId, action: TelemetryEventTypes.AgentFilterChanged, metadata: { filter: name, value: isCheckbox ? checked : value } });
  };

  const handleAgentSelect = async (agent: AgentViewModel) => {
    setSelectedAgent(agent);
    setSelectedAgentScope(null); // Clear previous scope
    telemetryAdapter.logEventAsync({ timestamp: new Date(), widgetId, panelId, userId, action: TelemetryEventTypes.AgentDetailsViewed, metadata: { agentId: agent.agentId, agentType: agent.agentType } });
    const scopeResult = await dataAPIAdapter.getAuthorityScopeAsync(agent.agentId, tenantId);
    if (scopeResult.data) {
      setSelectedAgentScope(scopeResult.data);
    }
  };

  const handleEscalate = (agent: AgentViewModel) => {
    alert(`Escalating agent ${agent.agentType} for review.`);
    telemetryAdapter.logEventAsync({ timestamp: new Date(), widgetId, panelId, userId, action: TelemetryEventTypes.AgentEscalated, metadata: { agentId: agent.agentId } });
    setSelectedAgent(null);
  };

  const handleOverride = (agent: AgentViewModel) => {
    alert(`Opening authority override for agent ${agent.agentType}.`);
    // In a real app, this would open a more complex form/modal
    telemetryAdapter.logEventAsync({ timestamp: new Date(), widgetId, panelId, userId, action: TelemetryEventTypes.AuthorityOverrideAttempted, metadata: { agentId: agent.agentId } });
    setSelectedAgent(null);
  };

  const filteredAgents = agents.filter(agent =>
    (agent.agentType.toLowerCase().includes(filters.searchText.toLowerCase()) ||
     agent.description.toLowerCase().includes(filters.searchText.toLowerCase())) &&
    (filters.status === 'all' || agent.status.toLowerCase() === filters.status)
  );

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Active': return '#28a745';
      case 'Deprecated': return '#ffc107';
      case 'Retired': return '#6c757d';
      default: return '#000';
    }
  };

  const containerStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.container, ...widgetStyles.darkContainer } : widgetStyles.container;
  const headerStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.header, ...widgetStyles.darkHeader } : widgetStyles.header;
  const inputStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.input, ...widgetStyles.darkInput } : widgetStyles.input;
  const thStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.th, ...widgetStyles.darkTh } : widgetStyles.th;
  const tdStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.td, ...widgetStyles.darkTd } : widgetStyles.td;

  return (
    <div style={containerStyle} role="region" aria-labelledby="acc-title">
      <h2 id="acc-title" style={headerStyle}>Agent Control Center</h2>
      
      <div style={widgetStyles.filterBar}>
        <input
          type="text"
          name="searchText"
          placeholder="Search agents..."
          value={filters.searchText}
          onChange={handleFilterChange}
          style={inputStyle}
          aria-label="Search agents"
        />
        <select name="status" value={filters.status} onChange={handleFilterChange} style={inputStyle} aria-label="Filter by status">
          <option value="all">All Statuses</option>
          <option value="active">Active</option>
          <option value="deprecated">Deprecated</option>
        </select>
        <label>
          <input
            type="checkbox"
            name="includeRetired"
            checked={filters.includeRetired}
            onChange={handleFilterChange}
          />
          Include Retired
        </label>
      </div>

      {loading ? (
        <p style={widgetStyles.loading} aria-live="polite">Loading agents...</p>
      ) : error ? (
        <div style={widgetStyles.error} role="alert">
          <p>Error: {error.message}</p>
          {error.canRetry && <button style={{...widgetStyles.button, ...widgetStyles.primaryButton}} onClick={fetchAgents}>Retry</button>}
        </div>
      ) : (
        <table style={widgetStyles.table} aria-label="Agent Registry">
          <thead>
            <tr>
              <th style={thStyle}>Agent Type</th>
              <th style={thStyle}>Status</th>
              <th style={thStyle}>Version</th>
              <th style={thStyle}>Default Autonomy</th>
              <th style={thStyle}>Description</th>
            </tr>
          </thead>
          <tbody>
            {filteredAgents.length > 0 ? (
              filteredAgents.map(agent => (
                <tr
                  key={agent.agentId}
                  onClick={() => handleAgentSelect(agent)}
                  onMouseOver={(e) => e.currentTarget.style.backgroundColor = currentTheme?.name === 'Dark' ? widgetStyles.darkTrHover.backgroundColor : widgetStyles.trHover.backgroundColor}
                  onMouseOut={(e) => e.currentTarget.style.backgroundColor = 'transparent'}
                  style={widgetStyles.tr}
                  tabIndex={0}
                  onKeyPress={(e) => e.key === 'Enter' && handleAgentSelect(agent)}
                >
                  <td style={tdStyle}>{agent.agentType}</td>
                  <td style={tdStyle}>
                    <span style={{ ...widgetStyles.statusBadge, backgroundColor: getStatusColor(agent.status) }}>
                      {agent.status}
                    </span>
                  </td>
                  <td style={tdStyle}>{agent.version}</td>
                  <td style={tdStyle}>{agent.defaultAutonomy}</td>
                  <td style={tdStyle}>{agent.description}</td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan={5} style={{ ...tdStyle, textAlign: 'center' }}>No agents found matching your criteria.</td>
              </tr>
            )}
          </tbody>
        </table>
      )}

      {selectedAgent && (
        <AgentDetailsModal
          agent={selectedAgent}
          scope={selectedAgentScope}
          onClose={() => setSelectedAgent(null)}
          onOverride={handleOverride}
          onEscalate={handleEscalate}
          theme={currentTheme || { name: 'Light' }}
        />
      )}
    </div>
  );
};

export default AgentControlCenter;
