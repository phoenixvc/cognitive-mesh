import React, { useState, useEffect, CSSProperties } from 'react';

// --- Mock Adapter Interfaces and Models (as defined in IAgencyWidgetAdapters.cs and AgencyWidgetModels.cs) ---
// In a real application, these would be imported from a shared library and injected.

interface ErrorEnvelope {
  errorCode: string;
  message: string;
  correlationId?: string;
  details?: any;
  canRetry?: boolean;
}

enum ConsentType {
  ValueDiagnosticDataCollection = 'ValueDiagnosticDataCollection',
  EmployabilityAnalysis = 'EmployabilityAnalysis',
}

interface EmployabilityScoreViewModel {
  userId: string;
  riskScore: number;
  riskLevel: string; // Low | Medium | High
  riskFactors: string[];
  recommendedActions: string[];
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
  EmployabilityViewed = 'EmployabilityViewed',
  EmployabilityReportGenerated = 'EmployabilityReportGenerated',
  ConsentRequested = 'ConsentRequested',
  ConsentGranted = 'ConsentGranted',
  ConsentDenied = 'ConsentDenied',
  ApiCallFailed = 'ApiCallFailed',
  OptOut = 'OptOut',
}

interface ThemeSettings {
  name: 'Light' | 'Dark';
  highContrastEnabled: boolean;
  languageCode: string;
}

interface IDataAPIAdapterPort {
  getEmployabilityScoreAsync(
    userId: string,
    tenantId: string
  ): Promise<WidgetState<EmployabilityScoreViewModel>>;
}

interface IConsentAdapter {
  hasActiveConsentAsync(userId: string, consentType: ConsentType): Promise<boolean>;
  submitConsentAsync(consentRequest: {
    userId: string;
    tenantId: string;
    consentType: ConsentType;
    isGranted: boolean;
    source: string;
  }): Promise<boolean>;
  revokeConsentAsync(userId: string, tenantId: string, consentType: ConsentType): Promise<boolean>;
}

interface ITelemetryAdapter {
  logEventAsync(event: TelemetryEvent): Promise<void>;
}

interface IThemeAdapter {
  getCurrentThemeAsync(): Promise<ThemeSettings>;
  onThemeChanged: (callback: (settings: ThemeSettings) => void) => void;
}

// --- Mock Implementations (for demonstration purposes) ---
const mockDataAPIAdapter: IDataAPIAdapterPort = {
  getEmployabilityScoreAsync: async (userId, tenantId) => {
    console.log(`Mock API: Fetching employability score for ${userId}`);
    return new Promise((resolve) => {
      setTimeout(() => {
        if (Math.random() > 0.1) { // Simulate 10% failure rate
          const riskScore = Math.round(Math.random() * 100) / 100; // 0.0 to 1.0
          let riskLevel = 'Low';
          if (riskScore > 0.6) riskLevel = 'High';
          else if (riskScore > 0.3) riskLevel = 'Medium';

          resolve({
            data: {
              userId,
              riskScore,
              riskLevel,
              riskFactors: [
                'Skills mismatch with market trends',
                'Low recent creative output',
                'Limited cross-functional collaboration',
              ],
              recommendedActions: [
                'Explore training for "AI Prompt Engineering"',
                'Engage in more cross-functional projects',
                'Participate in innovation workshops',
              ],
            },
            isStale: false,
            lastSyncTimestamp: new Date(),
            lastError: null,
          });
        } else {
          resolve({
            data: null,
            isStale: true,
            lastSyncTimestamp: new Date(),
            lastError: {
              errorCode: 'API_ERROR',
              message: 'Failed to fetch employability data from backend.',
              canRetry: true,
            },
          });
        }
      }, 1800);
    });
  },
};

