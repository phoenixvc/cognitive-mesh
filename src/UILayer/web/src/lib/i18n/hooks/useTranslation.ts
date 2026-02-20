/**
 * @fileoverview Type-safe translation hook for Cognitive Mesh.
 *
 * Wraps react-i18next's `useTranslation` with project-specific
 * typing so that callers receive auto-complete for translation keys
 * and consistent defaults for the namespace argument.
 */

import { useTranslation, UseTranslationResponse } from 'react-i18next';
import type { TFunction } from 'i18next';
import type enUSCommon from '../locales/en-US/common.json';

// ---------------------------------------------------------------------------
// Type helpers
// ---------------------------------------------------------------------------

/**
 * Recursively flattens a nested JSON object type into dot-separated key paths.
 *
 * @example
 * ```ts
 * type Keys = FlatKeys<{ a: { b: string; c: string } }>;
 * // "a.b" | "a.c"
 * ```
 */
type FlatKeys<T, Prefix extends string = ''> = T extends object
  ? {
      [K in keyof T & string]: T[K] extends object
        ? FlatKeys<T[K], `${Prefix}${K}.`>
        : `${Prefix}${K}`;
    }[keyof T & string]
  : never;

/**
 * Union of every valid translation key in the `common` namespace.
 * Derived from the canonical en-US resource file so that new keys
 * automatically appear in the type.
 */
export type CommonTranslationKey = FlatKeys<typeof enUSCommon>;

/**
 * A narrowed version of i18next's `TFunction` that only accepts
 * keys from the `common` namespace, preventing typos at compile time.
 */
export type TypedTFunction = {
  (key: CommonTranslationKey, options?: Record<string, unknown>): string;
};

// ---------------------------------------------------------------------------
// Namespace constants
// ---------------------------------------------------------------------------

/** All known namespaces in the application. Extend as new namespaces are added. */
export type TranslationNamespace = 'common';

/** The default namespace when none is provided. */
const DEFAULT_NS: TranslationNamespace = 'common';

// ---------------------------------------------------------------------------
// Hook
// ---------------------------------------------------------------------------

/**
 * Return type of {@link useCognitiveMeshTranslation}.
 *
 * Exposes the typed `t` function, the i18n instance, and a boolean
 * indicating whether the translations are still loading.
 */
export interface CognitiveMeshTranslationResult {
  /** Typed translation function scoped to the requested namespace. */
  t: TypedTFunction;
  /** The underlying i18next instance for advanced operations. */
  i18n: UseTranslationResponse<TranslationNamespace>[1];
  /** `true` while the namespace resources are still being loaded. */
  ready: boolean;
}

/**
 * Custom hook that wraps `react-i18next`'s `useTranslation` with
 * Cognitive Mesh-specific type safety.
 *
 * @param ns - The namespace to load. Defaults to `'common'`.
 * @returns An object containing the typed `t` function, the `i18n`
 *          instance, and a `ready` flag.
 *
 * @example
 * ```tsx
 * function AgentCard() {
 *   const { t } = useCognitiveMeshTranslation();
 *   return <span>{t('agent.status.active')}</span>;
 * }
 * ```
 *
 * @example
 * ```tsx
 * // With interpolation
 * function PageIndicator({ current, total }: { current: number; total: number }) {
 *   const { t } = useCognitiveMeshTranslation();
 *   return <span>{t('pagination.pageOf', { current, total })}</span>;
 * }
 * ```
 */
export function useCognitiveMeshTranslation(
  ns: TranslationNamespace = DEFAULT_NS,
): CognitiveMeshTranslationResult {
  const { t, i18n, ready } = useTranslation(ns);

  return {
    // Cast to our narrowed function type. At runtime this is still
    // the same i18next t-function, but TypeScript will enforce key
    // correctness at call sites.
    t: t as unknown as TypedTFunction,
    i18n,
    ready,
  };
}

export default useCognitiveMeshTranslation;
