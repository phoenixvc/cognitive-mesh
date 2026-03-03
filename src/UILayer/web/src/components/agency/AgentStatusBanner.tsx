import React, { useState, useEffect, useCallback } from 'react';

// --- Interfaces for Adapter Ports ---
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

interface WidgetState<T> {
  data: T | null;
  isStale: boolean;
  lastSyncTimestamp: Date;
  lastError: ErrorEnvelope | null;
}

interface Notification {
  notificationId: string;
  title: string;
  message: string;
  severity: 'Info' | 'Warning' | 'Error' | 'Critical';
  timestamp: Date;
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

interface ThemeSettings {
  name: 'Light' | 'Dark';
  highContrastEnabled: boolean;
  languageCode: string;
}

// --- Agent Status Types ---
type AgentStatus = 
  | 'idle' 
  | 'executing' 
  | 'awaiting_approval' 
  | 'offline' 
  | 'circuit_broken' 
  | 'authority_required'
  | 'consent_required'
  | 'error';

// --- Adapter Port Interfaces ---
interface IDataAPIAdapterPort {
  getAgentStatusAsync(agentId: string, tenantId: string): Promise<WidgetState<AgentStatus>>;
  getAgentDetailsAsync(agentId: string, tenantId: string): Promise<WidgetState<AgentViewModel>>;
}

interface INotificationAdapter {
  onNotificationReceived(callback: (notification: Notification) => void): void;
  startListeningAsync(): Promise<void>;
  stopListeningAsync(): Promise<void>;
}

interface ITelemetryAdapter {
  logEventAsync(event: TelemetryEvent): Promise<void>;
}

interface IThemeAdapter {
  getCurrentThemeAsync(): Promise<ThemeSettings>;
  onThemeChanged(callback: (settings: ThemeSettings) => void): void;
}

// --- Mock Implementations ---
const mockAgentStatus: Record<string, AgentStatus> = {
  'agent-001': 'executing',
  'agent-002': 'awaiting_approval',
  'agent-003': 'idle',
  'agent-004': 'offline',
  'agent-005': 'circuit_broken',
};

const mockDataAPIAdapter: IDataAPIAdapterPort = {
  getAgentStatusAsync: async (agentId, tenantId) => {
    console.log(`Mock API: Fetching status for agent ${agentId}`);
    return new Promise(resolve => setTimeout(() => {
      const status = mockAgentStatus[agentId] || 'idle';
      if (Math.random() > 0.9) { // 10% chance of error for testing
        resolve({
          data: null,
          isStale: false,
          lastSyncTimestamp: new Date(),
          lastError: {
            errorCode: 'API_ERROR',
            message: 'Failed to fetch agent status.',
            canRetry: true
          }
        });
      } else {
        resolve({
          data: status,
          isStale: false,
          lastSyncTimestamp: new Date(),
          lastError: null
        });
      }
    }, 500));
  },
  getAgentDetailsAsync: async (agentId, tenantId) => {
    console.log(`Mock API: Fetching details for agent ${agentId}`);
    return new Promise(resolve => setTimeout(() => {
      resolve({
        data: {
          agentId,
          agentType: `Agent-${agentId.split('-')[1]}`,
          description: 'Mock agent description',
          status: 'Active',
          version: '1.0.0',
          defaultAutonomy: 'ActWithConfirmation'
        },
        isStale: false,
        lastSyncTimestamp: new Date(),
        lastError: null
      });
    }, 300));
  }
};

const mockNotificationAdapter: INotificationAdapter = {
  onNotificationReceived: (callback) => {
    // Simulate notifications every 10 seconds
    setInterval(() => {
      const notifications = [
        { notificationId: '1', title: 'Agent Status Change', message: 'Agent is now executing a task', severity: 'Info', timestamp: new Date() },
        { notificationId: '2', title: 'Approval Required', message: 'Agent requires approval to proceed', severity: 'Warning', timestamp: new Date() },
        { notificationId: '3', title: 'Agent Error', message: 'Agent encountered an error', severity: 'Error', timestamp: new Date() }
      ];
      const randomNotification = notifications[Math.floor(Math.random() * notifications.length)];
      callback(randomNotification as Notification);
    }, 10000);
  },
  startListeningAsync: async () => console.log('Started listening for notifications'),
  stopListeningAsync: async () => console.log('Stopped listening for notifications')
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
    banner: {
      position: 'fixed' as 'fixed',
      top: 0,
      left: 0,
      right: 0,
      padding: '10px 20px',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      zIndex: 1000,
      boxShadow: '0 2px 4px rgba(0,0,0,0.2)',
      transition: 'background-color 0.3s ease',
      color: 'white',
      fontSize: '14px',
      fontFamily: 'Arial, sans-serif',
    },
    statusIndicator: {
      width: '12px',
      height: '12px',
      borderRadius: '50%',
      marginRight: '10px',
      display: 'inline-block'
    },
    statusText: {
      fontWeight: 'bold' as 'bold',
      display: 'flex',
      alignItems: 'center'
    },
    actions: {
      display: 'flex',
      gap: '10px'
    },
    actionButton: {
      padding: '5px 10px',
      borderRadius: '4px',
      border: 'none',
      cursor: 'pointer',
      backgroundColor: 'rgba(255, 255, 255, 0.2)',
      color: 'white',
      fontSize: '12px',
      fontWeight: 'bold' as 'bold',
      transition: 'background-color 0.2s ease',
    },
    notification: {
      flexGrow: 1,
      textAlign: 'center' as 'center',
      margin: '0 20px',
      overflow: 'hidden',
      textOverflow: 'ellipsis',
      whiteSpace: 'nowrap' as 'nowrap',
    }
  };
};

