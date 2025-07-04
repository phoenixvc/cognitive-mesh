import React, { useState, useEffect, useCallback, CSSProperties } from 'react';

// --- Mock Adapter Interfaces and Models (as defined in IAgencyWidgetAdapters.cs) ---
// These are simplified versions for Storybook/testing purposes.
// In a real application, these would be imported from a shared library.

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
  userId?: string;
  timestamp: Date;
  outcome: 'Success' | 'Failure' | 'Escalated' | 'Info';
  correlationId?: string;
  eventData: any; // JSON string or object
}

interface ThemeSettings {
  name: 'Light' | 'Dark';
  highContrastEnabled: boolean;
  languageCode: string;
}

// --- Mock Implementations (for demonstration purposes) ---
// In a real application, these would be concrete implementations provided via DI.

const mockThemeAdapter = {
  getCurrentThemeAsync: async () => ({ name: 'Light', highContrastEnabled: false, languageCode: 'en-US' }),
  onThemeChanged: (callback: (settings: ThemeSettings) => void) => {
    // Simulate theme change after 5 seconds for testing
    setTimeout(() => {
      callback({ name: 'Dark', highContrastEnabled: false, languageCode: 'en-US' });
    }, 5000);
  },
};

// --- Component Styles ---
const getStyles = (theme: ThemeSettings) => {
  const isDark = theme?.name === 'Dark';

  return {
    container: {
      fontFamily: 'Arial, sans-serif',
      padding: '20px',
      borderRadius: '8px',
      boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
      maxWidth: '900px',
      margin: '20px auto',
      backgroundColor: isDark ? '#2c2c2c' : '#fff',
      color: isDark ? '#f0f0f0' : '#333',
    },
    header: {
      borderBottom: `1px solid ${isDark ? '#444' : '#eee'}`,
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
      marginRight: '10px',
      backgroundColor: '#007bff',
      color: 'white',
    },
    secondaryButton: {
      backgroundColor: '#6c757d',
      color: 'white',
    },
    logContainer: {
      border: `1px solid ${isDark ? '#555' : '#ddd'}`,
      borderRadius: '4px',
      padding: '10px',
      marginBottom: '15px',
      maxHeight: '600px', // Fixed height for scrollability
      overflowY: 'auto' as 'auto',
    },
    logGroup: {
      marginBottom: '15px',
      border: `1px solid ${isDark ? '#666' : '#ccc'}`,
      borderRadius: '4px',
      padding: '10px',
      backgroundColor: isDark ? '#3a3a3a' : '#f8f8f8',
    },
    logGroupHeader: {
      fontWeight: 'bold' as 'bold',
      marginBottom: '10px',
      color: isDark ? '#87ceeb' : '#005a9e',
      cursor: 'pointer',
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
      backgroundColor: isDark ? '#5c2c2c' : '#f8d7da',
      border: `1px solid ${isDark ? '#8a4c4c' : '#f5c6cb'}`,
      padding: '15px',
      borderRadius: '4px',
      marginBottom: '15px',
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

const formatRelativeTime = (timestamp: Date): string => {
  const now = new Date();
  const seconds = Math.floor((now.getTime() - timestamp.getTime()) / 1000);

  if (seconds < 60) return `${seconds} seconds ago`;
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes} minutes ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours} hours ago`;
  const days = Math.floor(hours / 24);
  if (days < 30) return `${days} days ago`;
  const months = Math.floor(days / 30);
  if (months < 12) return `${months} months ago`;
  const years = Math.floor(months / 12);
  return `${years} years ago`;
};

// --- Main Component ---
interface AgentActionAuditTrailProps {
  auditEvents: AuditEventViewModel[]; // Pre-fetched audit data
  userId: string;
  tenantId: string;
  themeAdapter?: {
    getCurrentThemeAsync: () => Promise<ThemeSettings>;
    onThemeChanged: (callback: (settings: ThemeSettings) => void) => void;
  };
}

const AgentActionAuditTrail: React.FC<AgentActionAuditTrailProps> = ({
  auditEvents,
  userId,
  tenantId,
  themeAdapter = mockThemeAdapter,
}) => {
  const [currentTheme, setCurrentTheme] = useState<ThemeSettings>({ name: 'Light', highContrastEnabled: false, languageCode: 'en-US' });
  const [expandedLogId, setExpandedLogId] = useState<string | null>(null);

  // Filters and Pagination
  const [filterEventType, setFilterEventType] = useState('');
  const [filterOutcome, setFilterOutcome] = useState('all');
  const [filterCorrelationId, setFilterCorrelationId] = useState('');
  const [pageSize, setPageSize] = useState(10);
  const [currentPage, setCurrentPage] = useState(1);

  const styles = getStyles(currentTheme);

  useEffect(() => {
    themeAdapter.getCurrentThemeAsync().then(setCurrentTheme);
    themeAdapter.onThemeChanged(setCurrentTheme);
  }, [themeAdapter]);

  const handleFilterChange = (setter: React.Dispatch<React.SetStateAction<string>>) => (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    setter(e.target.value);
    setCurrentPage(1); // Reset to first page on filter change
  };

  const toggleExpand = (auditId: string) => {
    setExpandedLogId(expandedLogId === auditId ? null : auditId);
  };

  const filteredEvents = auditEvents.filter(log => {
    const matchesEventType = filterEventType === '' || log.actionType.toLowerCase().includes(filterEventType.toLowerCase());
    const matchesOutcome = filterOutcome === 'all' || log.outcome.toLowerCase() === filterOutcome.toLowerCase();
    const matchesCorrelationId = filterCorrelationId === '' || (log.correlationId && log.correlationId.toLowerCase().includes(filterCorrelationId.toLowerCase()));
    return matchesEventType && matchesOutcome && matchesCorrelationId;
  });

  // Group events by correlationId
  const groupedEvents = filteredEvents.reduce((acc, event) => {
    const key = event.correlationId || 'no-correlation-id';
    if (!acc[key]) {
      acc[key] = [];
    }
    acc[key].push(event);
    return acc;
  }, {} as Record<string, AuditEventViewModel[]>);

  const sortedGroupKeys = Object.keys(groupedEvents).sort((a, b) => {
    const firstEventA = groupedEvents[a][0];
    const firstEventB = groupedEvents[b][0];
    return firstEventB.timestamp.getTime() - firstEventA.timestamp.getTime(); // Sort groups by latest event
  });

  const paginatedGroupKeys = sortedGroupKeys.slice(
    (currentPage - 1) * pageSize,
    currentPage * pageSize
  );

  const totalPages = Math.ceil(sortedGroupKeys.length / pageSize);

  return (
    <div style={styles.container} role="region" aria-labelledby="audit-trail-title">
      <h2 id="audit-trail-title" style={styles.header}>Agent Action Audit Trail</h2>
      <p>Detailed log of all agent activities, consent events, and system interactions.</p>

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
        <select name="outcome" value={filterOutcome} onChange={handleFilterChange(setFilterOutcome)} style={styles.select} aria-label="Filter by outcome">
          <option value="all">All Outcomes</option>
          <option value="Success">Success</option>
          <option value="Failure">Failure</option>
          <option value="Escalated">Escalated</option>
          <option value="Info">Info</option>
        </select>
        <input
          type="text"
          name="correlationId"
          placeholder="Filter by Correlation ID..."
          value={filterCorrelationId}
          onChange={handleFilterChange(setFilterCorrelationId)}
          style={styles.input}
          aria-label="Filter by correlation ID"
        />
      </div>

      <div style={styles.logContainer} role="log" aria-live="polite">
        {paginatedGroupKeys.length > 0 ? (
          paginatedGroupKeys.map(groupKey => (
            <div key={groupKey} style={styles.logGroup}>
              <h3
                style={styles.logGroupHeader}
                onClick={() => toggleExpand(groupKey)}
                onKeyPress={(e) => e.key === 'Enter' && toggleExpand(groupKey)}
                tabIndex={0}
                role="button"
                aria-expanded={expandedLogId === groupKey}
                aria-controls={`group-details-${groupKey}`}
              >
                Correlation ID: {groupKey === 'no-correlation-id' ? 'N/A (No Correlation ID)' : groupKey} ({groupedEvents[groupKey].length} events)
                <span style={{ float: 'right' }}>{expandedLogId === groupKey ? '▲' : '▼'}</span>
              </h3>
              {expandedLogId === groupKey && (
                <div id={`group-details-${groupKey}`}>
                  {groupedEvents[groupKey].map((log) => (
                    <div
                      key={log.auditId}
                      style={styles.logEntry}
                      onClick={() => toggleExpand(log.auditId)}
                      onMouseOver={(e) => (e.currentTarget.style.backgroundColor = styles.logEntryHover.backgroundColor)}
                      onMouseOut={(e) => (e.currentTarget.style.backgroundColor = 'transparent')}
                      tabIndex={0}
                      onKeyPress={(e) => e.key === 'Enter' && toggleExpand(log.auditId)}
                      aria-expanded={expandedLogId === log.auditId}
                      aria-controls={`log-details-${log.auditId}`}
                    >
                      <div style={styles.logHeader}>
                        <span>
                          {formatRelativeTime(log.timestamp)} - <strong>{log.actionType}</strong>
                          <span style={{ ...styles.outcomeBadge, backgroundColor: getOutcomeColor(log.outcome) }}>
                            {log.outcome}
                          </span>
                        </span>
                        <span>
                          Agent: {log.agentId || 'N/A'} | User: {log.userId || 'N/A'}
                        </span>
                      </div>
                      {expandedLogId === log.auditId && (
                        <div id={`log-details-${log.auditId}`} style={styles.logDetails}>
                          <h4>Event Data:</h4>
                          <pre>{JSON.stringify(log.eventData, null, 2)}</pre>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </div>
          ))
        ) : (
          <p style={{ textAlign: 'center', color: currentTheme.name === 'Dark' ? '#bbb' : '#666' }}>No audit events found matching your criteria.</p>
        )}
      </div>

      <div style={styles.pagination}>
        <button
          style={styles.button}
          onClick={() => setCurrentPage(prev => Math.max(1, prev - 1))}
          disabled={currentPage === 1}
          aria-label="Previous page"
        >
          Previous
        </button>
        <span aria-live="polite">Page {currentPage} of {totalPages}</span>
        <button
          style={styles.button}
          onClick={() => setCurrentPage(prev => Math.min(totalPages, prev + 1))}
          disabled={currentPage === totalPages}
          aria-label="Next page"
        >
          Next
        </button>
        <select
          name="pageSize"
          value={pageSize}
          onChange={(e) => {
            setPageSize(Number(e.target.value));
            setCurrentPage(1); // Reset to first page when changing page size
          }}
          style={styles.select}
          aria-label="Items per page"
        >
          <option value={5}>5 per group</option>
          <option value={10}>10 per group</option>
          <option value={20}>20 per group</option>
        </select>
      </div>
    </div>
  );
};

export default AgentActionAuditTrail;
