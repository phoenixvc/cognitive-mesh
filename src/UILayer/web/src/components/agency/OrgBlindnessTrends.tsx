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

interface OrgBlindnessTrendViewModel {
  blindnessRiskScore: number;
  topBlindSpots: string[];
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
  OrgBlindnessViewed = 'OrgBlindnessViewed',
  OrgBlindnessReportGenerated = 'OrgBlindnessReportGenerated',
  ConsentRequested = 'ConsentRequested',
  ConsentGranted = 'ConsentGranted',
  ConsentDenied = 'ConsentDenied',
  ApiCallFailed = 'ApiCallFailed',
  FilterChanged = 'FilterChanged',
  DrillDown = 'DrillDown',
}

interface ThemeSettings {
  name: 'Light' | 'Dark';
  highContrastEnabled: boolean;
  languageCode: string;
}

interface IDataAPIAdapterPort {
  getOrgBlindnessTrendsAsync(
    organizationId: string,
    departmentFilters: string[],
    tenantId: string
  ): Promise<WidgetState<OrgBlindnessTrendViewModel>>;
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
const mockDataAPIAdapter: IDataAPIAdapterPort = {
  getOrgBlindnessTrendsAsync: async (organizationId, departmentFilters, tenantId) => {
    console.log(`Mock API: Fetching org blindness data for ${organizationId} (departments: ${departmentFilters.join(', ')})`);
    return new Promise((resolve) => {
      setTimeout(() => {
        if (Math.random() > 0.1) { // Simulate 10% failure rate
          resolve({
            data: {
              blindnessRiskScore: Math.round(Math.random() * 100) / 100, // 0.0 to 1.0
              topBlindSpots: [
                'Overvaluing legacy processes',
                'Undervaluing cross-functional initiatives',
                'Lack of recognition for quiet contributors',
                'Siloed knowledge sharing',
                'Resistance to new technologies',
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
              message: 'Failed to fetch organizational blindness data from backend.',
              canRetry: true,
            },
          });
        }
      }, 2000);
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
    maxWidth: '900px',
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
  filterGroup: {
    marginBottom: '15px',
    display: 'flex',
    gap: '10px',
    alignItems: 'center',
    flexWrap: 'wrap',
  },
  filterLabel: {
    fontWeight: 'bold',
  },
  select: {
    padding: '8px',
    borderRadius: '4px',
    border: '1px solid #ddd',
    backgroundColor: '#f9f9f9',
    color: '#333',
  },
  darkSelect: {
    backgroundColor: '#444',
    borderColor: '#666',
    color: '#eee',
  },
  chartPlaceholder: {
    backgroundColor: '#e0e0e0',
    height: '200px',
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    borderRadius: '4px',
    color: '#666',
    marginBottom: '15px',
  },
  darkChartPlaceholder: {
    backgroundColor: '#555',
    color: '#bbb',
  },
};

// --- Component Definition ---

interface OrgBlindnessTrendsProps {
  userId: string;
  tenantId: string;
  organizationId: string;
  dataAPIAdapter?: IDataAPIAdapterPort;
  consentAdapter?: IConsentAdapter;
  telemetryAdapter?: ITelemetryAdapter;
  themeAdapter?: IThemeAdapter;
}

const OrgBlindnessTrends: React.FC<OrgBlindnessTrendsProps> = ({
  userId,
  tenantId,
  organizationId,
  dataAPIAdapter = mockDataAPIAdapter,
  consentAdapter = mockConsentAdapter,
  telemetryAdapter = mockTelemetryAdapter,
  themeAdapter = mockThemeAdapter,
}) => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<ErrorEnvelope | null>(null);
  const [showConsent, setShowConsent] = useState(false);
  const [consentGranted, setConsentGranted] = useState(false);
  const [orgBlindnessData, setOrgBlindnessData] = useState<OrgBlindnessTrendViewModel | null>(null);
  const [currentTheme, setCurrentTheme] = useState<ThemeSettings | null>(null);
  const [selectedTimePeriod, setSelectedTimePeriod] = useState('last_6_months');
  const [selectedDepartment, setSelectedDepartment] = useState('all');
  const [selectedRiskSeverity, setSelectedRiskSeverity] = useState('all');

  const widgetId = 'org-blindness-trends-widget';
  const panelId = 'OrgBlindnessTrendsPanel';

  useEffect(() => {
    themeAdapter.getCurrentThemeAsync().then(setCurrentTheme);
    themeAdapter.onThemeChanged(setCurrentTheme);

    telemetryAdapter.logEventAsync({
      timestamp: new Date(),
      widgetId,
      panelId,
      userId,
      action: TelemetryEventTypes.OrgBlindnessViewed,
      metadata: { tenantId, organizationId },
    });

    fetchOrgBlindnessData();
  }, [userId, tenantId, organizationId, telemetryAdapter, themeAdapter, selectedTimePeriod, selectedDepartment, selectedRiskSeverity]);

  const fetchOrgBlindnessData = async () => {
    setLoading(true);
    setError(null);

    try {
      // Check consent for value diagnostic data collection
      const hasConsent = await consentAdapter.hasActiveConsentAsync(
        userId,
        ConsentType.ValueDiagnosticDataCollection
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
          metadata: { consentType: ConsentType.ValueDiagnosticDataCollection },
        });
        return;
      }

      // Fetch organizational blindness data
      const departmentFilters = selectedDepartment === 'all' ? [] : [selectedDepartment];
      const orgBlindnessState = await dataAPIAdapter.getOrgBlindnessTrendsAsync(
        organizationId,
        departmentFilters,
        tenantId
      );

      if (orgBlindnessState.data) {
        let filteredData = orgBlindnessState.data;
        if (selectedRiskSeverity !== 'all') {
          // Mock filtering by risk severity - in a real app, this would be done by backend
          const riskThreshold = parseFloat(selectedRiskSeverity);
          if (!isNaN(riskThreshold)) {
            filteredData = {
              ...filteredData,
              topBlindSpots: filteredData.topBlindSpots.filter(spot => {
                // Simple mock logic: assume spots containing 'Overvaluing' or 'Undervaluing' are higher risk
                if (riskThreshold === 0.5) { // Medium risk
                  return spot.includes('Overvaluing') || spot.includes('Undervaluing');
                } else if (riskThreshold === 0.8) { // High risk
                  return spot.includes('Siloed') || spot.includes('Resistance');
                }
                return true;
              }),
            };
          }
        }
        setOrgBlindnessData(filteredData);
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          action: TelemetryEventTypes.OrgBlindnessReportGenerated,
          metadata: {
            organizationId,
            blindnessRiskScore: filteredData.blindnessRiskScore,
            blindSpotCount: filteredData.topBlindSpots?.length || 0,
            timePeriod: selectedTimePeriod,
            department: selectedDepartment,
            riskSeverity: selectedRiskSeverity,
          },
        });
      } else if (orgBlindnessState.lastError) {
        setError(orgBlindnessState.lastError);
        telemetryAdapter.logEventAsync({
          timestamp: new Date(),
          widgetId,
          panelId,
          userId,
          action: TelemetryEventTypes.ApiCallFailed,
          errorCode: orgBlindnessState.lastError.errorCode,
          metadata: { error: orgBlindnessState.lastError.message, api: 'getOrgBlindnessTrends' },
        });
      }

    } catch (err) {
      console.error('Error fetching organizational blindness data:', err);
      setError({
        errorCode: 'FETCH_ERROR',
        message: 'Failed to load organizational blindness data. Please try again.',
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
          fetchOrgBlindnessData(); // Re-fetch data if consent granted
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
    fetchOrgBlindnessData();
  };

  const handleTimePeriodChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setSelectedTimePeriod(e.target.value);
    telemetryAdapter.logEventAsync({
      timestamp: new Date(),
      widgetId,
      panelId,
      userId,
      action: TelemetryEventTypes.FilterChanged,
      metadata: { filter: 'timePeriod', value: e.target.value },
    });
  };

  const handleDepartmentChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setSelectedDepartment(e.target.value);
    telemetryAdapter.logEventAsync({
      timestamp: new Date(),
      widgetId,
      panelId,
      userId,
      action: TelemetryEventTypes.FilterChanged,
      metadata: { filter: 'department', value: e.target.value },
    });
  };

  const handleRiskSeverityChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setSelectedRiskSeverity(e.target.value);
    telemetryAdapter.logEventAsync({
      timestamp: new Date(),
      widgetId,
      panelId,
      userId,
      action: TelemetryEventTypes.FilterChanged,
      metadata: { filter: 'riskSeverity', value: e.target.value },
    });
  };

