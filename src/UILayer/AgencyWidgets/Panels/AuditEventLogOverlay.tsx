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

interface AuditEventViewModel {
  auditId: string;
  agentId?: string;
  actionType: string;
  userId: string;
  timestamp: Date;
  outcome: 'Success' | 'Failure' | 'Escalated' | 'Info';
  correlationId?: string;
  eventData: any; // JSON string or object
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
  AuditLogViewed = 'AuditLogViewed',
  AuditLogFiltered = 'AuditLogFiltered',
  AuditLogSearched = 'AuditLogSearched',
  AuditLogExported = 'AuditLogExported',
  ApiCallFailed = 'ApiCallFailed',
}

interface ThemeSettings {
  name: 'Light' | 'Dark';
  highContrastEnabled: boolean;
  languageCode: string;
}

// --- Adapter Port Interfaces ---
interface IDataAPIAdapterPort {
  getAgentAuditEventsAsync(
    agentId: string | undefined,
    since: Date | undefined,
    pageSize: number,
    tenantId: string,
    correlationId?: string,
    eventType?: string,
    userId?: string
  ): Promise<WidgetState<AuditEventViewModel[]>>;
}

interface ITelemetryAdapter {
  logEventAsync(event: TelemetryEvent): Promise<void>;
}

interface IThemeAdapter {
  getCurrentThemeAsync(): Promise<ThemeSettings>;
  onThemeChanged(callback: (settings: ThemeSettings) => void): void;
}

// --- Mock Implementations ---
const mockAuditEvents: AuditEventViewModel[] = [
  {
    auditId: 'audit-001', agentId: 'agent-A', actionType: 'AgentRegistered', userId: 'admin-1',
    timestamp: new Date(Date.now() - 3600000), outcome: 'Success', correlationId: 'corr-1',
    eventData: { agentType: 'DataProcessor', version: '1.0.0' }
  },
  {
    auditId: 'audit-002', agentId: 'agent-B', actionType: 'AgentActionExecuted', userId: 'user-X',
    timestamp: new Date(Date.now() - 1800000), outcome: 'Success', correlationId: 'corr-2',
    eventData: { action: 'ProcessData', inputSize: 1024 }
  },
  {
    auditId: 'audit-003', agentId: 'agent-A', actionType: 'AuthorityOverridden', userId: 'admin-2',
    timestamp: new Date(Date.now() - 900000), outcome: 'Escalated', correlationId: 'corr-3',
    eventData: { reason: 'Emergency fix', oldScope: 'read-only' }
  },
  {
    auditId: 'audit-004', agentId: 'agent-B', actionType: 'AgentActionExecuted', userId: 'user-Y',
    timestamp: new Date(Date.now() - 300000), outcome: 'Failure', correlationId: 'corr-2',
    eventData: { action: 'GenerateReport', errorMessage: 'Database connection failed' }
  },
  {
    auditId: 'audit-005', agentId: 'agent-C', actionType: 'ConsentRequested', userId: 'user-Z',
    timestamp: new Date(Date.now() - 60000), outcome: 'Info', correlationId: 'corr-4',
    eventData: { consentType: 'SensitiveDataAccess' }
  },
  {
    auditId: 'audit-006', agentId: 'agent-C', actionType: 'ConsentGranted', userId: 'user-Z',
    timestamp: new Date(Date.now() - 30000), outcome: 'Success', correlationId: 'corr-4',
    eventData: { consentType: 'SensitiveDataAccess' }
  },
];

