/**
 * app-main.js — E-Invoice System Core Logic
 * Professional SPA-feel navigation: progress bar, prefetch, instant active state,
 * page entry animation, sidebar toggle, toast notifications, online/offline.
 */

// ─── Progress Bar ─────────────────────────────────────────────────────────────
const NavProgress = (() => {
    let bar, timer, currentWidth = 0;

    function create() {
        if (document.getElementById('nav-progress-bar')) return;
        bar = document.createElement('div');
        bar.id = 'nav-progress-bar';
        Object.assign(bar.style, {
            position: 'fixed', top: '0', left: '0', height: '3px',
            width: '0%', background: 'var(--primary)',
            zIndex: '99999', transition: 'width 0.25s ease, opacity 0.4s ease',
            boxShadow: '0 0 8px var(--primary)', opacity: '1', borderRadius: '0 2px 2px 0'
        });
        document.body.appendChild(bar);
    }

    function start() {
        create();
        clearInterval(timer);
        currentWidth = 0;
        bar.style.opacity = '1';
        bar.style.width = '0%';
        requestAnimationFrame(() => {
            bar.style.width = '30%';
            timer = setInterval(() => {
                if (currentWidth < 85) {
                    currentWidth += (85 - currentWidth) * 0.08;
                    bar.style.width = currentWidth + '%';
                }
            }, 200);
        });
    }

    function finish() {
        if (!bar) return;
        clearInterval(timer);
        bar.style.width = '100%';
        setTimeout(() => { bar.style.opacity = '0'; setTimeout(() => { bar.style.width = '0%'; }, 400); }, 200);
    }

    return { start, finish };
})();

// ─── Prefetch Cache ────────────────────────────────────────────────────────────
const prefetched = new Set();
function prefetchPage(url) {
    if (prefetched.has(url)) return;
    prefetched.add(url);
    const link = document.createElement('link');
    link.rel = 'prefetch';
    link.href = url;
    document.head.appendChild(link);
}

// ─── Active Nav ────────────────────────────────────────────────────────────────
function updateActiveNav(urlPath) {
    const sidebarNav = document.querySelector('.sidebar-nav');
    if (!sidebarNav) return;
    const navItems = Array.from(sidebarNav.querySelectorAll('.nav-item'));
    let bestMatch = null, maxLen = -1;

    navItems.forEach(el => {
        el.classList.remove('active');
        const href = el.getAttribute('href');
        if (!href) return;
        const hrefPath = new URL(href, window.location.origin).pathname.toLowerCase();
        const normUrl = urlPath.replace(/\/index$/, '') || '/';
        const normHref = hrefPath.replace(/\/index$/, '') || '/';
        if (normUrl === normHref || (normUrl.startsWith(normHref + '/') && normHref !== '/')) {
            if (normHref.length > maxLen) { maxLen = normHref.length; bestMatch = el; }
        }
    });
    if (bestMatch) bestMatch.classList.add('active');
}

// ─── Sidebar Nav: Prefetch on hover + instant active + progress bar ────────────
function setupNavLinks() {
    const sidebarNav = document.querySelector('.sidebar-nav');
    if (!sidebarNav) return;

    sidebarNav.querySelectorAll('a.nav-item').forEach(link => {
        const href = link.getAttribute('href');
        if (!href || href.startsWith('#') || href.startsWith('javascript')) return;

        // Prefetch on hover
        link.addEventListener('mouseenter', () => prefetchPage(href), { passive: true });

        // Instant feedback on click
        link.addEventListener('click', (e) => {
            // Don't intercept if modifier key is held
            if (e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;

            // Instantly update active state
            sidebarNav.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));
            link.classList.add('active');

            // Start progress bar
            NavProgress.start();

            // Close sidebar on mobile
            const sb = document.querySelector('.sidebar');
            if (window.innerWidth <= 768 && sb) sb.classList.remove('open');
        });
    });
}

