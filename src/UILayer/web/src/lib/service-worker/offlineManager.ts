/**
 * @fileoverview Offline state management for Cognitive Mesh.
 *
 * Detects online/offline transitions, queues failed API requests
 * for replay when connectivity returns, shows an offline banner,
 * and synchronises queued telemetry events on reconnect.
 */

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

/** Represents a single API request that failed while offline. */
export interface QueuedRequest {
  /** Unique identifier for the queued entry. */
  id: string;
  /** The request URL. */
  url: string;
  /** HTTP method. */
  method: string;
  /** Serialised request headers. */
  headers: Record<string, string>;
  /** Serialised request body (stringified JSON or raw text). */
  body: string | null;
  /** Timestamp (ms since epoch) when the request was queued. */
  queuedAt: number;
  /** Number of replay attempts already made. */
  retryCount: number;
}

/** Callback invoked whenever the online/offline state changes. */
export type ConnectivityChangeCallback = (online: boolean) => void;

/** Callback invoked when the offline banner visibility should change. */
export type BannerVisibilityCallback = (visible: boolean) => void;

/** Configuration for the {@link OfflineManager}. */
export interface OfflineManagerConfig {
  /** Maximum number of requests to keep in the offline queue. Default: 100 */
  maxQueueSize?: number;
  /** Maximum number of retry attempts per queued request. Default: 3 */
  maxRetries?: number;
  /** Delay (ms) between replay attempts during a sync cycle. Default: 500 */
  replayDelayMs?: number;
  /** localStorage key for the request queue. Default: 'cognitivemesh_offline_queue' */
  storageKey?: string;
}

// ---------------------------------------------------------------------------
// Constants
// ---------------------------------------------------------------------------

const DEFAULT_MAX_QUEUE_SIZE = 100;
const DEFAULT_MAX_RETRIES = 3;
const DEFAULT_REPLAY_DELAY_MS = 500;
const DEFAULT_STORAGE_KEY = 'cognitivemesh_offline_queue';

/** Background Sync tag registered with the service worker. */
const TELEMETRY_SYNC_TAG = 'sync-telemetry';

// ---------------------------------------------------------------------------
// OfflineManager
// ---------------------------------------------------------------------------

/**
 * Manages the application's offline state.
 *
 * Responsibilities:
 * 1. Listen for `online` / `offline` browser events
 * 2. Maintain a queue of failed API requests in `localStorage`
 * 3. Replay queued requests when connectivity is restored
 * 4. Notify UI components to show/hide an offline banner
 * 5. Trigger Background Sync for queued telemetry events
 *
 * @example
 * ```ts
 * import { OfflineManager } from './ServiceWorker/offlineManager';
 *
 * const offlineManager = new OfflineManager();
 *
 * offlineManager.onConnectivityChange((online) => {
 *   console.log(online ? 'Back online!' : 'Went offline');
 * });
 *
 * offlineManager.onBannerVisibilityChange((visible) => {
 *   setShowOfflineBanner(visible);
 * });
 *
 * offlineManager.start();
 * ```
 */
export class OfflineManager {
  // ---- internal state ----------------------------------------------------

  private _isOnline: boolean;
  private _connectivityListeners: Set<ConnectivityChangeCallback> = new Set();
  private _bannerListeners: Set<BannerVisibilityCallback> = new Set();
  private _config: Required<OfflineManagerConfig>;
  private _started = false;

  // Bound references so we can remove the listeners in `stop()`.
  private _handleOnline: () => void;
  private _handleOffline: () => void;

  // ---- constructor -------------------------------------------------------

  constructor(config: OfflineManagerConfig = {}) {
    this._config = {
      maxQueueSize: config.maxQueueSize ?? DEFAULT_MAX_QUEUE_SIZE,
      maxRetries: config.maxRetries ?? DEFAULT_MAX_RETRIES,
      replayDelayMs: config.replayDelayMs ?? DEFAULT_REPLAY_DELAY_MS,
      storageKey: config.storageKey ?? DEFAULT_STORAGE_KEY,
    };

    this._isOnline = typeof navigator !== 'undefined' ? navigator.onLine : true;

    this._handleOnline = this._onOnline.bind(this);
    this._handleOffline = this._onOffline.bind(this);
  }

  // ---- public API --------------------------------------------------------

  /** Whether the browser currently reports connectivity. */
  get isOnline(): boolean {
    return this._isOnline;
  }

  /**
   * Starts listening for connectivity changes.
   * Idempotent -- calling `start()` multiple times has no additional effect.
   */
  start(): void {
    if (this._started || typeof window === 'undefined') {
      return;
    }

    window.addEventListener('online', this._handleOnline);
    window.addEventListener('offline', this._handleOffline);
    this._started = true;

    // Emit initial state.
    this._isOnline = navigator.onLine;
    this._notifyBannerListeners(!this._isOnline);
  }

  /**
   * Stops listening for connectivity changes and clears all callbacks.
   */
  stop(): void {
    if (!this._started || typeof window === 'undefined') {
      return;
    }

    window.removeEventListener('online', this._handleOnline);
    window.removeEventListener('offline', this._handleOffline);
    this._started = false;
  }

  /**
   * Registers a callback that fires whenever connectivity changes.
   *
   * @param callback - Receives `true` when online, `false` when offline.
   * @returns A function to unsubscribe.
   */
  onConnectivityChange(callback: ConnectivityChangeCallback): () => void {
    this._connectivityListeners.add(callback);
    return () => {
      this._connectivityListeners.delete(callback);
    };
  }