const mockDataAPIAdapter: IDataAPIAdapterPort = {
  getAgentAuditEventsAsync: async (agentId, since, pageSize, tenantId, correlationId, eventType, userId) => {
    console.log(`Mock API: Fetching audit events for tenant ${tenantId}`);
    return new Promise(resolve => setTimeout(() => {
      let filteredEvents = mockAuditEvents;

      if (agentId) {
        filteredEvents = filteredEvents.filter(e => e.agentId === agentId);
      }
      if (since) {
        filteredEvents = filteredEvents.filter(e => e.timestamp >= since);
      }
      if (correlationId) {
        filteredEvents = filteredEvents.filter(e => e.correlationId === correlationId);
      }
      if (eventType) {
        filteredEvents = filteredEvents.filter(e => e.actionType === eventType);
      }
      if (userId) {
        filteredEvents = filteredEvents.filter(e => e.userId === userId);
      }

      // Simulate pagination
      const paginatedEvents = filteredEvents.slice(0, pageSize);

      if (Math.random() > 0.95) { // Simulate 5% chance of error
        resolve({
          data: null,
          isStale: false,
          lastSyncTimestamp: new Date(),
          lastError: {
            errorCode: 'API_ERROR',
            message: 'Failed to fetch audit events.',
            canRetry: true
          }
        });
      } else {
        resolve({
          data: paginatedEvents,
          isStale: false,
          lastSyncTimestamp: new Date(),
          lastError: null
        });
      }
    }, 1000));
  }
};

const mockTelemetryAdapter: ITelemetryAdapter = {
  logEventAsync: async (event) => console.log('Telemetry Event:', event)
};

const mockThemeAdapter: IThemeAdapter = {
  getCurrentThemeAsync: async () => ({ name: 'Light', highContrastEnabled: false, languageCode: 'en-US' }),
  onThemeChanged: (callback) => {
    // Simulate theme change after 5 seconds for testing
    setTimeout(() => {
      callback({ name: 'Dark', highContrastEnabled: false, languageCode: 'en-US' });
    }, 5000);
  }
};

// --- Component Styles ---
const getStyles = (theme: ThemeSettings) => {
  const isDark = theme?.name === 'Dark';

  return {
    overlay: {
      position: 'fixed' as 'fixed',
      top: 0,
      left: 0,
      width: '100%',
      height: '100%',
      backgroundColor: 'rgba(0,0,0,0.7)',
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      zIndex: 3000,
    },
    modal: {
      backgroundColor: isDark ? '#2c2c2c' : 'white',
      padding: '30px',
      borderRadius: '8px',
      boxShadow: '0 5px 15px rgba(0,0,0,0.3)',
      width: '90%',
      maxWidth: '900px',
      height: '80%',
      display: 'flex',
      flexDirection: 'column' as 'column',
      color: isDark ? '#f0f0f0' : '#333',
      position: 'relative' as 'relative',
    },
    closeButton: {
      position: 'absolute' as 'absolute',
      top: '10px',
      right: '10px',
      background: 'none',
      border: 'none',
      fontSize: '24px',
      cursor: 'pointer',
      color: isDark ? '#bbb' : '#666',
    },
    header: {
      borderBottom: `1px solid ${isDark ? '#555' : '#eee'}`,
      paddingBottom: '10px',
      marginBottom: '20px',
      color: isDark ? '#87ceeb' : '#005a9e',
    },
    filterBar: {
      display: 'flex',
      gap: '10px',
      marginBottom: '15px',
      flexWrap: 'wrap' as 'wrap',
    },
    input: {
      padding: '8px',
      borderRadius: '4px',
      border: `1px solid ${isDark ? '#666' : '#ccc'}`,
      backgroundColor: isDark ? '#444' : '#f9f9f9',
      color: isDark ? '#f0f0f0' : '#333',
    },
    select: {
      padding: '8px',
      borderRadius: '4px',
      border: `1px solid ${isDark ? '#666' : '#ccc'}`,
      backgroundColor: isDark ? '#444' : '#f9f9f9',
      color: isDark ? '#f0f0f0' : '#333',
    },
    button: {
      padding: '8px 15px',
      borderRadius: '5px',
      border: 'none',
      cursor: 'pointer',
      fontSize: '14px',
      backgroundColor: '#007bff',
      color: 'white',
    },
    secondaryButton: {
      backgroundColor: '#6c757d',
    },
    logContainer: {
      flexGrow: 1,
      overflowY: 'auto' as 'auto',
      border: `1px solid ${isDark ? '#555' : '#ddd'}`,
      borderRadius: '4px',
      padding: '10px',
      marginBottom: '15px',
    },
    logEntry: {
      padding: '10px',
      borderBottom: `1px solid ${isDark ? '#444' : '#eee'}`,
      cursor: 'pointer',
      transition: 'background-color 0.2s ease',
    },
    logEntryHover: {
      backgroundColor: isDark ? '#3a3a3a' : '#f5f5f5',
    },
    logHeader: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      fontWeight: 'bold' as 'bold',
    },
    logDetails: {
      marginTop: '5px',
      fontSize: '0.9em',
      color: isDark ? '#bbb' : '#555',
      whiteSpace: 'pre-wrap' as 'pre-wrap',
      wordBreak: 'break-all' as 'break-all',
    },
    outcomeBadge: {
      padding: '3px 8px',
      borderRadius: '12px',
      fontSize: '11px',
      fontWeight: 'bold' as 'bold',
      color: 'white',
      marginLeft: '10px',
    },
    pagination: {
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      gap: '10px',
      marginTop: '10px',
    },
    loading: {
      textAlign: 'center' as 'center',
      padding: '20px',
      color: isDark ? '#87ceeb' : '#007bff',
    },
    error: {
      color: '#dc3545',
      backgroundColor: '#f8d7da',
      border: '1px solid #f5c6cb',
      padding: '15px',
      borderRadius: '4px',
      marginBottom: '15px',
    },
    darkError: {
      backgroundColor: '#5c2c2c',
      borderColor: '#8a4c4c',
      color: '#f0f0f0',
    },
  };
};

