/**
 * @fileoverview Service Worker for the Cognitive Mesh UI.
 *
 * Implements a multi-strategy caching approach:
 * - **Cache-first** for static widget bundles and assets
 * - **Network-first** for API calls with offline fallback
 * - Cache versioning with automatic cleanup of stale caches
 * - Background sync for offline telemetry events
 *
 * Named caches:
 * - `widget-cache-v1`  -- Widget JavaScript bundles and CSS
 * - `api-cache-v1`     -- API responses (dashboard layouts, widget definitions)
 * - `static-cache-v1`  -- Static assets (images, fonts, HTML shell)
 */

// ---------------------------------------------------------------------------
// TypeScript declarations for the Service Worker global scope
// ---------------------------------------------------------------------------

declare const self: ServiceWorkerGlobalScope;

// ---------------------------------------------------------------------------
// Cache configuration
// ---------------------------------------------------------------------------

/** Current cache version. Increment to bust all caches on the next SW activation. */
const CACHE_VERSION = 'v1';

/** Cache name for widget JavaScript bundles and CSS. */
const WIDGET_CACHE = `widget-cache-${CACHE_VERSION}`;

/** Cache name for API responses. */
const API_CACHE = `api-cache-${CACHE_VERSION}`;

/** Cache name for static assets (images, fonts, HTML shell). */
const STATIC_CACHE = `static-cache-${CACHE_VERSION}`;

/** Set of all cache names managed by this service worker version. */
const CURRENT_CACHES = new Set([WIDGET_CACHE, API_CACHE, STATIC_CACHE]);

/** Tag used for the Background Sync API to replay telemetry events. */
const TELEMETRY_SYNC_TAG = 'sync-telemetry';

/** Maximum age (in milliseconds) for cached API responses before they are considered stale. */
const API_CACHE_MAX_AGE_MS = 5 * 60 * 1000; // 5 minutes

/** Static assets to pre-cache during the `install` event. */
const PRECACHE_URLS: string[] = [
  '/',
  '/index.html',
  '/manifest.json',
];

// ---------------------------------------------------------------------------
// URL classification helpers
// ---------------------------------------------------------------------------

/**
 * Returns `true` when the request URL points to a widget bundle.
 * Widget bundles are identified by their path or file extension.
 */
function isWidgetBundle(url: URL): boolean {
  return (
    url.pathname.startsWith('/widgets/') ||
    url.pathname.includes('/widget-') ||
    (url.pathname.endsWith('.js') && url.pathname.includes('widget'))
  );
}

/**
 * Returns `true` when the request URL is an API call.
 */
function isApiRequest(url: URL): boolean {
  return url.pathname.startsWith('/api/');
}

/**
 * Returns `true` when the request URL is a static asset
 * (image, font, CSS, or the HTML shell).
 */
function isStaticAsset(url: URL): boolean {
  const staticExtensions = ['.css', '.png', '.jpg', '.jpeg', '.gif', '.svg', '.woff', '.woff2', '.ttf', '.ico'];
  return (
    staticExtensions.some((ext) => url.pathname.endsWith(ext)) ||
    url.pathname === '/' ||
    url.pathname === '/index.html'
  );
}

/**
 * Returns `true` when the request is a telemetry event submission.
 */
function isTelemetryRequest(url: URL): boolean {
  return url.pathname.startsWith('/api/telemetry') || url.pathname.includes('/events');
}

/**
 * Returns `true` when the request fetches a dashboard layout definition.
 */
function isDashboardLayout(url: URL): boolean {
  return url.pathname.includes('/dashboard') && url.pathname.includes('/layout');
}

/**
 * Returns `true` when the request fetches a widget definition.
 */
function isWidgetDefinition(url: URL): boolean {
  return url.pathname.includes('/widget-definitions') || url.pathname.includes('/widgets/config');
}

// ---------------------------------------------------------------------------
// Caching strategies
// ---------------------------------------------------------------------------

/**
 * **Cache-first** strategy.
 *
 * Serves the response from the cache if available. Falls back to
 * the network and caches the response for future requests.
 *
 * Best for resources that change infrequently (widget bundles, static assets).
 */
async function cacheFirst(request: Request, cacheName: string): Promise<Response> {
  const cache = await caches.open(cacheName);
  const cachedResponse = await cache.match(request);

  if (cachedResponse) {
    return cachedResponse;
  }

  try {
    const networkResponse = await fetch(request);
    // Only cache successful responses.
    if (networkResponse.ok) {
      // Clone the response because it can only be consumed once.
      cache.put(request, networkResponse.clone());
    }
    return networkResponse;
  } catch (error) {
    // If both cache and network fail, return a basic offline response.
    return new Response(
      JSON.stringify({ error: 'offline', message: 'This resource is not available offline.' }),
      { status: 503, headers: { 'Content-Type': 'application/json' } },
    );
  }
}