// Status-specific styles
const getStatusStyles = (status: AgentStatus) => {
  switch (status) {
    case 'idle':
      return { backgroundColor: '#28a745', indicatorColor: '#a7e9b8' }; // Green
    case 'executing':
      return { backgroundColor: '#007bff', indicatorColor: '#99caff' }; // Blue
    case 'awaiting_approval':
      return { backgroundColor: '#ffc107', indicatorColor: '#ffe699', color: '#212529' }; // Yellow with dark text
    case 'offline':
      return { backgroundColor: '#6c757d', indicatorColor: '#c8cccf' }; // Gray
    case 'circuit_broken':
      return { backgroundColor: '#dc3545', indicatorColor: '#f5c2c7' }; // Red
    case 'authority_required':
      return { backgroundColor: '#fd7e14', indicatorColor: '#fed7aa' }; // Orange
    case 'consent_required':
      return { backgroundColor: '#fd7e14', indicatorColor: '#fed7aa' }; // Orange
    case 'error':
      return { backgroundColor: '#dc3545', indicatorColor: '#f5c2c7' }; // Red
    default:
      return { backgroundColor: '#6c757d', indicatorColor: '#c8cccf' }; // Gray (default)
  }
};

// Status text mapping
const getStatusText = (status: AgentStatus): string => {
  switch (status) {
    case 'idle': return 'Agent Idle';
    case 'executing': return 'Agent Executing';
    case 'awaiting_approval': return 'Agent Awaiting Approval';
    case 'offline': return 'Agent Offline';
    case 'circuit_broken': return 'Agent Circuit Broken';
    case 'authority_required': return 'Agent Authority Required';
    case 'consent_required': return 'Agent Consent Required';
    case 'error': return 'Agent Error';
    default: return 'Unknown Status';
  }
};

// --- Component Props ---
interface AgentStatusBannerProps {
  agentId: string;
  userId: string;
  tenantId: string;
  dataAPIAdapter?: IDataAPIAdapterPort;
  notificationAdapter?: INotificationAdapter;
  telemetryAdapter?: ITelemetryAdapter;
  themeAdapter?: IThemeAdapter;
  onEscalate?: (agentId: string) => void;
  onRetry?: (agentId: string) => void;
  onOpenControlCenter?: () => void;
}

