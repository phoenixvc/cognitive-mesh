import React, { CSSProperties } from 'react';

// --- Type Definitions ---
export enum AutonomyLevel {
  SovereigntyFirst = 'SovereigntyFirst',
  RecommendOnly = 'RecommendOnly',
  ActWithConfirmation = 'ActWithConfirmation',
  FullyAutonomous = 'FullyAutonomous',
}

interface ThemeSettings {
  name: 'Light' | 'Dark';
  highContrastEnabled: boolean;
  languageCode: string;
}

type BadgeSize = 'small' | 'medium' | 'large';
type DisplayVariant = 'compact' | 'expanded';

// --- Component Props ---
interface AuthorityLevelBadgeProps {
  level: AutonomyLevel;
  theme?: ThemeSettings;
  size?: BadgeSize; // Optional: 'small', 'medium', 'large'
  variant?: DisplayVariant; // Optional: 'compact', 'expanded'
  tooltip?: string; // Optional: custom tooltip text
}

// --- Component Styles ---
const getStyles = (theme: ThemeSettings, size: BadgeSize) => {
  const isDark = theme?.name === 'Dark';

  const baseSize = {
    small: { fontSize: '10px', padding: '2px 6px', iconSize: '12px' },
    medium: { fontSize: '12px', padding: '4px 8px', iconSize: '16px' },
    large: { fontSize: '14px', padding: '6px 10px', iconSize: '20px' },
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
      border: '1px solid transparent', // For high contrast
    },
    icon: {
      fontSize: currentSize.iconSize,
      lineHeight: 1,
    },
    description: {
      fontSize: '0.9em',
      marginTop: '5px',
      color: isDark ? '#bbb' : '#555',
    },
  };
};

// --- Helper Functions ---
const getAuthorityColors = (level: AutonomyLevel) => {
  switch (level) {
    case AutonomyLevel.SovereigntyFirst:
      return { background: '#e6f7ff', text: '#007bff', icon: '#007bff', border: '#007bff' }; // Light Blue
    case AutonomyLevel.RecommendOnly:
      return { background: '#f2f2f2', text: '#6c757d', icon: '#6c757d', border: '#6c757d' }; // Light Gray
    case AutonomyLevel.ActWithConfirmation:
      return { background: '#fff0e6', text: '#fd7e14', icon: '#fd7e14', border: '#fd7e14' }; // Light Orange
    case AutonomyLevel.FullyAutonomous:
      return { background: '#e6ffe6', text: '#28a745', icon: '#28a745', border: '#28a745' }; // Light Green
    default:
      return { background: '#f2f2f2', text: '#6c757d', icon: '#6c757d', border: '#6c757d' };
  }
};

const getAuthorityText = (level: AutonomyLevel): string => {
  switch (level) {
    case AutonomyLevel.SovereigntyFirst: return 'Sovereignty First';
    case AutonomyLevel.RecommendOnly: return 'Recommend Only';
    case AutonomyLevel.ActWithConfirmation: return 'Act with Confirmation';
    case AutonomyLevel.FullyAutonomous: return 'Fully Autonomous';
    default: return 'Unknown';
  }
};

const getAuthorityTooltip = (level: AutonomyLevel): string => {
  switch (level) {
    case AutonomyLevel.SovereigntyFirst: return 'Human authorship is paramount; AI assistance is minimal and explicit.';
    case AutonomyLevel.RecommendOnly: return 'The agent can only analyze and provide recommendations; human approval is always required for action.';
    case AutonomyLevel.ActWithConfirmation: return 'The agent can propose and prepare actions, but requires explicit user confirmation before execution.';
    case AutonomyLevel.FullyAutonomous: return 'The agent can act independently within its defined authority scope without requiring confirmation for each action.';
    default: return 'Defines the degree of independent decision-making an agent can exercise.';
  }
};

const getAuthorityDescription = (level: AutonomyLevel): string => {
  switch (level) {
    case AutonomyLevel.SovereigntyFirst: return 'This mode prioritizes human control and oversight, ensuring that AI systems act strictly as assistants rather than autonomous agents. All critical decisions and actions require explicit human intervention and approval.';
    case AutonomyLevel.RecommendOnly: return 'Agents operating at this level function as intelligent advisors. They can process information, generate insights, and suggest courses of action, but they cannot execute any changes or operations without direct human authorization.';
    case AutonomyLevel.ActWithConfirmation: return 'This is a hybrid mode where agents can prepare and stage actions, but a human user must review and confirm each step before it is executed. It provides a balance between automation efficiency and human oversight for moderate-risk tasks.';
    case AutonomyLevel.FullyAutonomous: return 'Agents at this level are empowered to execute actions independently within their predefined boundaries and authority scope. This mode is typically used for low-risk, high-volume, or time-sensitive operations where immediate action is beneficial and human intervention is not routinely required.';
    default: return 'No detailed description available for this authority level.';
  }
};

const getAuthorityIcon = (level: AutonomyLevel): string => {
  switch (level) {
    case AutonomyLevel.SovereigntyFirst: return '👑'; // Crown
    case AutonomyLevel.RecommendOnly: return '💡'; // Lightbulb
    case AutonomyLevel.ActWithConfirmation: return '🤝'; // Handshake
    case AutonomyLevel.FullyAutonomous: return '⚙️'; // Gear
    default: return '❓'; // Question Mark
  }
};

// --- Main Component ---
const AuthorityLevelBadge: React.FC<AuthorityLevelBadgeProps> = ({
  level,
  theme = { name: 'Light', highContrastEnabled: false, languageCode: 'en-US' },
  size = 'medium',
  variant = 'compact',
  tooltip,
}) => {
  const styles = getStyles(theme, size);
  const colors = getAuthorityColors(level);
  const levelText = getAuthorityText(level);
  const finalTooltip = tooltip || getAuthorityTooltip(level);
  const levelDescription = getAuthorityDescription(level);

  const isDark = theme.name === 'Dark';

  const containerStyle: CSSProperties = {
    ...styles.container,
    backgroundColor: isDark ? colors.background.replace('e6', '33').replace('f2', '44').replace('ff', '22') : colors.background,
    color: isDark ? (colors.text === '#212529' ? '#f0f0f0' : colors.icon) : colors.text,
    borderColor: isDark ? colors.border.replace('#', '#') : colors.border, // Ensure border is visible in dark mode
  };

  const iconStyle: CSSProperties = {
    ...styles.icon,
    color: colors.icon,
  };

  return (
    <div
      style={containerStyle}
      title={finalTooltip}
      role="status"
      aria-live="polite"
      aria-label={`Agent authority level: ${levelText}`}
    >
      {(variant === 'expanded' || size === 'large') && ( // Show icon for expanded or large compact
        <span style={iconStyle} aria-hidden="true">{getAuthorityIcon(level)}</span>
      )}
      <span>{levelText}</span>
      {variant === 'expanded' && (
        <p style={styles.description}>{levelDescription}</p>
      )}
    </div>
  );
};

export default AuthorityLevelBadge;
