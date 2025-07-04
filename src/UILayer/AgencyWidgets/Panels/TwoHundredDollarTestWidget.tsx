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

interface ValueDiagnosticViewModel {
  targetId: string;
  targetType: 'User' | 'Team';
  valueScore: number;
  valueProfile: string;
  strengths: string[];
  developmentOpportunities: string[];
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
  ValueDiagnosticViewed = 'ValueDiagnosticViewed',
  ValueDiagnosticSubmitted = 'ValueDiagnosticSubmitted',
  ConsentRequested = 'ConsentRequested',
  ConsentGranted = 'ConsentGranted',
  ConsentDenied = 'ConsentDenied',
  ApiCallFailed = 'ApiCallFailed',
}

interface ThemeSettings {
  name: 'Light' | 'Dark';
  highContrastEnabled: boolean;
  languageCode: string;
}

interface IDataAPIAdapterPort {
  submitTwoHundredDollarTestAsync(
    userId: string,
    responses: { [key: string]: any },
    tenantId: string
  ): Promise<boolean>;
  getValueDiagnosticDataAsync(
    targetId: string,
    targetType: 'User' | 'Team',
    tenantId: string
  ): Promise<WidgetState<ValueDiagnosticViewModel>>;
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
}

interface ITelemetryAdapter {
  logEventAsync(event: TelemetryEvent): Promise<void>;
}

interface IThemeAdapter {
  getCurrentThemeAsync(): Promise<ThemeSettings>;
  onThemeChanged: (callback: (settings: ThemeSettings) => void) => void;
}

// --- Mock Implementations (for demonstration purposes) ---
// In a real application, these would be concrete implementations provided via DI.

const mockDataAPIAdapter: IDataAPIAdapterPort = {
  submitTwoHundredDollarTestAsync: async (userId, responses, tenantId) => {
    console.log(`Mock API: Submitting $200 test for ${userId} in ${tenantId}`, responses);
    return new Promise((resolve) => {
      setTimeout(() => {
        if (Math.random() > 0.1) { // Simulate 10% failure rate
          resolve(true);
        } else {
          resolve(false);
        }
      }, 1000);
    });
  },
  getValueDiagnosticDataAsync: async (targetId, targetType, tenantId) => {
    console.log(`Mock API: Fetching diagnostic data for ${targetId}`);
    return new Promise((resolve) => {
      setTimeout(() => {
        if (Math.random() > 0.1) { // Simulate 10% failure rate
          resolve({
            data: {
              targetId,
              targetType,
              valueScore: Math.floor(Math.random() * 200) + 50, // Score between 50 and 250
              valueProfile: 'Innovator',
              strengths: ['Problem Solving', 'Collaboration'],
              developmentOpportunities: ['Time Management', 'Networking'],
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
              message: 'Failed to fetch data from backend.',
              canRetry: true,
            },
          });
        }
      }, 1500);
    });
  },
};

