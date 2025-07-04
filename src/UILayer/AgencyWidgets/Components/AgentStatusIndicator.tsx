import React, { CSSProperties, useEffect } from 'react';

// --- Type Definitions ---
type AgentStatus =
  | 'idle'
  | 'executing'
  | 'awaiting_approval'
  | 'offline'
  | 'circuit_broken'
  | 'authority_required'
  | 'consent_required'
  | 'error';

type ThemeSettings = {
  name: 'Light' | 'Dark';
  highContrastEnabled: boolean;
  languageCode: string;
};

type IndicatorSize = 'small' | 'medium' | 'large';
type DisplayVariant = 'icon' | 'text' | 'both';

// --- Component Props ---
interface AgentStatusIndicatorProps {
  status: AgentStatus;
  theme?: ThemeSettings;
  isPulsing?: boolean; // Optional: for active states like 'executing'
  tooltip?: string; // Optional: additional context on hover
  size?: IndicatorSize; // Optional: 'small', 'medium', 'large'
  variant?: DisplayVariant; // Optional: 'icon', 'text', 'both'
}

// --- Component Styles ---
const getStyles = (theme: ThemeSettings, size: IndicatorSize) => {
  const isDark = theme?.name === 'Dark';

  const baseSize = {
    small: { indicator: '8px', fontSize: '10px', padding: '2px 6px' },
    medium: { indicator: '12px', fontSize: '12px', padding: '4px 8px' },
    large: { indicator: '16px', fontSize: '14px', padding: '6px 10px' },
  };

  const currentSize = baseSize[size];

  return {
    container: {
      display: 'inline-flex',
      alignItems: 'center',
      gap: '5px',
      padding: currentSize.padding,
      borderRadius: '16px',
      fontWeight: 'bold' as 'bold',
      fontSize: currentSize.fontSize,
      transition: 'background-color 0.3s ease, color 0.3s ease',
      whiteSpace: 'nowrap' as 'nowrap',
      cursor: 'default',
    },
    indicator: {
      width: currentSize.indicator,
      height: currentSize.indicator,
      borderRadius: '50%',
      transition: 'background-color 0.3s ease',
    },
    // Keyframes for pulsing animation
    '@keyframes pulse': {
      '0%': { boxShadow: '0 0 0 0 rgba(0, 123, 255, 0.7)' },
      '70%': { boxShadow: '0 0 0 10px rgba(0, 123, 255, 0)' },
      '100%': { boxShadow: '0 0 0 0 rgba(0, 123, 255, 0)' },
    },
    // Specific pulse for dark theme
    '@keyframes pulseDark': {
      '0%': { boxShadow: '0 0 0 0 rgba(135, 206, 250, 0.7)' },
      '70%': { boxShadow: '0 0 0 10px rgba(135, 206, 250, 0)' },
      '100%': { boxShadow: '0 0 0 0 rgba(135, 206, 250, 0)' },
    },
  };
};

// --- Helper Functions ---
const getStatusColors = (status: AgentStatus) => {
  switch (status) {
    case 'idle':
      return { background: '#e6ffe6', text: '#28a745', indicator: '#28a745', pulse: 'none' }; // Light Green
    case 'executing':
      return { background: '#e6f7ff', text: '#007bff', indicator: '#007bff', pulse: 'pulse' }; // Light Blue
    case 'awaiting_approval':
      return { background: '#fffbe6', text: '#ffc107', indicator: '#ffc107', pulse: 'pulse' }; // Light Yellow
    case 'offline':
      return { background: '#f2f2f2', text: '#6c757d', indicator: '#6c757d', pulse: 'none' }; // Light Gray
    case 'circuit_broken':
      return { background: '#ffe6e6', text: '#dc3545', indicator: '#dc3545', pulse: 'pulse' }; // Light Red
    case 'authority_required':
      return { background: '#fff0e6', text: '#fd7e14', indicator: '#fd7e14', pulse: 'pulse' }; // Light Orange
    case 'consent_required':
      return { background: '#fff0e6', text: '#fd7e14', indicator: '#fd7e14', pulse: 'pulse' }; // Light Orange
    case 'error':
      return { background: '#ffe6e6', text: '#dc3545', indicator: '#dc3545', pulse: 'none' }; // Light Red
    default:
      return { background: '#f2f2f2', text: '#6c757d', indicator: '#6c757d', pulse: 'none' };
  }
};

