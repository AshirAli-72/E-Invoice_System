
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
    '/images/sata-logo.png',
    '/favicon.ico'
];

// ── Install: pre-cache static assets ──────────────────────
self.addEventListener('install', event => {
    self.skipWaiting();
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => {
            // Cache each asset individually — skip on failure so SW still installs
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

// ── Fetch: Cache-first for static assets, Network-first for pages ──
self.addEventListener('fetch', event => {
    const { request } = event;
    const url = new URL(request.url);

    // Only handle same-origin GET requests
    if (request.method !== 'GET' || url.origin !== self.location.origin) return;

    // Skip Blazor SignalR / API calls
    if (url.pathname.startsWith('/_blazor') ||
        url.pathname.startsWith('/api/') ||
        url.pathname.includes('blazor.server.js')) return;

    const isStaticAsset = /\.(css|js|woff2?|ttf|eot|png|jpg|jpeg|gif|svg|ico|webp)$/i.test(url.pathname);

    if (isStaticAsset) {
        // Cache-first: serve from cache immediately, update in background
        event.respondWith(
            caches.open(CACHE_NAME).then(cache =>
                cache.match(request).then(cached => {
                    const networkFetch = fetch(request).then(response => {
                        if (response && response.status === 200) {
                            cache.put(request, response.clone());
                        }
                        return response;
                    }).catch(() => cached); // offline fallback
                    return cached || networkFetch;
                })
            )
        );
    } else {
        // Network-first for HTML pages: try network, fall back to cache
        event.respondWith(
            fetch(request)
                .then(response => {
                    if (response && response.status === 200) {
                        const clone = response.clone();
                        caches.open(CACHE_NAME).then(cache => cache.put(request, clone));
                    }
                    return response;
                })
                .catch(() => caches.match(request))
        );
    }
});

<aside class="sidebar">
    <div class="sidebar-header sidebar-logo-container">
        <img src="/images/sata-logo.png" alt="SATA" class="sidebar-logo" />
        <button id="sidebarClose" class="mobile-only mobile-close-btn">
            <i class="ph-x"></i>
        </button>
    </div>
    <nav class="sidebar-nav">

        <a href="/" class="nav-item">
            <i class="ph-house nav-icon"></i>
            <span>Dashboard</span>
        </a>

        <a href="/customer" class="nav-item">
            <i class="ph-users nav-icon"></i>
            <span>Customers</span>
        </a>

        <a href="/product" class="nav-item">
            <i class="ph-package nav-icon"></i>
            <span>Products/Services</span>
        </a>

        <a href="/sale" class="nav-item">
            <i class="ph-shopping-cart nav-icon"></i>
            <span>Sales</span>
        </a>

        <a href="/invoice" class="nav-item">
            <i class="ph-file-text nav-icon"></i>
            <span>Invoices</span>
        </a>

        <a href="/invoice/create" class="nav-item">
            <i class="ph-file-plus nav-icon"></i>
            <span>Create Invoice</span>
        </a>

        <a href="/seller" class="nav-item">
            <i class="ph-storefront nav-icon"></i>
            <span>Seller Profile</span>
        </a>

    </nav>
</aside>