/**
 * **Network-first** strategy with offline fallback.
 *
 * Attempts to fetch from the network. On success, caches the
 * response. On failure, serves the last cached copy.
 *
 * Best for API calls where freshness matters but offline
 * availability is still desired.
 */
async function networkFirst(request: Request, cacheName: string): Promise<Response> {
  const cache = await caches.open(cacheName);

  try {
    const networkResponse = await fetch(request);

    if (networkResponse.ok) {
      // Store with a timestamp header so we can check staleness later.
      const responseToCache = networkResponse.clone();
      const headers = new Headers(responseToCache.headers);
      headers.set('sw-cached-at', Date.now().toString());

      const timestampedResponse = new Response(await responseToCache.blob(), {
        status: responseToCache.status,
        statusText: responseToCache.statusText,
        headers,
      });

      cache.put(request, timestampedResponse);
    }

    return networkResponse;
  } catch (error) {
    // Network failed -- try the cache.
    const cachedResponse = await cache.match(request);

    if (cachedResponse) {
      // Optionally check if the cached response is too stale.
      const cachedAt = cachedResponse.headers.get('sw-cached-at');
      if (cachedAt) {
        const age = Date.now() - parseInt(cachedAt, 10);
        if (age > API_CACHE_MAX_AGE_MS) {
          console.warn(`[SW] Serving stale cached response for ${request.url} (age: ${age}ms)`);
        }
      }
      return cachedResponse;
    }

    // Nothing in cache either -- return an offline error.
    return new Response(
      JSON.stringify({
        error: 'offline',
        message: 'You appear to be offline and no cached data is available.',
      }),
      { status: 503, headers: { 'Content-Type': 'application/json' } },
    );
  }
}

// ---------------------------------------------------------------------------
// Telemetry background sync
// ---------------------------------------------------------------------------

/**
 * Stores a failed telemetry request in IndexedDB for later replay
 * via the Background Sync API.
 */
async function queueTelemetryForSync(request: Request): Promise<void> {
  try {
    const body = await request.clone().text();
    const db = await openTelemetryDB();
    const tx = db.transaction('outbox', 'readwrite');
    const store = tx.objectStore('outbox');
    store.add({
      url: request.url,
      method: request.method,
      headers: Object.fromEntries(request.headers.entries()),
      body,
      timestamp: Date.now(),
    });
    await new Promise<void>((resolve, reject) => {
      tx.oncomplete = () => resolve();
      tx.onerror = () => reject(tx.error);
    });
  } catch (err) {
    console.error('[SW] Failed to queue telemetry event:', err);
  }
}

/**
 * Replays all queued telemetry events from IndexedDB.
 */
async function replayQueuedTelemetry(): Promise<void> {
  try {
    const db = await openTelemetryDB();
    const tx = db.transaction('outbox', 'readwrite');
    const store = tx.objectStore('outbox');

    const allEvents = await new Promise<any[]>((resolve, reject) => {
      const req = store.getAll();
      req.onsuccess = () => resolve(req.result);
      req.onerror = () => reject(req.error);
    });

    for (const event of allEvents) {
      try {
        const response = await fetch(event.url, {
          method: event.method,
          headers: event.headers,
          body: event.body,
        });

        if (response.ok) {
          // Remove successfully replayed events.
          const deleteTx = db.transaction('outbox', 'readwrite');
          deleteTx.objectStore('outbox').delete(event.id);
          await new Promise<void>((resolve) => {
            deleteTx.oncomplete = () => resolve();
          });
        }
      } catch {
        // Event will remain in the queue for the next sync.
        console.warn('[SW] Failed to replay telemetry event, will retry later.');
      }
    }
  } catch (err) {
    console.error('[SW] Failed to replay queued telemetry:', err);
  }
}

/**
 * Opens (or creates) the IndexedDB database used to store
 * telemetry events that failed to send.
 */
function openTelemetryDB(): Promise<IDBDatabase> {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open('cognitivemesh-telemetry', 1);

    request.onupgradeneeded = () => {
      const db = request.result;
      if (!db.objectStoreNames.contains('outbox')) {
        db.createObjectStore('outbox', { keyPath: 'id', autoIncrement: true });
      }
    };

    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });
}