const mockConsentAdapter: IConsentAdapter = {
  hasActiveConsentAsync: async (userId, consentType) => {
    console.log(`Mock Consent: Checking consent for ${userId}, type ${consentType}`);
    const hasConsent = sessionStorage.getItem(`consent_${userId}_${consentType}`) === 'granted';
    return new Promise((resolve) => setTimeout(() => resolve(hasConsent), 300));
  },
  submitConsentAsync: async (request) => {
    console.log(`Mock Consent: Submitting consent for ${request.userId}, type ${request.consentType}, granted: ${request.isGranted}`);
    return new Promise((resolve) => {
      setTimeout(() => {
        if (request.isGranted) {
          sessionStorage.setItem(`consent_${request.userId}_${request.consentType}`, 'granted');
        } else {
          sessionStorage.removeItem(`consent_${request.userId}_${request.consentType}`);
        }
        resolve(true);
      }, 500);
    });
  },
  revokeConsentAsync: async (userId, tenantId, consentType) => {
    console.log(`Mock Consent: Revoking consent for ${userId}, type ${consentType}`);
    return new Promise((resolve) => {
      setTimeout(() => {
        sessionStorage.removeItem(`consent_${userId}_${consentType}`);
        resolve(true);
      }, 500);
    });
  },
};

const mockTelemetryAdapter: ITelemetryAdapter = {
  logEventAsync: async (event) => {
    console.log('Mock Telemetry: Logging event', event);
    return Promise.resolve();
  },
};

const mockThemeAdapter: IThemeAdapter = {
  getCurrentThemeAsync: async () => {
    return { name: 'Light', highContrastEnabled: false, languageCode: 'en-US' };
  },
  onThemeChanged: (callback) => {
    // Simulate a theme change after some time
    setTimeout(() => {
      callback({ name: 'Dark', highContrastEnabled: false, languageCode: 'en-US' });
    }, 5000);
  },
};

// --- Component Styling (simple inline styles for demonstration) ---
const widgetStyles: { [key: string]: CSSProperties } = {
  container: {
    fontFamily: 'Arial, sans-serif',
    padding: '20px',
    borderRadius: '8px',
    boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
    maxWidth: '700px',
    margin: '20px auto',
    backgroundColor: '#fff',
    color: '#333',
  },
  darkContainer: {
    backgroundColor: '#333',
    color: '#eee',
  },
  header: {
    borderBottom: '1px solid #eee',
    paddingBottom: '10px',
    marginBottom: '20px',
    color: '#007bff',
  },
  darkHeader: {
    borderColor: '#555',
    color: '#66b3ff',
  },
  section: {
    marginBottom: '25px',
    padding: '15px',
    borderRadius: '6px',
    backgroundColor: '#f8f9fa',
    border: '1px solid #e9ecef',
  },
  darkSection: {
    backgroundColor: '#444',
    borderColor: '#555',
  },
  loading: {
    textAlign: 'center',
    padding: '20px',
    color: '#007bff',
  },
  error: {
    color: '#dc3545',
    backgroundColor: '#f8d7da',
    border: '1px solid #f5c6cb',
    padding: '10px',
    borderRadius: '4px',
    marginBottom: '15px',
  },
  results: {
    padding: '15px',
    borderRadius: '4px',
    marginTop: '20px',
  },
  list: {
    listStyleType: 'disc',
    marginLeft: '20px',
  },
  consentOverlay: {
    position: 'fixed',
    top: 0,
    left: 0,
    width: '100%',
    height: '100%',
    backgroundColor: 'rgba(0,0,0,0.5)',
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    zIndex: 1000,
  },
  consentBox: {
    backgroundColor: '#fff',
    padding: '30px',
    borderRadius: '8px',
    boxShadow: '0 4px 8px rgba(0,0,0,0.2)',
    maxWidth: '500px',
    textAlign: 'center',
    color: '#333',
  },
  darkConsentBox: {
    backgroundColor: '#222',
    color: '#eee',
  },
  button: {
    padding: '10px 20px',
    borderRadius: '5px',
    border: 'none',
    backgroundColor: '#007bff',
    color: 'white',
    cursor: 'pointer',
    fontSize: '16px',
    marginTop: '10px',
    marginRight: '10px',
  },
  buttonSecondary: {
    backgroundColor: '#6c757d',
  },
  privacyNotice: {
    backgroundColor: '#f0f8ff',
    border: '1px solid #b0e0e6',
    padding: '15px',
    borderRadius: '5px',
    marginBottom: '20px',
    fontSize: '0.9em',
    color: '#333',
  },
  darkPrivacyNotice: {
    backgroundColor: '#2a3b4c',
    borderColor: '#4c6a8a',
    color: '#c0d0e0',
  },
  optOutButton: {
    backgroundColor: '#dc3545',
    color: 'white',
    padding: '8px 15px',
    borderRadius: '5px',
    border: 'none',
    cursor: 'pointer',
    fontSize: '0.9em',
    marginTop: '15px',
  },
};