// ─── Page Entry Animation ──────────────────────────────────────────────────────
function animatePageEntry() {
    const main = document.querySelector('.main-content');
    if (!main) return;
    main.style.opacity = '0';
    main.style.transform = 'translateY(8px)';
    main.style.transition = 'opacity 0.28s ease, transform 0.28s ease';
    requestAnimationFrame(() => {
        requestAnimationFrame(() => {
            main.style.opacity = '1';
            main.style.transform = 'translateY(0)';
        });
    });
}

// ─── Click Delegation (sidebar toggle, profile, etc.) ─────────────────────────
document.addEventListener('click', (e) => {
    const sidebarBtn = e.target.closest('#sidebarToggle');
    const closeBtn   = e.target.closest('#sidebarClose');
    const profileBtn = e.target.closest('#profileDropdownToggle');
    const profileMenu = document.getElementById('profileDropdown');
    const sb = document.querySelector('.sidebar');

    if (sidebarBtn) {
        if (window.innerWidth > 768) document.body.classList.toggle('sidebar-collapsed');
        else sb && sb.classList.add('open');
        return;
    }
    if (closeBtn) { sb && sb.classList.remove('open'); return; }
    if (profileBtn && profileMenu) { e.stopPropagation(); profileMenu.classList.toggle('show'); return; }
    if (profileMenu && profileMenu.classList.contains('show') && !profileMenu.contains(e.target)) {
        profileMenu.classList.remove('show');
    }
    if (window.innerWidth <= 768 && sb && sb.classList.contains('open')) {
        if (!sb.contains(e.target) && !e.target.closest('#sidebarToggle')) sb.classList.remove('open');
    }
});

// ─── Toast Notifications ───────────────────────────────────────────────────────
function showToast(title, message, type = 'info') {
    const container = document.getElementById('toast-container');
    if (!container) return;
    const icons = { info: 'ph-info', success: 'ph-check-circle', error: 'ph-warning-circle' };
    const colors = { info: 'var(--primary)', success: '#10b981', error: '#ef4444' };
    const toast = document.createElement('div');
    toast.className = 'toast';
    toast.innerHTML = `
        <div class="toast-icon"><i class="${icons[type] || icons.info}" style="color:${colors[type] || colors.info};"></i></div>
        <div class="toast-content">
            <div class="toast-title">${title}</div>
            <div class="toast-message">${message}</div>
        </div>
        <button class="toast-close" onclick="this.parentElement.remove()"><i class="ph-x"></i></button>
    `;
    container.appendChild(toast);
    requestAnimationFrame(() => toast.classList.add('show'));
    setTimeout(() => { toast.classList.remove('show'); setTimeout(() => toast.remove(), 400); }, 3500);
}

// ─── Online / Offline ─────────────────────────────────────────────────────────
function updateOnlineStatus() {
    if (!navigator.onLine) {
        document.body.classList.add('offline-mode');
        showToast('Connectivity', 'You are offline. Check your connection.', 'error');
    } else {
        document.body.classList.remove('offline-mode');
        if (document.body.classList.contains('was-offline')) {
            showToast('Connectivity', 'Network connection restored.', 'success');
            document.body.classList.remove('was-offline');
        }
    }
}
window.addEventListener('online', updateOnlineStatus);
window.addEventListener('offline', () => { document.body.classList.add('was-offline'); updateOnlineStatus(); });

// ─── Init ──────────────────────────────────────────────────────────────────────
window.addEventListener('DOMContentLoaded', () => {
    NavProgress.finish();          // finish any leftover bar from previous page
    updateActiveNav(window.location.pathname);
    setupNavLinks();
    animatePageEntry();
    updateOnlineStatus();

    // Success toast from server TempData (called from _Layout inline script)
    // (kept as global so _Layout can call it)

    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register('/js/sw.js', { scope: '/' })
            .catch(err => console.warn('SW Register Error:', err));
    }
});

// Expose globally for _Layout.cshtml inline script
window.showToast = showToast;