// ---------------------------------------------------------------------------
// Service Worker lifecycle events
// ---------------------------------------------------------------------------

/**
 * **Install** event.
 *
 * Pre-caches critical static assets so the app shell is available
 * immediately on the next visit, even if the user is offline.
 */
self.addEventListener('install', (event: ExtendableEvent) => {
  console.log('[SW] Installing service worker');

  event.waitUntil(
    (async () => {
      const cache = await caches.open(STATIC_CACHE);
      console.log('[SW] Pre-caching static assets');
      await cache.addAll(PRECACHE_URLS);

      // Skip waiting so the new SW activates immediately.
      await self.skipWaiting();
    })(),
  );
});

/**
 * **Activate** event.
 *
 * Cleans up stale caches from previous service worker versions.
 */
self.addEventListener('activate', (event: ExtendableEvent) => {
  console.log('[SW] Activating service worker');

  event.waitUntil(
    (async () => {
      // Delete caches that are no longer in the current set.
      const cacheNames = await caches.keys();
      await Promise.all(
        cacheNames
          .filter((name) => !CURRENT_CACHES.has(name))
          .map((name) => {
            console.log(`[SW] Deleting stale cache: ${name}`);
            return caches.delete(name);
          }),
      );

      // Immediately claim all clients so the new SW controls pages
      // without requiring a reload.
      await self.clients.claim();
    })(),
  );
});

/**
 * **Fetch** event.
 *
 * Routes each request to the appropriate caching strategy based
 * on the URL pattern.
 */
self.addEventListener('fetch', (event: FetchEvent) => {
  const url = new URL(event.request.url);

  // Only handle same-origin requests.
  if (url.origin !== self.location.origin) {
    return;
  }

  // Telemetry requests: attempt to send, queue for sync on failure.
  if (isTelemetryRequest(url)) {
    event.respondWith(
      fetch(event.request.clone()).catch(async () => {
        await queueTelemetryForSync(event.request);
        return new Response(
          JSON.stringify({ queued: true, message: 'Telemetry event queued for background sync.' }),
          { status: 202, headers: { 'Content-Type': 'application/json' } },
        );
      }),
    );
    return;
  }

  // Widget bundles: cache-first.
  if (isWidgetBundle(url)) {
    event.respondWith(cacheFirst(event.request, WIDGET_CACHE));
    return;
  }

  // Dashboard layouts and widget definitions: network-first (they
  // change occasionally but should be available offline).
  if (isDashboardLayout(url) || isWidgetDefinition(url)) {
    event.respondWith(networkFirst(event.request, API_CACHE));
    return;
  }

  // General API requests: network-first.
  if (isApiRequest(url)) {
    event.respondWith(networkFirst(event.request, API_CACHE));
    return;
  }

  // Static assets: cache-first.
  if (isStaticAsset(url)) {
    event.respondWith(cacheFirst(event.request, STATIC_CACHE));
    return;
  }

  // All other requests: let the browser handle normally.
});

/**
 * **Sync** event (Background Sync API).
 *
 * Fired when the browser regains connectivity, allowing us to
 * replay queued telemetry events.
 */
self.addEventListener('sync', (event: SyncEvent) => {
  if (event.tag === TELEMETRY_SYNC_TAG) {
    console.log('[SW] Background sync: replaying queued telemetry');
    event.waitUntil(replayQueuedTelemetry());
  }
});

/**
 * **Message** event.
 *
 * Handles messages from the main thread, such as explicit cache
 * invalidation requests or version queries.
 */
self.addEventListener('message', (event: ExtendableMessageEvent) => {
  const { type, payload } = event.data ?? {};

  switch (type) {
    case 'SKIP_WAITING':
      self.skipWaiting();
      break;

    case 'GET_VERSION':
      event.ports?.[0]?.postMessage({ version: CACHE_VERSION });
      break;

    case 'CLEAR_CACHE': {
      const cacheName = payload?.cacheName;
      if (cacheName && CURRENT_CACHES.has(cacheName)) {
        caches.delete(cacheName).then(() => {
          event.ports?.[0]?.postMessage({ cleared: true, cacheName });
        });
      }
      break;
    }

    case 'CACHE_URLS': {
      const urls: string[] = payload?.urls ?? [];
      const targetCache: string = payload?.cacheName ?? STATIC_CACHE;
      caches.open(targetCache).then((cache) => cache.addAll(urls));
      break;
    }

    default:
      break;
  }
});

// Ensure TypeScript treats this file as a module.
export {};