// --- Component Definition ---

interface EmployabilityScoreWidgetProps {
  userId: string;
  tenantId: string;
  dataAPIAdapter?: IDataAPIAdapterPort;
  consentAdapter?: IConsentAdapter;
  telemetryAdapter?: ITelemetryAdapter;
  themeAdapter?: IThemeAdapter;
}

const EmployabilityScoreWidget: React.FC<EmployabilityScoreWidgetProps> = ({
  userId,
  tenantId,
  dataAPIAdapter = mockDataAPIAdapter,
  consentAdapter = mockConsentAdapter,
  telemetryAdapter = mockTelemetryAdapter,
  themeAdapter = mockThemeAdapter,
}) => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<ErrorEnvelope | null>(null);
  const [showConsent, setShowConsent] = useState(false);
  const [consentGranted, setConsentGranted] = useState(false);
  const [employabilityData, setEmployabilityData] = useState<EmployabilityScoreViewModel | null>(null);
  const [currentTheme, setCurrentTheme] = useState<ThemeSettings | null>(null);

  const widgetId = 'employability-score-widget';
  const panelId = 'EmployabilityScorePanel';

  useEffect(() => {
    themeAdapter.getCurrentThemeAsync().then(setCurrentTheme);
    themeAdapter.onThemeChanged(setCurrentTheme);

    telemetryAdapter.logEventAsync({
      timestamp: new Date(),
      widgetId,
      panelId,
      userId,
      action: TelemetryEventTypes.EmployabilityViewed,
      metadata: { tenantId },
    });

    fetchEmployabilityData();
  }, [userId, tenantId, telemetryAdapter, themeAdapter]);

  const fetchEmployabilityData = async () => {
    setLoading(true);
    setError(null);

    try {
      const hasConsent = await consentAdapter.hasActiveConsentAsync(
        userId,
        ConsentType.EmployabilityAnalysis
      );
      setConsentGranted(hasConsent);

      if (!hasConsent) {
        setShowConsent(true);
        setLoading(false);
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          action: TelemetryEventTypes.ConsentRequested,
          metadata: { consentType: ConsentType.EmployabilityAnalysis },
        });
        return;
      }

      const employabilityState = await dataAPIAdapter.getEmployabilityScoreAsync(
        userId,
        tenantId
      );

      if (employabilityState.data) {
        setEmployabilityData(employabilityState.data);
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          action: TelemetryEventTypes.EmployabilityReportGenerated,
          metadata: { riskScore: employabilityState.data.riskScore, riskLevel: employabilityState.data.riskLevel },
        });
      } else if (employabilityState.lastError) {
        setError(employabilityState.lastError);
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          action: TelemetryEventTypes.ApiCallFailed,
          errorCode: employabilityState.lastError.errorCode,
          metadata: { error: employabilityState.lastError.message, api: 'getEmployabilityScore' },
        });
      }
    } catch (err) {
      console.error('Error fetching employability data:', err);
      setError({
        errorCode: 'FETCH_ERROR',
        message: 'Failed to load employability data. Please try again.',
        canRetry: true,
      });
      telemetryAdapter.logEventAsync({
        timestamp: new Date(),
        widgetId,
        panelId,
        userId,
        action: TelemetryEventTypes.ApiCallFailed,
        errorCode: 'FETCH_ERROR',
        metadata: { error: (err as Error).message },
      });
    } finally {
      setLoading(false);
    }
  };

  const handleConsentDecision = async (granted: boolean) => {
    setLoading(true);
    try {
      const success = await consentAdapter.submitConsentAsync({
        userId,
        tenantId,
        consentType: ConsentType.EmployabilityAnalysis,
        isGranted: granted,
        source: widgetId,
      });
      if (success) {
        setConsentGranted(granted);
        setShowConsent(false);
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          action: granted ? TelemetryEventTypes.ConsentGranted : TelemetryEventTypes.ConsentDenied,
          metadata: { consentType: ConsentType.EmployabilityAnalysis },
        });
        if (granted) {
          fetchEmployabilityData(); // Re-fetch data if consent granted
        }
      } else {
        setError({
          errorCode: 'CONSENT_SUBMISSION_FAILED',
          message: 'Failed to record consent decision. Please try again.',
          canRetry: true,
        });
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          action: TelemetryEventTypes.ApiCallFailed,
          errorCode: 'CONSENT_SUBMISSION_FAILED',
          metadata: { error: 'Consent adapter failed to submit' },
        });
      }
    } catch (err) {
      console.error('Error submitting consent:', err);
      setError({
        errorCode: 'CONSENT_NETWORK_ERROR',
        message: 'A network error occurred while submitting consent. Please try again.',
        canRetry: true,
      });
      telemetryAdapter.logEventAsync({
        timestamp: new Date(),
        widgetId,
        panelId,
        userId,
        action: TelemetryEventTypes.ApiCallFailed,
        errorCode: 'CONSENT_NETWORK_ERROR',
        metadata: { error: (err as Error).message },
      });
    } finally {
      setLoading(false);
    }
  };

  const handleOptOut = async () => {
    setLoading(true);
    try {
      const success = await consentAdapter.revokeConsentAsync(
        userId,
        tenantId,
        ConsentType.EmployabilityAnalysis
      );
      if (success) {
        setConsentGranted(false);
        setEmployabilityData(null);
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          action: TelemetryEventTypes.OptOut,
          metadata: { consentType: ConsentType.EmployabilityAnalysis },
        });
        alert('You have successfully opted out of employability analysis. Your data will no longer be processed for this purpose.');
      } else {
        setError({
          errorCode: 'OPT_OUT_FAILED',
          message: 'Failed to process opt-out request. Please try again.',
          canRetry: true,
        });
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          action: TelemetryEventTypes.ApiCallFailed,
          errorCode: 'OPT_OUT_FAILED',
          metadata: { error: 'Consent adapter failed to revoke' },
        });
      }
    } catch (err) {
      console.error('Error opting out:', err);
      setError({
        errorCode: 'OPT_OUT_NETWORK_ERROR',
        message: 'A network error occurred during opt-out. Please try again.',
        canRetry: true,
      });
      telemetryAdapter.logEventAsync({
        timestamp: new Date(),
        widgetId,
        panelId,
        userId,
        action: TelemetryEventTypes.ApiCallFailed,
        errorCode: 'OPT_OUT_NETWORK_ERROR',
        metadata: { error: (err as Error).message },
      });
    } finally {
      setLoading(false);
    }
  };

  const handleRetry = () => {
    setError(null);
    fetchEmployabilityData();
  };

  const containerStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.container, ...widgetStyles.darkContainer } : widgetStyles.container;
  const headerStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.header, ...widgetStyles.darkHeader } : widgetStyles.header;
  const sectionStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.section, ...widgetStyles.darkSection } : widgetStyles.section;
  const privacyNoticeStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.privacyNotice, ...widgetStyles.darkPrivacyNotice } : widgetStyles.privacyNotice;
  const consentBoxStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.consentBox, ...widgetStyles.darkConsentBox } : widgetStyles.consentBox;

  return (
    <div style={containerStyle} role="region" aria-labelledby="employability-title">
      <h2 id="employability-title" style={headerStyle}>Employability Score & Development</h2>
      <p>Understand your current employability risk and get personalized development recommendations.</p>

      {loading && <p style={widgetStyles.loading} aria-live="polite">Loading employability data...</p>}

      {error && (
        <div style={widgetStyles.error} role="alert">
          <p>Error: {error.message}</p>
          {error.canRetry && (
            <button style={widgetStyles.button} onClick={handleRetry}>Retry</button>
          )}
        </div>
      )}

      {!consentGranted && !loading && !error && showConsent && (
        <div style={widgetStyles.consentOverlay} role="dialog" aria-modal="true" aria-labelledby="consent-title">
          <div style={consentBoxStyle}>
            <h3 id="consent-title">Consent Required for Employability Analysis</h3>
            <p>
              To provide your employability score and personalized recommendations, we need your explicit consent to collect and process sensitive HR-related data, including your skills, project history, and creative output.
              This data is used to assess your market relevance and identify potential development areas.
            </p>
            <p style={{ fontWeight: 'bold', color: '#dc3545' }}>
              Your data will be handled with the utmost care and in strict compliance with GDPR and internal privacy policies. No automated employment decisions will be made based on this analysis.
            </p>
            <button style={widgetStyles.button} onClick={() => handleConsentDecision(true)} disabled={loading}>
              Grant Consent
            </button>
            <button style={{ ...widgetStyles.button, ...widgetStyles.buttonSecondary }} onClick={() => handleConsentDecision(false)} disabled={loading}>
              Deny
            </button>
          </div>
        </div>
      )}

      {consentGranted && !loading && !error && (
        <>
          <div style={privacyNoticeStyle} role="note" aria-label="Privacy Notice">
            <p><strong>Privacy Notice:</strong> This Employability Score is generated using advanced AI models based on your professional data within the organization. It is intended for your personal development and career guidance only. No automated employment decisions are made based on this score. Your data is anonymized where possible and secured with enterprise-grade encryption. You can opt out at any time below.</p>
          </div>

          {employabilityData ? (
            <div style={sectionStyle} aria-labelledby="employability-summary-header">
              <h3 id="employability-summary-header">Your Employability Score</h3>
              <p><strong>Risk Score:</strong> {employabilityData.riskScore.toFixed(2)}</p>
              <p><strong>Risk Level:</strong> <span style={{ fontWeight: 'bold', color: employabilityData.riskLevel === 'High' ? '#dc3545' : employabilityData.riskLevel === 'Medium' ? '#ffc107' : '#28a745' }}>{employabilityData.riskLevel}</span></p>

              <h4>Key Risk Factors:</h4>
              {employabilityData.riskFactors && employabilityData.riskFactors.length > 0 ? (
                <ul style={widgetStyles.list}>
                  {employabilityData.riskFactors.map((factor, i) => (
                    <li key={i}>{factor}</li>
                  ))}
                </ul>
              ) : (
                <p>No significant risk factors identified at this time. Keep up the great work!</p>
              )}

              <h4>Recommended Actions for Development:</h4>
              {employabilityData.recommendedActions && employabilityData.recommendedActions.length > 0 ? (
                <ul style={widgetStyles.list}>
                  {employabilityData.recommendedActions.map((action, i) => (
                    <li key={i}>{action}</li>
                  ))}
                </ul>
              ) : (
                <p>No specific actions recommended at this time. Continue to seek new learning opportunities.</p>
              )}
            </div>
          ) : (
            <p style={{ textAlign: 'center', color: '#6c757d' }}>No employability data available. Please grant consent to view your score.</p>
          )}

          <div style={sectionStyle} aria-labelledby="opt-out-header">
            <h3 id="opt-out-header">Manage Your Privacy</h3>
            <p>If you no longer wish to have your employability data processed for this analysis, you can opt out below. This will remove your data from future analyses and clear your score from this widget.</p>
            <button style={widgetStyles.optOutButton} onClick={handleOptOut} disabled={loading}>
              Opt Out of Employability Analysis
            </button>
          </div>
        </>
      )}
    </div>
  );
};

export default EmployabilityScoreWidget;