// --- Main Component ---
const AgentStatusBanner: React.FC<AgentStatusBannerProps> = ({
  agentId,
  userId,
  tenantId,
  dataAPIAdapter = mockDataAPIAdapter,
  notificationAdapter = mockNotificationAdapter,
  telemetryAdapter = mockTelemetryAdapter,
  themeAdapter = mockThemeAdapter,
  onEscalate,
  onRetry,
  onOpenControlCenter
}) => {
  const [status, setStatus] = useState<AgentStatus>('idle');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<ErrorEnvelope | null>(null);
  const [theme, setTheme] = useState<ThemeSettings>({ name: 'Light', highContrastEnabled: false, languageCode: 'en-US' });
  const [notification, setNotification] = useState<Notification | null>(null);
  
  const widgetId = 'agent-status-banner';
  const panelId = 'AgentStatusBannerPanel';

  // Fetch agent status
  const fetchAgentStatus = useCallback(async () => {
    setLoading(true);
    try {
      const result = await dataAPIAdapter.getAgentStatusAsync(agentId, tenantId);
      if (result.data) {
        setStatus(result.data);
        setError(null);
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          action: 'AgentStatusChanged',
          metadata: { agentId, newStatus: result.data }
        });
      } else if (result.lastError) {
        setError(result.lastError);
        setStatus('error');
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          action: 'AgentStatusFetchFailed',
          errorCode: result.lastError.errorCode,
          metadata: { agentId, error: result.lastError.message }
        });
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Unknown error';
      setError({
        errorCode: 'FETCH_ERROR',
        message: 'Failed to fetch agent status',
        canRetry: true
      });
      setStatus('error');
      telemetryAdapter.logEventAsync({
        timestamp: new Date(),
        widgetId,
        panelId,
        userId,
        action: 'AgentStatusFetchFailed',
        errorCode: 'FETCH_ERROR',
        metadata: { agentId, error: errorMessage }
      });
    } finally {
      setLoading(false);
    }
  }, [agentId, tenantId, dataAPIAdapter, telemetryAdapter, userId]);

  // Handle notifications
  const handleNotification = useCallback((notification: Notification) => {
    setNotification(notification);
    
    // Update status based on notification content
    if (notification.title.includes('Status Change')) {
      fetchAgentStatus();
    } else if (notification.title.includes('Approval Required')) {
      setStatus('awaiting_approval');
    } else if (notification.title.includes('Error')) {
      setStatus('error');
    }
    
    // Log notification received
    telemetryAdapter.logEventAsync({
      timestamp: new Date(),
      widgetId,
      panelId,
      userId,
      action: 'NotificationReceived',
      metadata: { 
        notificationId: notification.notificationId,
        title: notification.title,
        severity: notification.severity
      }
    });
    
    // Clear notification after 5 seconds
    setTimeout(() => {
      setNotification(null);
    }, 5000);
  }, [fetchAgentStatus, telemetryAdapter, userId, widgetId, panelId]);

  // Handle action clicks
  const handleAction = (actionType: 'escalate' | 'retry' | 'open_control_center') => {
    telemetryAdapter.logEventAsync({
      timestamp: new Date(),
      widgetId,
      panelId,
      userId,
      action: `AgentAction${actionType.charAt(0).toUpperCase() + actionType.slice(1)}`,
      metadata: { agentId, currentStatus: status }
    });
    
    switch (actionType) {
      case 'escalate':
        onEscalate?.(agentId);
        break;
      case 'retry':
        fetchAgentStatus();
        onRetry?.(agentId);
        break;
      case 'open_control_center':
        onOpenControlCenter?.();
        break;
    }
  };

  // Initialize
  useEffect(() => {
    // Get theme
    themeAdapter.getCurrentThemeAsync().then(setTheme);
    themeAdapter.onThemeChanged(setTheme);
    
    // Start listening for notifications
    notificationAdapter.onNotificationReceived(handleNotification);
    notificationAdapter.startListeningAsync();
    
    // Fetch initial status
    fetchAgentStatus();
    
    // Log banner viewed
    telemetryAdapter.logEventAsync({
      timestamp: new Date(),
      widgetId,
      panelId,
      userId,
      action: 'AgentStatusBannerViewed',
      metadata: { agentId }
    });
    
    // Set up polling for status updates every 30 seconds
    const intervalId = setInterval(fetchAgentStatus, 30000);
    
    // Cleanup
    return () => {
      clearInterval(intervalId);
      notificationAdapter.stopListeningAsync();
    };
  }, [fetchAgentStatus, notificationAdapter, themeAdapter, handleNotification, telemetryAdapter, userId, widgetId, panelId, agentId]);

  // Get styles based on current status and theme
  const styles = getStyles(theme);
  const statusStyles = getStatusStyles(status);
  const statusText = getStatusText(status);
  
  // Combine styles
  const bannerStyle = {
    ...styles.banner,
    backgroundColor: statusStyles.backgroundColor,
    color: statusStyles.color || 'white'
  };
  
  const indicatorStyle = {
    ...styles.statusIndicator,
    backgroundColor: statusStyles.indicatorColor
  };

  return (
    <div 
      style={bannerStyle}
      role="status"
      aria-live="polite"
      aria-atomic="true"
      aria-label={`Agent Status: ${statusText}`}
    >
      <div style={styles.statusText}>
        <span style={indicatorStyle} aria-hidden="true"></span>
        <span>{statusText}</span>
        {loading && <span> (Updating...)</span>}
      </div>
      
      {notification && (
        <div style={styles.notification}>
          <strong>{notification.title}:</strong> {notification.message}
        </div>
      )}
      
      <div style={styles.actions}>
        {error?.canRetry && (
          <button 
            style={styles.actionButton}
            onClick={() => handleAction('retry')}
            aria-label="Retry fetching agent status"
          >
            Retry
          </button>
        )}
        
        {status === 'awaiting_approval' && (
          <button 
            style={styles.actionButton}
            onClick={() => handleAction('escalate')}
            aria-label="Escalate for review"
          >
            Review Approval
          </button>
        )}
        
        {(status === 'error' || status === 'offline' || status === 'circuit_broken') && (
          <button 
            style={styles.actionButton}
            onClick={() => handleAction('escalate')}
            aria-label="Escalate issue"
          >
            Escalate
          </button>
        )}
        
        <button 
          style={styles.actionButton}
          onClick={() => handleAction('open_control_center')}
          aria-label="Open agent control center"
        >
          Control Center
        </button>
      </div>
    </div>
  );
};

export default AgentStatusBanner;
