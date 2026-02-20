/**
 * @fileoverview Public API for the Cognitive Mesh Service Worker module.
 *
 * Re-exports registration utilities, the offline manager, and types
 * so consumers can import from a single entry point:
 *
 * ```ts
 * import { registerServiceWorker, offlineManager } from '../ServiceWorker';
 * ```
 *
 * Note: The service worker implementation (`sw.ts`) runs in its own
 * global scope and is NOT imported here. It must be compiled
 * separately and served as `/sw.js`.
 */

// Registration
export {
  registerServiceWorker,
  unregisterServiceWorker,
  skipWaiting,
  getServiceWorkerVersion,
} from './register';
export type {
  ServiceWorkerConfig,
  OnUpdateCallback,
  OnSuccessCallback,
} from './register';

// Offline manager
export {
  OfflineManager,
  offlineManager,
} from './offlineManager';
export type {
  QueuedRequest,
  ConnectivityChangeCallback,
  BannerVisibilityCallback,
  OfflineManagerConfig,
} from './offlineManager';