// --- Helper Functions ---
const getOutcomeColor = (outcome: AuditEventViewModel['outcome']) => {
  switch (outcome) {
    case 'Success': return '#28a745'; // Green
    case 'Failure': return '#dc3545'; // Red
    case 'Escalated': return '#fd7e14'; // Orange
    case 'Info': return '#007bff'; // Blue
    default: return '#6c757d'; // Gray
  }
};

const formatTimestamp = (timestamp: Date) => {
  return timestamp.toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: false,
    timeZoneName: 'short',
  });
};

// --- Main Component ---
interface AuditEventLogOverlayProps {
  isOpen: boolean;
  onClose: () => void;
  userId: string;
  tenantId: string;
  dataAPIAdapter?: IDataAPIAdapterPort;
  telemetryAdapter?: ITelemetryAdapter;
  themeAdapter?: IThemeAdapter;
}

const AuditEventLogOverlay: React.FC<AuditEventLogOverlayProps> = ({
  isOpen,
  onClose,
  userId,
  tenantId,
  dataAPIAdapter = mockDataAPIAdapter,
  telemetryAdapter = mockTelemetryAdapter,
  themeAdapter = mockThemeAdapter,
}) => {
  const [logs, setLogs] = useState<AuditEventViewModel[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<ErrorEnvelope | null>(null);
  const [currentTheme, setCurrentTheme] = useState<ThemeSettings>({ name: 'Light', highContrastEnabled: false, languageCode: 'en-US' });
  const [expandedLogId, setExpandedLogId] = useState<string | null>(null);

  // Filters and Pagination
  const [filterAgentId, setFilterAgentId] = useState('');
  const [filterEventType, setFilterEventType] = useState('');
  const [filterCorrelationId, setFilterCorrelationId] = useState('');
  const [filterUserId, setFilterUserId] = useState('');
  const [pageSize, setPageSize] = useState(10);
  const [currentPage, setCurrentPage] = useState(1);

  const widgetId = 'audit-event-log-overlay';
  const panelId = 'AuditEventLogOverlayPanel';

  const fetchLogs = useCallback(async () => {
    setLoading(true);
    setError(null);
    telemetryAdapter.logEventAsync({
      timestamp: new Date(),
      widgetId,
      panelId,
      userId,
      action: TelemetryEventTypes.AuditLogViewed,
      metadata: { tenantId, filters: { filterAgentId, filterEventType, filterCorrelationId, filterUserId, pageSize, currentPage } }
    });

    try {
      const result = await dataAPIAdapter.getAgentAuditEventsAsync(
        filterAgentId || undefined,
        undefined, // 'since' not directly mapped to filter inputs yet
        pageSize,
        tenantId,
        filterCorrelationId || undefined,
        filterEventType || undefined,
        filterUserId || undefined
      );

      if (result.data) {
        setLogs(result.data);
      } else if (result.lastError) {
        setError(result.lastError);
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          action: TelemetryEventTypes.ApiCallFailed,
          errorCode: result.lastError.errorCode,
          metadata: { error: result.lastError.message, api: 'getAgentAuditEvents' }
        });
      }
    } catch (err) {
      const errEnv = { errorCode: 'FETCH_ERROR', message: 'Failed to load audit logs.', canRetry: true };
      setError(errEnv);
      telemetryAdapter.logEventAsync({
        timestamp: new Date(),
        widgetId,
        panelId,
        userId,
        action: TelemetryEventTypes.ApiCallFailed,
        errorCode: errEnv.errorCode,
        metadata: { error: (err as Error).message }
      });
    } finally {
      setLoading(false);
    }
  }, [dataAPIAdapter, tenantId, userId, telemetryAdapter, filterAgentId, filterEventType, filterCorrelationId, filterUserId, pageSize, currentPage]);

  useEffect(() => {
    if (isOpen) {
      themeAdapter.getCurrentThemeAsync().then(setCurrentTheme);
      themeAdapter.onThemeChanged(setCurrentTheme);
      fetchLogs();
    }
  }, [isOpen, fetchLogs, themeAdapter]);

  // Handle escape key to close modal
  useEffect(() => {
    const handleEscapeKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen) {
        onClose();
      }
    };
    document.addEventListener('keydown', handleEscapeKey);
    return () => {
      document.removeEventListener('keydown', handleEscapeKey);
    };
  }, [isOpen, onClose]);

  const handleFilterChange = (setter: React.Dispatch<React.SetStateAction<string>>) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    setter(e.target.value);
    telemetryAdapter.logEventAsync({
      timestamp: new Date(),
      widgetId,
      panelId,
      userId,
      action: TelemetryEventTypes.AuditLogFiltered,
      metadata: { filterName: e.target.name, filterValue: e.target.value }
    });
  };

  const handleSearch = () => {
    setCurrentPage(1); // Reset to first page on new search
    fetchLogs();
    telemetryAdapter.logEventAsync({
      timestamp: new Date(),
      widgetId,
      panelId,
      userId,
      action: TelemetryEventTypes.AuditLogSearched,
      metadata: { searchText: filterEventType, agentId: filterAgentId, correlationId: filterCorrelationId, userId: filterUserId }
    });
  };

  const handleExport = () => {
    const jsonString = JSON.stringify(logs, null, 2);
    const blob = new Blob([jsonString], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `audit_logs_${new Date().toISOString()}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);

    telemetryAdapter.logEventAsync({
      timestamp: new Date(),
      widgetId,
      panelId,
      userId,
      action: TelemetryEventTypes.AuditLogExported,
      metadata: { logCount: logs.length, format: 'JSON' }
    });
    alert('Logs exported successfully!');
  };

  const handleRetry = () => {
    setError(null);
    fetchLogs();
  };

  if (!isOpen) return null;

  const styles = getStyles(currentTheme);

  return (
    <div style={styles.overlay} role="dialog" aria-modal="true" aria-labelledby="audit-log-title">
      <div style={styles.modal}>
        <button style={styles.closeButton} onClick={onClose} aria-label="Close audit log overlay">&times;</button>
        <h2 id="audit-log-title" style={styles.header}>Agent Audit Log</h2>

        <div style={styles.filterBar}>
          <input
            type="text"
            name="eventType"
            placeholder="Filter by Event Type..."
            value={filterEventType}
            onChange={handleFilterChange(setFilterEventType)}
            style={styles.input}
            aria-label="Filter by event type"
          />
          <input
            type="text"
            name="agentId"
            placeholder="Filter by Agent ID..."
            value={filterAgentId}
            onChange={handleFilterChange(setFilterAgentId)}
            style={styles.input}
            aria-label="Filter by agent ID"
          />
          <input
            type="text"
            name="correlationId"
            placeholder="Filter by Correlation ID..."
            value={filterCorrelationId}
            onChange={handleFilterChange(setFilterCorrelationId)}
            style={styles.input}
            aria-label="Filter by correlation ID"
          />
          <input
            type="text"
            name="userId"
            placeholder="Filter by User ID..."
            value={filterUserId}
            onChange={handleFilterChange(setFilterUserId)}
            style={styles.input}
            aria-label="Filter by user ID"
          />
          <button style={styles.button} onClick={handleSearch}>Search</button>
          <button style={{ ...styles.button, ...styles.secondaryButton }} onClick={handleExport}>Export Logs</button>
        </div>

        {loading ? (
          <p style={styles.loading} aria-live="polite">Loading audit events...</p>
        ) : error ? (
          <div style={currentTheme.name === 'Dark' ? { ...styles.error, ...styles.darkError } : styles.error} role="alert">
            <p>Error: {error.message}</p>
            {error.canRetry && <button style={styles.button} onClick={handleRetry}>Retry</button>}
          </div>
        ) : (
          <div style={styles.logContainer} role="log" aria-live="polite">
            {logs.length > 0 ? (
              logs.map((log) => (
                <div
                  key={log.auditId}
                  style={styles.logEntry}
                  onClick={() => setExpandedLogId(expandedLogId === log.auditId ? null : log.auditId)}
                  onMouseOver={(e) => (e.currentTarget.style.backgroundColor = styles.logEntryHover.backgroundColor)}
                  onMouseOut={(e) => (e.currentTarget.style.backgroundColor = 'transparent')}
                  tabIndex={0}
                  onKeyPress={(e) => e.key === 'Enter' && setExpandedLogId(expandedLogId === log.auditId ? null : log.auditId)}
                  aria-expanded={expandedLogId === log.auditId}
                  aria-controls={`log-details-${log.auditId}`}
                >
                  <div style={styles.logHeader}>
                    <span>
                      {formatTimestamp(log.timestamp)} - <strong>{log.actionType}</strong>
                      <span style={{ ...styles.outcomeBadge, backgroundColor: getOutcomeColor(log.outcome) }}>
                        {log.outcome}
                      </span>
                    </span>
                    <span>
                      Agent: {log.agentId || 'N/A'} | User: {log.userId || 'N/A'}
                    </span>
                  </div>
                  {log.correlationId && (
                    <div style={{ fontSize: '0.85em', color: currentTheme.name === 'Dark' ? '#aaa' : '#777' }}>
                      Correlation ID: {log.correlationId}
                    </div>
                  )}
                  {expandedLogId === log.auditId && (
                    <div id={`log-details-${log.auditId}`} style={styles.logDetails}>
                      <h4>Event Data:</h4>
                      <pre>{JSON.stringify(log.eventData, null, 2)}</pre>
                    </div>
                  )}
                </div>
              ))
            ) : (
              <p style={{ textAlign: 'center', color: currentTheme.name === 'Dark' ? '#bbb' : '#666' }}>No audit events found matching your criteria.</p>
            )}
          </div>
        )}

        <div style={styles.pagination}>
          <button
            style={styles.button}
            onClick={() => setCurrentPage(prev => Math.max(1, prev - 1))}
            disabled={currentPage === 1 || loading}
            aria-label="Previous page"
          >
            Previous
          </button>
          <span aria-live="polite">Page {currentPage}</span>
          <button
            style={styles.button}
            onClick={() => setCurrentPage(prev => prev + 1)}
            disabled={loading || logs.length < pageSize}
            aria-label="Next page"
          >
            Next
          </button>
          <select
            name="pageSize"
            value={pageSize}
            onChange={handleFilterChange(val => setPageSize(parseInt(val)))}
            style={styles.select}
            aria-label="Items per page"
          >
            <option value={5}>5 per page</option>
            <option value={10}>10 per page</option>
            <option value={20}>20 per page</option>
          </select>
        </div>
      </div>
    </div>
  );
};

export default AuditEventLogOverlay;
