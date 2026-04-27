
// ── E-Invoice System Service Worker v3 ─────────────────────────────────────
// Changes: login page cached, stale-while-revalidate for HTML,
//          smarter offline fallback, bumped cache name.

const CACHE_NAME = 'sata-pos-v3';

const STATIC_ASSETS = [
    '/',
    '/Account/Login',
    '/css/dashboard.css',
    '/css/customer.css',
    '/css/invoice.css',
    '/css/sale.css',
    '/css/product.css',
    '/css/settings.css',
    '/css/login.css',
    '/lib/inter-font/inter.css',
    '/lib/phosphor/icons.css',
    '/js/site.js',
    '/js/app-main.js',
    '/images/sata-logo.png',
    '/favicon.ico'
];

// ── Install: pre-cache static assets ───────────────────────────────────────
self.addEventListener('install', event => {
    self.skipWaiting();
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache =>
            Promise.allSettled(
                STATIC_ASSETS.map(url =>
                    cache.add(url).catch(() => { /* asset may not exist yet */ })
                )
            )
        )
    );
});

// ── Activate: delete old caches ─────────────────────────────────────────────
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys()
            .then(keys => Promise.all(
                keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k))
            ))
            .then(() => self.clients.claim())
    );
});

// ── Helpers ──────────────────────────────────────────────────────────────────
const isStaticAsset = url =>
    /\.(css|js|woff2?|ttf|eot|png|jpg|jpeg|gif|svg|ico|webp)$/i.test(new URL(url).pathname);

const isSkipped = url => {
    const p = new URL(url).pathname;
    return p.startsWith('/_blazor') ||
           p.startsWith('/api/') ||
           p.includes('blazor.server.js') ||
           p.includes('__');
};

// ── Fetch Strategy ───────────────────────────────────────────────────────────
self.addEventListener('fetch', event => {
    const { request } = event;
    const url = request.url;

    // Only handle same-origin GETs
    if (request.method !== 'GET' ||
        new URL(url).origin !== self.location.origin ||
        isSkipped(url)) return;

    if (isStaticAsset(url)) {
        // ── Static assets: Cache-First (serve instantly, revalidate in background) ──
        event.respondWith(
            caches.open(CACHE_NAME).then(cache =>
                cache.match(request).then(cached => {
                    // Revalidate in background regardless
                    const networkFetch = fetch(request)
                        .then(response => {
                            if (response && response.status === 200)
                                cache.put(request, response.clone());
                            return response;
                        })
                        .catch(() => null);
                    return cached || networkFetch;
                })
            )
        );
    } else {
        // ── HTML pages: Network-First with 1.5s timeout, then cache fallback ──
        event.respondWith(
            caches.open(CACHE_NAME).then(cache =>
                new Promise(resolve => {
                    let settled = false;

                    // Race: 1.5-second timeout before we serve cache
                    const timer = setTimeout(() => {
                        if (!settled) {
                            cache.match(request).then(cached => {
                                if (cached) { settled = true; resolve(cached); }
                            });
                        }
                    }, 1500);

                    fetch(request)
                        .then(response => {
                            clearTimeout(timer);
                            if (!settled) {
                                settled = true;
                                if (response && response.status === 200)
                                    cache.put(request, response.clone());
                                resolve(response);
                            }
                        })
                        .catch(async () => {
                            clearTimeout(timer);
                            if (!settled) {
                                settled = true;
                                const cached = await cache.match(request) ||
                                               await cache.match('/Account/Login');
                                resolve(cached || new Response(
                                    `<!DOCTYPE html><html><head>
                                    <meta charset="utf-8">
                                    <meta name="viewport" content="width=device-width,initial-scale=1">
                                    <title>Offline - E-Invoice</title>
                                    <style>
                                        *{margin:0;padding:0;box-sizing:border-box}
                                        body{font-family:system-ui,sans-serif;background:#0f172a;color:#e2e8f0;
                                             display:flex;align-items:center;justify-content:center;
                                             min-height:100vh;text-align:center;padding:2rem}
                                        .card{background:#1e293b;border:1px solid #334155;border-radius:16px;
                                              padding:2.5rem;max-width:400px;width:100%}
                                        h2{font-size:1.5rem;font-weight:800;margin-bottom:.5rem;color:#f8fafc}
                                        p{color:#94a3b8;margin-bottom:1.5rem;font-size:.9rem}
                                        button{background:#3b82f6;color:#fff;border:none;border-radius:8px;
                                               padding:.75rem 1.5rem;font-size:.95rem;font-weight:700;
                                               cursor:pointer;width:100%}
                                        button:hover{background:#2563eb}
                                    </style></head>
                                    <body><div class="card">
                                        <h2>📡 You are Offline</h2>
                                        <p>This page isn't cached yet. Check your connection and try again.</p>
                                        <button onclick="window.location.reload()">Retry Connection</button>
                                    </div></body></html>`,
                                    { headers: { 'Content-Type': 'text/html' } }
                                ));
                            }
                        });
                })
            )
        );
    }
});