  const handleDrillDown = (spot: string) => {
    console.log(`Drilling down into blind spot: ${spot}`);
    telemetryAdapter.logEventAsync({
      timestamp: new Date(),
      widgetId,
      panelId,
      userId,
      action: TelemetryEventTypes.DrillDown,
      metadata: { blindSpot: spot },
    });
    alert(`Analyzing "${spot}" in more detail. (Check console for mock data)`);
  };

  const containerStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.container, ...widgetStyles.darkContainer } : widgetStyles.container;
  const headerStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.header, ...widgetStyles.darkHeader } : widgetStyles.header;
  const sectionStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.section, ...widgetStyles.darkSection } : widgetStyles.section;
  const selectStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.select, ...widgetStyles.darkSelect } : widgetStyles.select;
  const chartPlaceholderStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.chartPlaceholder, ...widgetStyles.darkChartPlaceholder } : widgetStyles.chartPlaceholder;
  const consentBoxStyle = currentTheme?.name === 'Dark' ? { ...widgetStyles.consentBox, ...widgetStyles.darkConsentBox } : widgetStyles.consentBox;

  return (
    <div style={containerStyle} role="region" aria-labelledby="org-blindness-title">
      <h2 id="org-blindness-title" style={headerStyle}>Organizational Blindness Trends</h2>
      <p>Identify areas where value creation is overlooked or undervalued within your organization.</p>

      {loading && <p style={widgetStyles.loading} aria-live="polite">Loading organizational blindness data...</p>}

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
            <h3 id="consent-title">Consent Required for Organizational Diagnostics</h3>
            <p>
              To display comprehensive organizational blindness trends, we need your consent to collect and process aggregated organizational data.
              This data will be used solely for generating diagnostic reports and will not be shared externally without further explicit consent.
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
          <div style={widgetStyles.filterGroup}>
            <label htmlFor="time-period-filter" style={widgetStyles.filterLabel}>Time Period:</label>
            <select id="time-period-filter" style={selectStyle} value={selectedTimePeriod} onChange={handleTimePeriodChange}>
              <option value="last_3_months">Last 3 Months</option>
              <option value="last_6_months">Last 6 Months</option>
              <option value="last_12_months">Last 12 Months</option>
              <option value="all_time">All Time</option>
            </select>

            <label htmlFor="department-filter" style={widgetStyles.filterLabel}>Department:</label>
            <select id="department-filter" style={selectStyle} value={selectedDepartment} onChange={handleDepartmentChange}>
              <option value="all">All Departments</option>
              <option value="engineering">Engineering</option>
              <option value="marketing">Marketing</option>
              <option value="sales">Sales</option>
              <option value="hr">HR</option>
            </select>

            <label htmlFor="risk-severity-filter" style={widgetStyles.filterLabel}>Risk Severity:</label>
            <select id="risk-severity-filter" style={selectStyle} value={selectedRiskSeverity} onChange={handleRiskSeverityChange}>
              <option value="all">All</option>
              <option value="0.5">Medium</option>
              <option value="0.8">High</option>
            </select>
          </div>

          {orgBlindnessData ? (
            <div style={sectionStyle} aria-labelledby="org-blindness-summary-header">
              <h3 id="org-blindness-summary-header">Overall Organizational Blindness</h3>
              <p><strong>Blindness Risk Score:</strong> {orgBlindnessData.blindnessRiskScore.toFixed(2)}</p>
              <h4>Top Identified Blind Spots:</h4>
              {orgBlindnessData.topBlindSpots && orgBlindnessData.topBlindSpots.length > 0 ? (
                <ul style={widgetStyles.list}>
                  {orgBlindnessData.topBlindSpots.map((spot, i) => (
                    <li key={i}>
                      {spot}
                      <button
                        style={{ ...widgetStyles.button, ...widgetStyles.buttonSecondary, marginLeft: '10px', padding: '5px 10px', fontSize: '12px' }}
                        onClick={() => handleDrillDown(spot)}
                      >
                        Drill Down
                      </button>
                    </li>
                  ))}
                </ul>
              ) : (
                <p>No significant blind spots identified for the selected filters.</p>
              )}
              <div style={chartPlaceholderStyle} aria-label="Placeholder for Organizational Blindness Trend Chart">
                [Mock Chart: Org Blindness Trend Over Time ({selectedTimePeriod})]
              </div>
            </div>
          ) : (
            <p style={{ textAlign: 'center', color: '#6c757d' }}>No organizational blindness data available for the selected filters.</p>
          )}

          <div style={sectionStyle} aria-labelledby="departmental-breakdown-header">
            <h3 id="departmental-breakdown-header">Departmental Breakdown</h3>
            <div style={chartPlaceholderStyle} aria-label="Placeholder for Departmental Blindness Breakdown Chart">
              [Mock Chart: Blindness Score by Department]
            </div>
            <button
              style={widgetStyles.button}
              onClick={() => handleDrillDown('Departmental Breakdown')}
            >
              View Departmental Details
            </button>
          </div>

          <div style={sectionStyle} aria-labelledby="actionable-insights-header">
            <h3 id="actionable-insights-header">Actionable Insights</h3>
            <p>Based on the trends, here are some recommended actions:</p>
            <ul style={widgetStyles.list}>
              <li>Implement cross-functional knowledge sharing initiatives.</li>
              <li>Review resource allocation in undervalued areas.</li>
              <li>Develop a recognition program for quiet contributors.</li>
            </ul>
            <button
              style={widgetStyles.button}
              onClick={() => handleDrillDown('Actionable Insights')}
            >
              Generate Action Plan
            </button>
          </div>
        </>
      )}
    </div>
  );
};

export default OrgBlindnessTrends;
