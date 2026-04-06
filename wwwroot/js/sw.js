
const CACHE_NAME = 'sata-pos-v2';

const STATIC_ASSETS = [
    '/',
    '/css/dashboard.css',
    '/css/app-base.css',
    '/css/customer.css',
    '/css/invoice.css',
    '/css/sale.css',
    '/css/product.css',
    '/css/login.css',
    '/lib/inter-font/inter.css',
    '/lib/phosphor/icons.css',
    '/js/site.js',
    '/js/app-main.js',
    '/images/sata-logo.png',
    '/favicon.ico'
];

// ── Install: pre-cache static assets ──────────────────────
self.addEventListener('install', event => {
    self.skipWaiting();
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => {
            return Promise.allSettled(
                STATIC_ASSETS.map(url =>
                    cache.add(url).catch(() => { /* asset might not exist yet */ })
                )
            );
        })
    );
});

// ── Activate: clean old caches ────────────────────────────
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(
                keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k))
            )
        ).then(() => self.clients.claim())
    );
});

// ── Fetch Strategy ──────────────────────────────────────────
self.addEventListener('fetch', event => {
    const { request } = event;
    const url = new URL(request.url);

    // Only handle same-origin GET requests
    if (request.method !== 'GET' || url.origin !== self.location.origin) return;

    // Skip SignalR / API
    if (url.pathname.startsWith('/_blazor') ||
        url.pathname.startsWith('/api/') ||
        url.pathname.includes('blazor.server.js')) return;

    const isStaticAsset = /\.(css|js|woff2?|ttf|eot|png|jpg|jpeg|gif|svg|ico|webp)$/i.test(url.pathname);

    if (isStaticAsset) {
        // Cache-first for assets
        event.respondWith(
            caches.match(request).then(cached => {
                const networkFetch = fetch(request).then(response => {
                    if (response && response.status === 200) {
                        caches.open(CACHE_NAME).then(cache => cache.put(request, response.clone()));
                    }
                    return response;
                }).catch(() => cached);
                return cached || networkFetch;
            })
        );
    } else {
        // Fast-Fallback for HTML: 2-second limit before cache
        event.respondWith(
            new Promise((resolve) => {
                const timeoutId = setTimeout(() => {
                    caches.match(request).then(cached => {
                        if (cached) resolve(cached);
                    });
                }, 2000);

                fetch(request)
                    .then(response => {
                        clearTimeout(timeoutId);
                        if (response && response.status === 200) {
                            const clone = response.clone();
                            caches.open(CACHE_NAME).then(cache => cache.put(request, clone));
                        }
                        resolve(response);
                    })
                    .catch(() => {
                        clearTimeout(timeoutId);
                        caches.match(request).then(cached => resolve(cached));
                    });
            })
        );
    }
});