  /**
   * Registers a callback that fires when the offline banner should
   * be shown or hidden.
   *
   * @param callback - Receives `true` to show, `false` to hide.
   * @returns A function to unsubscribe.
   */
  onBannerVisibilityChange(callback: BannerVisibilityCallback): () => void {
    this._bannerListeners.add(callback);
    return () => {
      this._bannerListeners.delete(callback);
    };
  }

  /**
   * Queues a failed API request for replay when connectivity returns.
   *
   * @param request - The `Request` object (or a plain descriptor) that failed.
   */
  async queueFailedRequest(request: Request | QueuedRequest): Promise<void> {
    const queue = this._loadQueue();

    let entry: QueuedRequest;

    if (request instanceof Request) {
      const body = await request.clone().text();
      entry = {
        id: `${Date.now()}-${Math.random().toString(36).slice(2, 9)}`,
        url: request.url,
        method: request.method,
        headers: Object.fromEntries(request.headers.entries()),
        body: body || null,
        queuedAt: Date.now(),
        retryCount: 0,
      };
    } else {
      entry = { ...request };
    }

    // Enforce maximum queue size (FIFO eviction).
    if (queue.length >= this._config.maxQueueSize) {
      queue.shift();
    }

    queue.push(entry);
    this._saveQueue(queue);
  }

  /**
   * Returns all currently queued requests.
   */
  getQueuedRequests(): QueuedRequest[] {
    return this._loadQueue();
  }

  /**
   * Returns the number of queued requests.
   */
  get queueLength(): number {
    return this._loadQueue().length;
  }

  /**
   * Replays all queued requests. Successful requests are removed
   * from the queue; failed ones remain for the next attempt.
   *
   * @returns The number of successfully replayed requests.
   */
  async replayQueue(): Promise<number> {
    if (!this._isOnline) {
      console.warn('[OfflineManager] Cannot replay queue while offline.');
      return 0;
    }

    const queue = this._loadQueue();
    if (queue.length === 0) {
      return 0;
    }

    let successCount = 0;
    const remaining: QueuedRequest[] = [];

    for (const entry of queue) {
      try {
        const response = await fetch(entry.url, {
          method: entry.method,
          headers: entry.headers,
          body: entry.body,
        });

        if (response.ok) {
          successCount += 1;
        } else {
          entry.retryCount += 1;
          if (entry.retryCount < this._config.maxRetries) {
            remaining.push(entry);
          } else {
            console.warn(`[OfflineManager] Dropping request after ${entry.retryCount} retries: ${entry.url}`);
          }
        }
      } catch {
        entry.retryCount += 1;
        if (entry.retryCount < this._config.maxRetries) {
          remaining.push(entry);
        }
      }

      // Small delay between requests to avoid thundering herd.
      if (this._config.replayDelayMs > 0) {
        await this._delay(this._config.replayDelayMs);
      }
    }

    this._saveQueue(remaining);
    console.log(`[OfflineManager] Replayed ${successCount}/${queue.length} queued requests.`);
    return successCount;
  }

  /**
   * Clears the offline request queue entirely.
   */
  clearQueue(): void {
    this._saveQueue([]);
  }

  /**
   * Requests a Background Sync for telemetry events via the
   * Service Worker registration.
   */
  async requestTelemetrySync(): Promise<void> {
    if (!('serviceWorker' in navigator)) {
      return;
    }

    try {
      const registration = await navigator.serviceWorker.ready;
      if ('sync' in registration) {
        await (registration as any).sync.register(TELEMETRY_SYNC_TAG);
        console.log('[OfflineManager] Background sync registered for telemetry.');
      }
    } catch (error) {
      console.warn('[OfflineManager] Background Sync not available:', error);
      // Fall back to immediate replay.
      await this.replayQueue();
    }
  }

  // ---- private helpers ---------------------------------------------------

  private _onOnline(): void {
    this._isOnline = true;
    this._notifyConnectivityListeners(true);
    this._notifyBannerListeners(false);

    // Replay queued requests now that we are back online.
    this.replayQueue().catch((err) =>
      console.error('[OfflineManager] Replay failed:', err),
    );

    // Also trigger Background Sync for telemetry.
    this.requestTelemetrySync().catch(() => {});
  }

  private _onOffline(): void {
    this._isOnline = false;
    this._notifyConnectivityListeners(false);
    this._notifyBannerListeners(true);
  }

  private _notifyConnectivityListeners(online: boolean): void {
    this._connectivityListeners.forEach((cb) => {
      try {
        cb(online);
      } catch (err) {
        console.error('[OfflineManager] Connectivity listener error:', err);
      }
    });
  }

  private _notifyBannerListeners(visible: boolean): void {
    this._bannerListeners.forEach((cb) => {
      try {
        cb(visible);
      } catch (err) {
        console.error('[OfflineManager] Banner listener error:', err);
      }
    });
  }

  private _loadQueue(): QueuedRequest[] {
    try {
      const raw = localStorage.getItem(this._config.storageKey);
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }

  private _saveQueue(queue: QueuedRequest[]): void {
    try {
      localStorage.setItem(this._config.storageKey, JSON.stringify(queue));
    } catch {
      // localStorage may be full or unavailable.
      console.warn('[OfflineManager] Failed to persist request queue.');
    }
  }

  private _delay(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
}

// ---------------------------------------------------------------------------
// Singleton instance
// ---------------------------------------------------------------------------

/**
 * Pre-configured singleton instance of {@link OfflineManager}.
 *
 * Import and call `.start()` in your application entry point:
 *
 * ```ts
 * import { offlineManager } from './ServiceWorker/offlineManager';
 * offlineManager.start();
 * ```
 */
export const offlineManager = new OfflineManager();

export default OfflineManager;
