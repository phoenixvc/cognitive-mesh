/**
 * @fileoverview Service Worker registration utility for Cognitive Mesh.
 *
 * Provides a single entry point (`registerServiceWorker`) that:
 * - Checks for Service Worker API support
 * - Registers the worker with scope `'/'`
 * - Handles update lifecycle (new version notification)
 * - Exposes an `unregisterServiceWorker` helper for testing
 */

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

/**
 * Callback invoked when a new service worker version becomes available.
 *
 * @param registration - The SW registration that contains the new worker.
 */
export type OnUpdateCallback = (registration: ServiceWorkerRegistration) => void;

/**
 * Callback invoked when the service worker has been registered for the first
 * time and is controlling the page.
 *
 * @param registration - The newly active SW registration.
 */
export type OnSuccessCallback = (registration: ServiceWorkerRegistration) => void;

/** Configuration options for {@link registerServiceWorker}. */
export interface ServiceWorkerConfig {
  /** Called when a new service worker is installed and waiting to activate. */
  onUpdate?: OnUpdateCallback;
  /** Called when the service worker is first installed and active. */
  onSuccess?: OnSuccessCallback;
  /** Path to the compiled service worker file. Defaults to `'/sw.js'`. */
  swUrl?: string;
  /** Registration scope. Defaults to `'/'`. */
  scope?: string;
}

// ---------------------------------------------------------------------------
// Registration
// ---------------------------------------------------------------------------

/**
 * Registers the Cognitive Mesh service worker.
 *
 * The function is a no-op in environments where the Service Worker
 * API is not available (e.g. SSR, older browsers, insecure contexts).
 *
 * @param config - Optional configuration overrides.
 * @returns A promise that resolves to the registration, or `undefined`
 *          if service workers are not supported.
 *
 * @example
 * ```ts
 * import { registerServiceWorker } from './ServiceWorker/register';
 *
 * registerServiceWorker({
 *   onUpdate: (reg) => {
 *     // Prompt the user to refresh for the new version.
 *     showUpdateNotification(reg);
 *   },
 *   onSuccess: (reg) => {
 *     console.log('Service worker active:', reg.scope);
 *   },
 * });
 * ```
 */
export async function registerServiceWorker(
  config: ServiceWorkerConfig = {},
): Promise<ServiceWorkerRegistration | undefined> {
  const { onUpdate, onSuccess, swUrl = '/sw.js', scope = '/' } = config;

  // Guard: Service Worker API must be available.
  if (!('serviceWorker' in navigator)) {
    console.warn('[SW Register] Service workers are not supported in this browser.');
    return undefined;
  }

  // Guard: Must be served over HTTPS or localhost.
  if (
    window.location.protocol !== 'https:' &&
    !window.location.hostname.includes('localhost') &&
    window.location.hostname !== '127.0.0.1'
  ) {
    console.warn('[SW Register] Service workers require HTTPS (or localhost).');
    return undefined;
  }

  try {
    const registration = await navigator.serviceWorker.register(swUrl, { scope });
    console.log(`[SW Register] Registered with scope: ${registration.scope}`);

    // Listen for updates.
    registration.onupdatefound = () => {
      const installingWorker = registration.installing;

      if (!installingWorker) {
        return;
      }

      installingWorker.onstatechange = () => {
        if (installingWorker.state !== 'installed') {
          return;
        }

        if (navigator.serviceWorker.controller) {
          // A new worker is installed but waiting to activate.
          // This means there is an update available.
          console.log('[SW Register] New content is available; please refresh.');
          onUpdate?.(registration);
        } else {
          // First-time install -- the worker is now controlling the page.
          console.log('[SW Register] Content is cached for offline use.');
          onSuccess?.(registration);
        }
      };
    };

    // If there is already an active service worker, notify success.
    if (registration.active && !navigator.serviceWorker.controller) {
      // Edge case: page was hard-refreshed, existing SW but no controller.
      onSuccess?.(registration);
    }

    return registration;
  } catch (error) {
    console.error('[SW Register] Registration failed:', error);
    return undefined;
  }
}

// ---------------------------------------------------------------------------
// Unregistration (useful for development & testing)
// ---------------------------------------------------------------------------

/**
 * Unregisters all service workers and clears all caches.
 *
 * Intended for development, testing, or when a user needs to
 * fully reset the application state.
 *
 * @returns `true` if at least one registration was unregistered.
 *
 * @example
 * ```ts
 * import { unregisterServiceWorker } from './ServiceWorker/register';
 *
 * // In a "Reset App" handler:
 * await unregisterServiceWorker();
 * window.location.reload();
 * ```
 */
export async function unregisterServiceWorker(): Promise<boolean> {
  if (!('serviceWorker' in navigator)) {
    return false;
  }

  try {
    const registrations = await navigator.serviceWorker.getRegistrations();
    const results = await Promise.all(
      registrations.map((registration) => registration.unregister()),
    );

    // Also clear caches.
    const cacheNames = await caches.keys();
    await Promise.all(cacheNames.map((name) => caches.delete(name)));

    const unregisteredCount = results.filter(Boolean).length;
    if (unregisteredCount > 0) {
      console.log(`[SW Register] Unregistered ${unregisteredCount} service worker(s) and cleared caches.`);
    }

    return unregisteredCount > 0;
  } catch (error) {
    console.error('[SW Register] Unregistration failed:', error);
    return false;
  }
}

// ---------------------------------------------------------------------------
// Update helpers
// ---------------------------------------------------------------------------

/**
 * Sends a `SKIP_WAITING` message to the waiting service worker,
 * causing it to activate immediately. Typically called after the
 * user confirms they want to update.
 *
 * @param registration - The SW registration containing a waiting worker.
 *
 * @example
 * ```ts
 * import { skipWaiting } from './ServiceWorker/register';
 *
 * function handleUpdateAccepted(reg: ServiceWorkerRegistration) {
 *   skipWaiting(reg);
 *   window.location.reload();
 * }
 * ```
 */
export function skipWaiting(registration: ServiceWorkerRegistration): void {
  registration.waiting?.postMessage({ type: 'SKIP_WAITING' });
}

/**
 * Queries the active service worker for its cache version.
 *
 * @returns The cache version string, or `undefined` if no SW is active.
 */
export async function getServiceWorkerVersion(): Promise<string | undefined> {
  if (!navigator.serviceWorker?.controller) {
    return undefined;
  }

  return new Promise<string | undefined>((resolve) => {
    const channel = new MessageChannel();
    channel.port1.onmessage = (event) => {
      resolve(event.data?.version);
    };

    navigator.serviceWorker.controller.postMessage(
      { type: 'GET_VERSION' },
      [channel.port2],
    );

    // Timeout after 2 seconds.
    setTimeout(() => resolve(undefined), 2000);
  });
}
