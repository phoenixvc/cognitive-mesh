/**
 * @fileoverview Language selector component for Cognitive Mesh.
 *
 * Renders an accessible dropdown that lets the user switch the
 * application locale. The selection is persisted to `localStorage`
 * and a telemetry event is emitted on every change.
 */

import React, { useCallback, useRef, useState, useEffect, CSSProperties } from 'react';
import {
  SUPPORTED_LANGUAGES,
  LANGUAGE_LABELS,
  type SupportedLanguage,
} from '../i18nConfig';
import { useCognitiveMeshTranslation } from '../hooks/useTranslation';

// ---------------------------------------------------------------------------
// Constants
// ---------------------------------------------------------------------------

/** localStorage key used to persist the selected language. */
const STORAGE_KEY = 'cognitivemesh_language';

/** Telemetry action name emitted when the language changes. */
const TELEMETRY_ACTION = 'LanguageChanged';

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

/** Shape of the telemetry event dispatched by this component. */
interface LanguageChangeTelemetryEvent {
  timestamp: Date;
  action: string;
  metadata: {
    previousLanguage: string;
    newLanguage: string;
    source: string;
  };
}

/** Optional adapter for dispatching telemetry events. */
interface ITelemetryAdapter {
  logEventAsync(event: LanguageChangeTelemetryEvent): Promise<void>;
}

/** Props accepted by the {@link LanguageSelector} component. */
export interface LanguageSelectorProps {
  /** Optional telemetry adapter. When provided, a telemetry event is
   *  emitted every time the user changes the language. */
  telemetryAdapter?: ITelemetryAdapter;
  /** Additional CSS class name applied to the root element. */
  className?: string;
  /** Inline styles applied to the root `<div>` wrapper. */
  style?: CSSProperties;
}

// ---------------------------------------------------------------------------
// Styles
// ---------------------------------------------------------------------------

const styles: Record<string, CSSProperties> = {
  wrapper: {
    display: 'inline-flex',
    alignItems: 'center',
    position: 'relative',
  },
  select: {
    appearance: 'none',
    WebkitAppearance: 'none',
    MozAppearance: 'none',
    padding: '6px 32px 6px 10px',
    borderRadius: '4px',
    border: '1px solid #ccc',
    backgroundColor: '#fff',
    color: '#333',
    fontSize: '14px',
    cursor: 'pointer',
    fontFamily: 'inherit',
    lineHeight: 1.4,
  },
  chevron: {
    position: 'absolute',
    right: '10px',
    top: '50%',
    transform: 'translateY(-50%)',
    pointerEvents: 'none',
    fontSize: '10px',
    color: '#666',
  },
};

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

/**
 * Accessible language selector dropdown for the Cognitive Mesh UI.
 *
 * Features:
 * - Displays flag emoji and native language name for each option
 * - Persists the selection to `localStorage`
 * - Fires a telemetry event on every change
 * - Fully keyboard-navigable (`Tab`, `Enter`, arrow keys)
 * - Includes `aria-label` for screen readers
 *
 * @example
 * ```tsx
 * <LanguageSelector telemetryAdapter={myTelemetryAdapter} />
 * ```
 */
const LanguageSelector: React.FC<LanguageSelectorProps> = ({
  telemetryAdapter,
  className,
  style,
}) => {
  const { i18n, t } = useCognitiveMeshTranslation();
  const [currentLanguage, setCurrentLanguage] = useState<SupportedLanguage>(
    (i18n.language as SupportedLanguage) ?? 'en-US',
  );
  const selectRef = useRef<HTMLSelectElement>(null);

  // Keep local state in sync if the language is changed externally
  // (e.g. via another LanguageSelector instance or programmatically).
  useEffect(() => {
    const handleLanguageChanged = (lng: string) => {
      if ((SUPPORTED_LANGUAGES as readonly string[]).includes(lng)) {
        setCurrentLanguage(lng as SupportedLanguage);
      }
    };
    i18n.on('languageChanged', handleLanguageChanged);
    return () => {
      i18n.off('languageChanged', handleLanguageChanged);
    };
  }, [i18n]);

  /**
   * Handles the `<select>` change event: updates i18n language,
   * persists the preference, and emits telemetry.
   */
  const handleChange = useCallback(
    async (event: React.ChangeEvent<HTMLSelectElement>) => {
      const newLanguage = event.target.value as SupportedLanguage;
      const previousLanguage = currentLanguage;

      if (newLanguage === previousLanguage) {
        return;
      }

      // Update i18next language (triggers re-render across all consumers).
      await i18n.changeLanguage(newLanguage);

      // Persist preference.
      try {
        localStorage.setItem(STORAGE_KEY, newLanguage);
      } catch {
        // localStorage may be unavailable; fail silently.
      }

      setCurrentLanguage(newLanguage);

      // Emit telemetry event.
      if (telemetryAdapter) {
        try {
          await telemetryAdapter.logEventAsync({
            timestamp: new Date(),
            action: TELEMETRY_ACTION,
            metadata: {
              previousLanguage,
              newLanguage,
              source: 'LanguageSelector',
            },
          });
        } catch {
          // Telemetry failures should never block the UI.
        }
      }
    },
    [currentLanguage, i18n, telemetryAdapter],
  );

  return (
    <div
      className={className}
      style={{ ...styles.wrapper, ...style }}
      data-testid="language-selector"
    >
      <select
        ref={selectRef}
        value={currentLanguage}
        onChange={handleChange}
        aria-label={t('language.selector.label')}
        style={styles.select}
      >
        {SUPPORTED_LANGUAGES.map((lang) => {
          const { flag, nativeLabel } = LANGUAGE_LABELS[lang];
          return (
            <option key={lang} value={lang}>
              {flag} {nativeLabel}
            </option>
          );
        })}
      </select>
      <span style={styles.chevron} aria-hidden="true">
        &#9662;
      </span>
    </div>
  );
};

export default LanguageSelector;
