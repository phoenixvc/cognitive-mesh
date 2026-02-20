/**
 * @fileoverview Public API for the Cognitive Mesh i18n / Localization module.
 *
 * Re-exports configuration, hooks, components, and type definitions
 * so consumers can import everything from a single entry point:
 *
 * ```ts
 * import { useCognitiveMeshTranslation, LanguageSelector, SUPPORTED_LANGUAGES } from '../Localization';
 * ```
 */

// Configuration & constants
export {
  default as i18n,
  SUPPORTED_LANGUAGES,
  DEFAULT_LANGUAGE,
  FALLBACK_LANGUAGE,
  DEFAULT_NAMESPACE,
  LANGUAGE_LABELS,
} from './i18nConfig';
export type { SupportedLanguage } from './i18nConfig';

// Hooks
export {
  useCognitiveMeshTranslation,
  default as useTranslation,
} from './hooks/useTranslation';
export type {
  CommonTranslationKey,
  TypedTFunction,
  TranslationNamespace,
  CognitiveMeshTranslationResult,
} from './hooks/useTranslation';

// Components
export { default as LanguageSelector } from './components/LanguageSelector';
export type { LanguageSelectorProps } from './components/LanguageSelector';
