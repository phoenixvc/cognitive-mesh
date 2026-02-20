/**
 * @fileoverview Internationalization (i18n) configuration for Cognitive Mesh UI.
 *
 * Configures react-i18next with namespace-based resource loading,
 * fallback language support, and lazy-loaded locale bundles.
 *
 * Supported locales: en-US, fr-FR, de-DE
 * Default / fallback: en-US
 */

import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

import enUSCommon from './locales/en-US/common.json';
import frFRCommon from './locales/fr-FR/common.json';
import deDECommon from './locales/de-DE/common.json';

/** All locale identifiers the application supports. */
export const SUPPORTED_LANGUAGES = ['en-US', 'fr-FR', 'de-DE'] as const;

/** Union type of supported locale codes. */
export type SupportedLanguage = (typeof SUPPORTED_LANGUAGES)[number];

/** The locale used when a requested key is missing from the active language. */
export const DEFAULT_LANGUAGE: SupportedLanguage = 'en-US';

/** The fallback locale (identical to default for safety). */
export const FALLBACK_LANGUAGE: SupportedLanguage = 'en-US';

/** Default namespace used when no namespace is specified in a translation call. */
export const DEFAULT_NAMESPACE = 'common';

/**
 * Human-readable labels for each supported language, including native
 * script names and optional flag emoji for display purposes.
 */
export const LANGUAGE_LABELS: Record<SupportedLanguage, { label: string; nativeLabel: string; flag: string }> = {
  'en-US': { label: 'English (US)', nativeLabel: 'English', flag: '\uD83C\uDDFA\uD83C\uDDF8' },
  'fr-FR': { label: 'French (France)', nativeLabel: 'Fran\u00E7ais', flag: '\uD83C\uDDEB\uD83C\uDDF7' },
  'de-DE': { label: 'German (Germany)', nativeLabel: 'Deutsch', flag: '\uD83C\uDDE9\uD83C\uDDEA' },
};

/**
 * Bundled translation resources keyed by locale and namespace.
 *
 * Resources are imported statically so the initial bundle always contains
 * the fallback language. Additional namespaces can be added later via
 * `i18n.addResourceBundle`.
 */
const resources = {
  'en-US': { common: enUSCommon },
  'fr-FR': { common: frFRCommon },
  'de-DE': { common: deDECommon },
};

/**
 * Reads the persisted language preference from `localStorage`, falling
 * back to `DEFAULT_LANGUAGE` when no preference has been stored or
 * the stored value is no longer in the supported set.
 */
function getPersistedLanguage(): SupportedLanguage {
  try {
    const stored = localStorage.getItem('cognitivemesh_language');
    if (stored && (SUPPORTED_LANGUAGES as readonly string[]).includes(stored)) {
      return stored as SupportedLanguage;
    }
  } catch {
    // localStorage may be unavailable (SSR, privacy mode, etc.)
  }
  return DEFAULT_LANGUAGE;
}

i18n
  .use(initReactI18next)
  .init({
    resources,
    lng: getPersistedLanguage(),
    fallbackLng: FALLBACK_LANGUAGE,
    defaultNS: DEFAULT_NAMESPACE,
    ns: [DEFAULT_NAMESPACE],

    interpolation: {
      /**
       * Escaping is disabled because React already protects against XSS
       * by escaping rendered values. Enabling this would cause
       * double-escaping in JSX output.
       */
      escapeValue: false,
    },

    react: {
      /** Suspend rendering until translations are ready. */
      useSuspense: true,
    },

    /**
     * When `true`, keys that are missing from the current language AND
     * the fallback language are returned as-is so the developer can spot
     * untranslated strings during development.
     */
    returnEmptyString: false,

    /**
     * Enables debug logging in non-production environments to surface
     * missing keys and namespace resolution issues.
     */
    debug: process.env.NODE_ENV === 'development',
  });

export default i18n;