const mockConsentAdapter: IConsentAdapter = {
  hasActiveConsentAsync: async (userId, consentType) => {
    console.log(`Mock Consent: Checking consent for ${userId}, type ${consentType}`);
    // Simulate consent being granted after a short delay, or if already in session storage
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
    maxWidth: '600px',
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
  formGroup: {
    marginBottom: '15px',
  },
  label: {
    display: 'block',
    marginBottom: '5px',
    fontWeight: 'bold',
  },
  input: {
    width: '100%',
    padding: '8px',
    borderRadius: '4px',
    border: '1px solid #ddd',
    boxSizing: 'border-box',
    backgroundColor: '#f9f9f9',
    color: '#333',
  },
  darkInput: {
    backgroundColor: '#444',
    borderColor: '#666',
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
  },
  buttonSecondary: {
    backgroundColor: '#6c757d',
    marginLeft: '10px',
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
    backgroundColor: '#e9ecef',
    padding: '15px',
    borderRadius: '4px',
    marginTop: '20px',
    color: '#333',
  },
  darkResults: {
    backgroundColor: '#444',
    color: '#eee',
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
};

// --- Component Definition ---

interface TwoHundredDollarTestWidgetProps {
  userId: string;
  tenantId: string;
  // In a real app, adapters would be passed as props or via a Context API
  dataAPIAdapter?: IDataAPIAdapterPort;
  consentAdapter?: IConsentAdapter;
  telemetryAdapter?: ITelemetryAdapter;
  themeAdapter?: IThemeAdapter;
}

const TwoHundredDollarTestWidget: React.FC<TwoHundredDollarTestWidgetProps> = ({
  userId,
  tenantId,
  dataAPIAdapter = mockDataAPIAdapter,
  consentAdapter = mockConsentAdapter,
  telemetryAdapter = mockTelemetryAdapter,
  themeAdapter = mockThemeAdapter,
}) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<ErrorEnvelope | null>(null);
  const [showConsent, setShowConsent] = useState(false);
  const [consentGranted, setConsentGranted] = useState(false);
  const [results, setResults] = useState<ValueDiagnosticViewModel | null>(null);
  const [formResponses, setFormResponses] = useState({
    q1: '',
    q2: '',
    q3: '',
  });
  const [currentTheme, setCurrentTheme] = useState<ThemeSettings | null>(null);

  const widgetId = 'two-hundred-dollar-test-widget';
  const panelId = 'TwoHundredDollarTestPanel';

  useEffect(() => {
    // Initialize theme
    themeAdapter.getCurrentThemeAsync().then(setCurrentTheme);
    themeAdapter.onThemeChanged(setCurrentTheme);

    // Log widget view event
    telemetryAdapter.logEventAsync({
      timestamp: new Date(),
      widgetId,
      panelId,
      userId,
      action: TelemetryEventTypes.ValueDiagnosticViewed,
      metadata: { tenantId },
    });

    // Check initial consent status
    checkConsentStatus();
  }, [userId, tenantId, telemetryAdapter, themeAdapter]);

  const checkConsentStatus = async () => {
    try {
      const hasConsent = await consentAdapter.hasActiveConsentAsync(
        userId,
        ConsentType.ValueDiagnosticDataCollection
      );
      setConsentGranted(hasConsent);
    } catch (err) {
      console.error('Error checking consent status:', err);
      setError({
        errorCode: 'CONSENT_CHECK_FAILED',
        message: 'Failed to check consent status. Please try again.',
        canRetry: true,
      });
      telemetryAdapter.logEventAsync({
        timestamp: new Date(),
        widgetId,
        panelId,
        userId,
        action: TelemetryEventTypes.ApiCallFailed,
        errorCode: 'CONSENT_CHECK_FAILED',
        metadata: { error: (err as Error).message },
      });
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormResponses((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    if (!consentGranted) {
      setShowConsent(true);
      setLoading(false);
      telemetryAdapter.logEventAsync({
        timestamp: new Date(),
        widgetId,
        panelId,
        userId,
        action: TelemetryEventTypes.ConsentRequested,
        metadata: { consentType: ConsentType.ValueDiagnosticDataCollection },
      });
      return;
    }

    try {
      const success = await dataAPIAdapter.submitTwoHundredDollarTestAsync(
        userId,
        formResponses,
        tenantId
      );

      if (success) {
        // Assuming successful submission means we can now fetch results
        const resultState = await dataAPIAdapter.getValueDiagnosticDataAsync(
          userId,
          'User', // Assuming this widget is for individual users
          tenantId
        );
        if (resultState.data) {
          setResults(resultState.data);
          telemetryAdapter.logEventAsync({
            timestamp: new Date(),
            widgetId,
            panelId,
            userId,
            action: TelemetryEventTypes.ValueDiagnosticSubmitted,
            metadata: { valueScore: resultState.data.valueScore },
          });
        } else if (resultState.lastError) {
          setError(resultState.lastError);
          telemetryAdapter.logEventAsync({
            timestamp: new Date(),
            widgetId,
            panelId,
            userId,
            action: TelemetryEventTypes.ApiCallFailed,
            errorCode: resultState.lastError.errorCode,
            metadata: { error: resultState.lastError.message },
          });
        }
      } else {
        setError({
          errorCode: 'SUBMISSION_FAILED',
          message: 'Failed to submit your test. Please try again.',
          canRetry: true,
        });
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          action: TelemetryEventTypes.ApiCallFailed,
          errorCode: 'SUBMISSION_FAILED',
          metadata: { error: 'Backend submission failed' },
        });
      }
    } catch (err) {
      console.error('Error during test submission:', err);
      setError({
        errorCode: 'NETWORK_ERROR',
        message: 'A network error occurred. Please check your connection and try again.',
        canRetry: true,
      });
      telemetryAdapter.logEventAsync({
        timestamp: new Date(),
        widgetId,
        panelId,
        userId,
        action: TelemetryEventTypes.ApiCallFailed,
        errorCode: 'NETWORK_ERROR',
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
        consentType: ConsentType.ValueDiagnosticDataCollection,
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
          metadata: { consentType: ConsentType.ValueDiagnosticDataCollection },
        });
        if (granted) {
          // If consent granted, re-attempt submission
          handleSubmit(new Event('submit') as unknown as React.FormEvent);
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

  const handleRetry = () => {
    setError(null);
    if (results) {
      // If retrying after fetching results, re-fetch
      setResults(null); // Clear previous results
      handleSubmit(new Event('submit') as unknown as React.FormEvent);
    } else {
      // If retrying after form submission, re-submit
      handleSubmit(new Event('submit') as unknown as React.FormEvent);
    }
  };

  const containerStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.container, ...widgetStyles.darkContainer } : widgetStyles.container;
  const headerStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.header, ...widgetStyles.darkHeader } : widgetStyles.header;
  const inputStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.input, ...widgetStyles.darkInput } : widgetStyles.input;
  const resultsStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.results, ...widgetStyles.darkResults } : widgetStyles.results;
  const consentBoxStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.consentBox, ...widgetStyles.darkConsentBox } : widgetStyles.consentBox;

  return (
    <div style={containerStyle} role="region" aria-labelledby="widget-title">
      <h2 id="widget-title" style={headerStyle}>The $200 Test: Value Diagnostic</h2>
      <p>Answer a few questions to get your personal value generation diagnostic.</p>

      {loading && <p style={widgetStyles.loading} aria-live="polite">Loading...</p>}

      {error && (
        <div style={widgetStyles.error} role="alert">
          <p>Error: {error.message}</p>
          {error.canRetry && (
            <button style={widgetStyles.button} onClick={handleRetry}>Retry</button>
          )}
        </div>
      )}

      {results ? (
        <div style={resultsStyle} aria-live="polite">
          <h3>Your Value Diagnostic Results for {results.targetId}</h3>
          <p><strong>Value Score:</strong> {results.valueScore}</p>
          <p><strong>Value Profile:</strong> {results.valueProfile}</p>
          <h4>Strengths:</h4>
          <ul style={widgetStyles.list}>
            {results.strengths.map((s, i) => (
              <li key={i}>{s}</li>
            ))}
          </ul>
          <h4>Development Opportunities:</h4>
          <ul style={widgetStyles.list}>
            {results.developmentOpportunities.map((d, i) => (
              <li key={i}>{d}</li>
            ))}
          </ul>
          <button style={widgetStyles.button} onClick={() => setResults(null)}>Take Test Again</button>
        </div>
      ) : (
        <form onSubmit={handleSubmit} aria-disabled={loading}>
          <div style={widgetStyles.formGroup}>
            <label htmlFor="q1" style={widgetStyles.label}>
              How often do you contribute new ideas that lead to tangible improvements?
            </label>
            <input
              type="text"
              id="q1"
              name="q1"
              value={formResponses.q1}
              onChange={handleInputChange}
              style={inputStyle}
              aria-required="true"
              disabled={loading}
            />
          </div>
          <div style={widgetStyles.formGroup}>
            <label htmlFor="q2" style={widgetStyles.label}>
              Describe a recent instance where your work directly impacted a key business metric.
            </label>
            <textarea
              id="q2"
              name="q2"
              value={formResponses.q2}
              onChange={handleInputChange}
              rows={4}
              style={inputStyle}
              aria-required="true"
              disabled={loading}
            ></textarea>
          </div>
          <div style={widgetStyles.formGroup}>
            <label htmlFor="q3" style={widgetStyles.label}>
              How effectively do you collaborate across teams to achieve shared goals? (1-5, 5 being highly effective)
            </label>
            <input
              type="number"
              id="q3"
              name="q3"
              value={formResponses.q3}
              onChange={handleInputChange}
              min="1"
              max="5"
              style={inputStyle}
              aria-required="true"
              disabled={loading}
            />
          </div>
          <button type="submit" style={widgetStyles.button} disabled={loading}>
            Get My Diagnostic
          </button>
        </form>
      )}

      {showConsent && (
        <div style={widgetStyles.consentOverlay} role="dialog" aria-modal="true" aria-labelledby="consent-title">
          <div style={consentBoxStyle}>
            <h3 id="consent-title">Consent Required</h3>
            <p>
              To perform the Value Diagnostic, we need your consent to collect and process your data related to your contributions and activities.
              This data will be used solely for generating your diagnostic report and will not be shared externally without further explicit consent.
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
    </div>
  );
};

export default TwoHundredDollarTestWidget;