const getStatusText = (status: AgentStatus): string => {
  switch (status) {
    case 'idle': return 'Idle';
    case 'executing': return 'Executing';
    case 'awaiting_approval': return 'Awaiting Approval';
    case 'offline': return 'Offline';
    case 'circuit_broken': return 'Circuit Broken';
    case 'authority_required': return 'Authority Required';
    case 'consent_required': return 'Consent Required';
    case 'error': return 'Error';
    default: return 'Unknown';
  }
};

const getStatusTooltip = (status: AgentStatus): string => {
  switch (status) {
    case 'idle': return 'The agent is currently inactive and ready for tasks.';
    case 'executing': return 'The agent is actively performing a task.';
    case 'awaiting_approval': return 'The agent is paused, awaiting human approval to proceed.';
    case 'offline': return 'The agent is not reachable or has been shut down.';
    case 'circuit_broken': return 'The agent\'s circuit breaker is open due to repeated failures. It will not process new requests.';
    case 'authority_required': return 'The agent requires elevated authority to perform the requested action.';
    case 'consent_required': return 'The agent requires explicit user consent for the current operation.';
    case 'error': return 'The agent encountered an unrecoverable error.';
    default: return 'Current status of the agent.';
  }
};

// --- Main Component ---
const AgentStatusIndicator: React.FC<AgentStatusIndicatorProps> = ({
  status,
  theme = { name: 'Light', highContrastEnabled: false, languageCode: 'en-US' },
  isPulsing = false,
  tooltip,
  size = 'medium',
  variant = 'both',
}) => {
  const styles = getStyles(theme, size);
  const colors = getStatusColors(status);
  const statusText = getStatusText(status);
  const finalTooltip = tooltip || getStatusTooltip(status);

  const isDark = theme.name === 'Dark';

  const containerStyle: CSSProperties = {
    ...styles.container,
    backgroundColor: isDark ? colors.background.replace('e6', '33').replace('f2', '44').replace('ff', '22') : colors.background,
    color: isDark ? (colors.text === '#212529' ? '#f0f0f0' : colors.indicator) : colors.text,
  };

  const indicatorStyle: CSSProperties = {
    ...styles.indicator,
    backgroundColor: colors.indicator,
    animation: isPulsing && colors.pulse !== 'none'
      ? `${isDark ? 'pulseDark' : 'pulse'} 2s infinite`
      : 'none',
  };

  // Inject keyframes into the document head
  useEffect(() => {
    const styleSheet = document.createElement("style");
    styleSheet.type = "text/css";
    styleSheet.innerText = `
      @keyframes pulse {
        0% { box-shadow: 0 0 0 0 rgba(0, 123, 255, 0.7); }
        70% { box-shadow: 0 0 0 10px rgba(0, 123, 255, 0); }
        100% { box-shadow: 0 0 0 0 rgba(0, 123, 255, 0); }
      }
      @keyframes pulseDark {
        0% { box-shadow: 0 0 0 0 rgba(135, 206, 250, 0.7); }
        70% { box-shadow: 0 0 0 10px rgba(135, 206, 250, 0); }
        100% { box-shadow: 0 0 0 0 rgba(135, 206, 250, 0); }
      }
    `;
    document.head.appendChild(styleSheet);
    return () => {
      document.head.removeChild(styleSheet);
    };
  }, []);


  return (
    <div
      style={containerStyle}
      title={finalTooltip}
      role="status"
      aria-live="polite"
      aria-label={`Agent status: ${statusText}`}
    >
      {(variant === 'icon' || variant === 'both') && (
        <span style={indicatorStyle} aria-hidden="true"></span>
      )}
      {(variant === 'text' || variant === 'both') && (
        <span>{statusText}</span>
      )}
    </div>
  );
};

export default AgentStatusIndicator;
