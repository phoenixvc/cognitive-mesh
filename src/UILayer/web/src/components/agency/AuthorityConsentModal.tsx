import React, { useState, useEffect, useCallback, CSSProperties } from 'react';

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

interface ConsentRequest {
  userId: string;
  tenantId: string;
  consentType: string;
  scope?: string;
  isGranted: boolean;
  source: string;
  evidence?: string;
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
  ConsentModalDisplayed = 'ConsentModalDisplayed',
  ConsentDecisionMade = 'ConsentDecisionMade',
  ConsentSubmissionFailed = 'ConsentSubmissionFailed',
  ApiCallFailed = 'ApiCallFailed',
}

interface ThemeSettings {
  name: 'Light' | 'Dark';
  highContrastEnabled: boolean;
  languageCode: string;
}

// --- Risk Level Types ---
type RiskLevel = 'Low' | 'Medium' | 'High' | 'Critical';

// --- Adapter Port Interfaces ---
interface IConsentAdapter {
  recordAgentConsentAsync(request: ConsentRequest, agentId: string, agentAction: string): Promise<boolean>;
}

interface ITelemetryAdapter {
  logEventAsync(event: TelemetryEvent): Promise<void>;
}

interface IThemeAdapter {
  getCurrentThemeAsync(): Promise<ThemeSettings>;
  onThemeChanged(callback: (settings: ThemeSettings) => void): void;
}

// --- Mock Implementations ---
const mockConsentAdapter: IConsentAdapter = {
  recordAgentConsentAsync: async (request, agentId, agentAction) => {
    console.log(`Mock Consent: Recording agent consent for ${agentId}, action ${agentAction}, granted: ${request.isGranted}`);
    return new Promise(resolve => setTimeout(() => {
      if (Math.random() > 0.1) { // Simulate 10% failure rate
        resolve(true);
      } else {
        resolve(false);
      }
    }, 500));
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

// --- Component Props ---
interface AuthorityConsentModalProps {
  isOpen: boolean;
  onClose: () => void;
  agent: AgentViewModel;
  actionDetails: {
    type: string; // e.g., "FinancialTransaction", "SensitiveDataRead"
    description: string;
    riskLevel: RiskLevel;
    consentType: string;
    correlationId: string;
    parameters?: { [key: string]: any };
  };
  userId: string;
  tenantId: string;
  consentAdapter?: IConsentAdapter;
  telemetryAdapter?: ITelemetryAdapter;
  themeAdapter?: IThemeAdapter;
  timeoutMs?: number; // Optional timeout in milliseconds
}

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
      zIndex: 2000,
    },
    modal: {
      backgroundColor: isDark ? '#333' : 'white',
      padding: '30px',
      borderRadius: '8px',
      boxShadow: '0 5px 15px rgba(0,0,0,0.3)',
      width: '90%',
      maxWidth: '600px',
      color: isDark ? '#eee' : '#333',
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
    section: {
      marginBottom: '15px',
    },
    detailGrid: {
      display: 'grid',
      gridTemplateColumns: '120px 1fr',
      gap: '8px',
      marginBottom: '10px',
    },
    detailLabel: {
      fontWeight: 'bold' as 'bold',
      color: isDark ? '#bbb' : '#555',
    },
    riskBadge: {
      padding: '4px 8px',
      borderRadius: '12px',
      fontSize: '12px',
      fontWeight: 'bold' as 'bold',
      color: 'white',
      display: 'inline-block',
      marginLeft: '10px',
    },
    buttonGroup: {
      marginTop: '25px',
      display: 'flex',
      justifyContent: 'flex-end',
      gap: '10px',
    },
    button: {
      padding: '10px 20px',
      borderRadius: '5px',
      border: 'none',
      cursor: 'pointer',
      fontSize: '16px',
      fontWeight: 'bold' as 'bold',
    },
    approveButton: {
      backgroundColor: '#28a745',
      color: 'white',
    },
    denyButton: {
      backgroundColor: '#dc3545',
      color: 'white',
    },
    cancelButton: {
      backgroundColor: isDark ? '#555' : '#6c757d',
      color: 'white',
    },
    loading: {
      textAlign: 'center' as 'center',
      padding: '20px',
      color: isDark ? '#87ceeb' : '#007bff',
    },
    error: {
      color: '#d9534f',
      backgroundColor: '#f2dede',
      border: '1px solid #ebccd1',
      padding: '15px',
      borderRadius: '4px',
      marginBottom: '15px',
    },
    darkError: {
      backgroundColor: '#5c2c2c',
      borderColor: '#8a4c4c',
      color: '#f0f0f0',
    },
    timeoutWarning: {
      textAlign: 'center' as 'center',
      color: '#fd7e14',
      marginTop: '10px',
    },
    riskSummary: {
      backgroundColor: isDark ? '#444' : '#f8f9fa',
      padding: '15px',
      borderRadius: '5px',
      marginTop: '15px',
      borderLeft: `4px solid ${isDark ? '#777' : '#ddd'}`,
    },
    parameterValue: {
      fontFamily: 'monospace',
      backgroundColor: isDark ? '#444' : '#f5f5f5',
      padding: '2px 4px',
      borderRadius: '3px',
      fontSize: '14px',
    },
  };
};

// --- Risk Level Utilities ---
const getRiskColor = (riskLevel: RiskLevel): string => {
  switch (riskLevel) {
    case 'Low': return '#28a745'; // Green
    case 'Medium': return '#ffc107'; // Yellow
    case 'High': return '#fd7e14'; // Orange
    case 'Critical': return '#dc3545'; // Red
    default: return '#6c757d'; // Gray
  }
};

const getRiskDescription = (riskLevel: RiskLevel): string => {
  switch (riskLevel) {
    case 'Low': 
      return 'This action has minimal impact and can be easily reversed if needed.';
    case 'Medium': 
      return 'This action has moderate impact and should be reviewed before approval.';
    case 'High': 
      return 'This action has significant impact and requires careful consideration before approval.';
    case 'Critical': 
      return 'This action has critical impact and could result in major consequences if misused. Please review thoroughly.';
    default: 
      return 'Risk level unknown.';
  }
};

// --- Main Component ---
const AuthorityConsentModal: React.FC<AuthorityConsentModalProps> = ({
  isOpen,
  onClose,
  agent,
  actionDetails,
  userId,
  tenantId,
  consentAdapter = mockConsentAdapter,
  telemetryAdapter = mockTelemetryAdapter,
  themeAdapter = mockThemeAdapter,
  timeoutMs = 60000, // Default timeout: 1 minute
}) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<ErrorEnvelope | null>(null);
  const [currentTheme, setCurrentTheme] = useState<ThemeSettings | null>(null);
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [timeRemaining, setTimeRemaining] = useState(timeoutMs / 1000);
  
  const widgetId = 'authority-consent-modal';
  const panelId = 'AuthorityConsentModalPanel';
  
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
  
  // Initialize theme
  useEffect(() => {
    if (isOpen) {
      themeAdapter.getCurrentThemeAsync().then(setCurrentTheme);
      themeAdapter.onThemeChanged(setCurrentTheme);
      
      // Log modal display
      telemetryAdapter.logEventAsync({
        timestamp: new Date(),
        widgetId,
        panelId,
        userId,
        correlationId: actionDetails.correlationId,
        action: TelemetryEventTypes.ConsentModalDisplayed,
        metadata: { 
          agentId: agent.agentId, 
          actionType: actionDetails.type, 
          riskLevel: actionDetails.riskLevel,
          consentType: actionDetails.consentType
        }
      });
    }
  }, [isOpen, themeAdapter, telemetryAdapter, userId, agent, actionDetails]);
  
  // Handle timeout countdown
  useEffect(() => {
    if (!isOpen) return;
    
    let timer: NodeJS.Timeout;
    
    if (timeRemaining > 0) {
      timer = setInterval(() => {
        setTimeRemaining(prev => {
          if (prev <= 1) {
            clearInterval(timer);
            // Auto-deny on timeout
            handleDecision(false);
            return 0;
          }
          return prev - 1;
        });
      }, 1000);
    }
    
    return () => {
      if (timer) clearInterval(timer);
    };
  }, [isOpen, timeRemaining]);
  
  // Reset state when modal opens
  useEffect(() => {
    if (isOpen) {
      setLoading(false);
      setError(null);
      setShowConfirmation(false);
      setTimeRemaining(timeoutMs / 1000);
    }
  }, [isOpen, timeoutMs]);
  
  // Handle decision (approve/deny)
  const handleDecision = async (granted: boolean) => {
    setLoading(true);
    setError(null);
    
    try {
      const success = await consentAdapter.recordAgentConsentAsync({
        userId,
        tenantId,
        consentType: actionDetails.consentType,
        isGranted: granted,
        source: widgetId,
        evidence: `Decision made via AuthorityConsentModal for action: ${actionDetails.type}`
      }, agent.agentId, actionDetails.type);
      
      if (success) {
        // Log successful consent decision
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          correlationId: actionDetails.correlationId,
          action: TelemetryEventTypes.ConsentDecisionMade,
          metadata: { 
            agentId: agent.agentId, 
            actionType: actionDetails.type, 
            consentType: actionDetails.consentType,
            decision: granted ? 'approved' : 'denied'
          }
        });
        
        onClose();
      } else {
        // Handle consent recording failure
        setError({
          errorCode: 'CONSENT_RECORD_FAILED',
          message: 'Failed to record your decision. Please try again.',
          canRetry: true
        });
        
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          correlationId: actionDetails.correlationId,
          action: TelemetryEventTypes.ConsentSubmissionFailed,
          errorCode: 'CONSENT_RECORD_FAILED',
          metadata: { 
            agentId: agent.agentId, 
            actionType: actionDetails.type,
            consentType: actionDetails.consentType,
            decision: granted ? 'approved' : 'denied'
          }
        });
      }
    } catch (err) {
      // Handle unexpected errors
      console.error('Error recording consent:', err);
      
      setError({
        errorCode: 'UNEXPECTED_ERROR',
        message: 'An unexpected error occurred while recording your decision. Please try again.',
        canRetry: true
      });
      
      telemetryAdapter.logEventAsync({
        timestamp: new Date(),
        widgetId,
        panelId,
        userId,
        correlationId: actionDetails.correlationId,
        action: TelemetryEventTypes.ApiCallFailed,
        errorCode: 'UNEXPECTED_ERROR',
        metadata: { 
          agentId: agent.agentId, 
          actionType: actionDetails.type, 
          error: err instanceof Error ? err.message : 'Unknown error'
        }
      });
    } finally {
      setLoading(false);
      setShowConfirmation(false);
    }
  };
  
  // Handle approve click
  const handleApproveClick = () => {
    // For high-risk or critical actions, show confirmation dialog
    if (actionDetails.riskLevel === 'High' || actionDetails.riskLevel === 'Critical') {
      setShowConfirmation(true);
    } else {
      handleDecision(true);
    }
  };
  
  // Handle deny click
  const handleDenyClick = () => {
    handleDecision(false);
  };
  
  // Handle confirmation approve
  const handleConfirmationApprove = () => {
    handleDecision(true);
  };
  
  // Handle confirmation cancel
  const handleConfirmationCancel = () => {
    setShowConfirmation(false);
  };
  
  // If modal is not open, don't render anything
  if (!isOpen) return null;
  
  // Get styles based on current theme
  const styles = getStyles(currentTheme || { name: 'Light', highContrastEnabled: false, languageCode: 'en-US' });
  
  // Get risk color for the badge
  const riskColor = getRiskColor(actionDetails.riskLevel);
  
  return (
    <div 
      style={styles.overlay} 
      role="dialog" 
      aria-modal="true" 
      aria-labelledby="authority-consent-title"
      onClick={(e) => {
        // Close modal when clicking outside (but not when clicking inside)
        if (e.target === e.currentTarget) {
          onClose();
        }
      }}
    >
      <div 
        style={styles.modal}
        onClick={(e) => e.stopPropagation()}
        tabIndex={-1}
      >
        <button 
          style={styles.closeButton} 
          onClick={onClose}
          aria-label="Close modal"
          tabIndex={0}
        >
          &times;
        </button>
        
        <h2 id="authority-consent-title" style={styles.header}>
          Agent Action Requires Your Approval
        </h2>
        
        {loading ? (
          <p style={styles.loading} aria-live="polite">Processing your decision...</p>
        ) : error ? (
          <div 
            style={currentTheme?.name === 'Dark' ? {...styles.error, ...styles.darkError} : styles.error}
            role="alert"
          >
            <p>Error: {error.message}</p>
            {error.canRetry && (
              <button 
                style={{...styles.button, ...styles.cancelButton}} 
                onClick={() => handleDecision(showConfirmation)}
                tabIndex={0}
              >
                Retry
              </button>
            )}
          </div>
        ) : showConfirmation ? (
          <div style={styles.section}>
            <h3 style={{color: riskColor}}>Confirm {actionDetails.riskLevel} Risk Action</h3>
            <p>
              You are about to approve a <strong>{actionDetails.riskLevel}</strong> risk action.
              Please ensure you understand the implications. This decision will be logged with
              correlation ID <code>{actionDetails.correlationId}</code> for audit purposes.
            </p>
            <div style={styles.buttonGroup}>
              <button 
                style={{...styles.button, ...styles.cancelButton}}
                onClick={handleConfirmationCancel}
                aria-label="Cancel approval"
                tabIndex={0}
              >
                Cancel
              </button>
              <button 
                style={{...styles.button, ...styles.approveButton}}
                onClick={handleConfirmationApprove}
                aria-label={`Confirm and approve ${actionDetails.riskLevel.toLowerCase()} risk action`}
                tabIndex={0}
              >
                Confirm Approval
              </button>
            </div>
          </div>
        ) : (
          <>
            <div style={styles.section}>
              <h3>Action Details</h3>
              <div style={styles.detailGrid}>
                <span style={styles.detailLabel}>Agent:</span>
                <span>{agent.agentType} (v{agent.version})</span>
                
                <span style={styles.detailLabel}>Action:</span>
                <span>{actionDetails.type}</span>
                
                <span style={styles.detailLabel}>Description:</span>
                <span>{actionDetails.description}</span>
                
                <span style={styles.detailLabel}>Risk Level:</span>
                <span>
                  {actionDetails.riskLevel}
                  <span 
                    style={{...styles.riskBadge, backgroundColor: riskColor}}
                    aria-hidden="true"
                  >
                    {actionDetails.riskLevel}
                  </span>
                </span>
                
                <span style={styles.detailLabel}>Correlation ID:</span>
                <span>{actionDetails.correlationId}</span>
              </div>
              
              {actionDetails.parameters && Object.keys(actionDetails.parameters).length > 0 && (
                <>
                  <h4>Action Parameters:</h4>
                  <div style={styles.detailGrid}>
                    {Object.entries(actionDetails.parameters).map(([key, value]) => (
                      <React.Fragment key={key}>
                        <span style={styles.detailLabel}>{key}:</span>
                        <span style={styles.parameterValue}>
                          {typeof value === 'object' ? JSON.stringify(value) : String(value)}
                        </span>
                      </React.Fragment>
                    ))}
                  </div>
                </>
              )}
              
              <div style={styles.riskSummary}>
                <h4 style={{color: riskColor, margin: '0 0 10px 0'}}>Risk Summary</h4>
                <p>{getRiskDescription(actionDetails.riskLevel)}</p>
              </div>
            </div>
            
            <div style={styles.buttonGroup}>
              <button 
                style={{...styles.button, ...styles.denyButton}}
                onClick={handleDenyClick}
                aria-label="Deny agent action"
                tabIndex={0}
              >
                Deny
              </button>
              <button 
                style={{...styles.button, ...styles.approveButton}}
                onClick={handleApproveClick}
                aria-label="Approve agent action"
                tabIndex={0}
              >
                Approve
              </button>
            </div>
            
            {timeRemaining < 30 && (
              <p style={styles.timeoutWarning} aria-live="polite">
                Decision required: {timeRemaining} seconds remaining
              </p>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default AuthorityConsentModal;
