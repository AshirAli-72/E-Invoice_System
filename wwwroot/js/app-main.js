/**
 * app-main.js - E-Invoice System Core Logic
 * Handles sidebar, navigation, online/offline status, and toast notifications.
 */

// --- Global State ---
let prefetchCache = {};
const sidebar = document.querySelector('.sidebar');
const globalLoader = document.getElementById('global-loader');
const mainContent = document.querySelector('.main-content');

/**
 * Updates the active sidebar item based on current URL
 */
function updateActiveNav(urlPath) {
    const sidebarNav = document.querySelector('.sidebar-nav');
    if (!sidebarNav) return;
    const navItems = Array.from(sidebarNav.querySelectorAll('.nav-item'));
    let bestMatch = null;
    let maxLen = -1;

    navItems.forEach(el => {
        const href = el.getAttribute('href');
        if (!href) return;
        const hrefPath = new URL(href, window.location.origin).pathname.toLowerCase();
        const normUrl = urlPath.replace(/\/index$/, '') || '/';
        const normHref = hrefPath.replace(/\/index$/, '') || '/';

        if (normUrl === normHref || (normUrl.startsWith(normHref + '/') && normHref !== '/')) {
            if (normHref.length > maxLen) {
                maxLen = normHref.length;
                bestMatch = el;
            }
        }
        el.classList.remove('active');
    });
    if (bestMatch) bestMatch.classList.add('active');
}

/**
 * Global UI Interactions (Click Delegation)
 */
document.addEventListener('click', (e) => {
    const link = e.target.closest('a');

    // Intercept internal links to prevent full reloads
    if (link && link.href && link.origin === window.location.origin && !link.hasAttribute('download') && link.target !== '_blank') {
        const url = new URL(link.href);
        // Avoid intercepting fragments or SignalR
        if (url.pathname !== window.location.pathname || url.search !== window.location.search) {
            e.preventDefault();
            softNavigate(link.href);
            return;
        }
    }

    const sidebarBtn = e.target.closest('#sidebarToggle');
    // ... (rest of the click handler)
    const closeBtn = e.target.closest('#sidebarClose');
    const profileBtn = e.target.closest('#profileDropdownToggle');
    const profileMenu = document.getElementById('profileDropdown');

    // Sidebar Toggle
    if (sidebarBtn) {
        if (window.innerWidth > 768) {
            document.body.classList.toggle('sidebar-collapsed');
        } else {
            sidebar && sidebar.classList.add('open');
        }
        return;
    }

    // Sidebar Close
    if (closeBtn) {
        sidebar && sidebar.classList.remove('open');
        return;
    }

    // Profile Toggle
    if (profileBtn && profileMenu) {
        e.stopPropagation();
        profileMenu.classList.toggle('show');
        return;
    }

    // Auto-close profile dropdown
    if (profileMenu && profileMenu.classList.contains('show')) {
        if (!profileMenu.contains(e.target)) {
            profileMenu.classList.remove('show');
        }
    }

    // Auto-close sidebar on mobile
    if (window.innerWidth <= 768 && sidebar && sidebar.classList.contains('open')) {
        if (!sidebar.contains(e.target) && !sidebarBtn) {
            sidebar.classList.remove('open');
        }
    }
});

/**
 * SPA Navigation (softNavigate)
 * Optimized for speed and "instant" feel.
 */
async function softNavigate(url, push = true) {
    if (!mainContent || !globalLoader) { window.location.href = url; return; }

    try {
        // UI Feedback: Instant sidebar update & Close mobile menu
        const urlPath = new URL(url, window.location.href).pathname.toLowerCase();
        updateActiveNav(urlPath);
        if (window.innerWidth <= 768 && sidebar) sidebar.classList.remove('open');

        // Loader Progress
        globalLoader.style.width = '20%'; // Start lower for immediate feedback
        globalLoader.style.opacity = '1';

        let response;
        if (prefetchCache[url]) {
            response = await prefetchCache[url];
            delete prefetchCache[url];
        } else {
            response = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        }

        globalLoader.style.width = '60%';
        if (!response.ok) throw new Error(`Server returned ${response.status}`);

        const html = await response.text();
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const fetchedMain = doc.querySelector('.main-content');

        if (!fetchedMain) {
            console.warn("Soft navigation target missing .main-content, full reload triggered.");
            window.location.href = url;
            return;
        }

        const newTitle = doc.querySelector('title');
        if (newTitle) document.title = newTitle.innerText;

        // "Instant" feel replacing
        mainContent.innerHTML = fetchedMain.innerHTML;

        // Re-execute scripts in the new content
        const scripts = Array.from(mainContent.querySelectorAll('script'));
        scripts.forEach(oldScript => {
            const newScript = document.createElement('script');
            Array.from(oldScript.attributes).forEach(attr => newScript.setAttribute(attr.name, attr.value));
            newScript.textContent = oldScript.textContent;
            oldScript.parentNode.replaceChild(newScript, oldScript);
        });

        // Trigger lifecycle events
        document.dispatchEvent(new Event('DOMContentLoaded'));
        window.dispatchEvent(new Event('load'));

        if (push) history.pushState({ url }, '', url);
        window.scrollTo(0, 0);

        globalLoader.style.width = '100%';
        setTimeout(() => { globalLoader.style.opacity = '0'; }, 200);
        setTimeout(() => { globalLoader.style.width = '0%'; }, 500);

    } catch (error) {
        console.error('Soft navigation failed:', error);
        window.location.href = url; // Fallback to traditional load
    }
}

/**
 * Prefetcher on Hover
 */
document.addEventListener('mouseover', e => {
    const link = e.target.closest('a');
    if (link && link.href && link.onclick && link.onclick.toString().includes('softNavigate')) {
        const url = link.href;
        if (!prefetchCache[url] && !url.includes('#')) {
            prefetchCache[url] = fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        }
    }
});

window.onpopstate = (e) => {
    if (e.state && e.state.url) softNavigate(e.state.url, false);
};

/**
 * Toast Notifications
 */
function showToast(title, message) {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = 'toast';
    toast.innerHTML = `
        <div class="toast-icon">
            <i class="ph-info" style="color:var(--primary);"></i>
        </div>
        <div class="toast-content">
            <div class="toast-title">${title}</div>
            <div class="toast-message">${message}</div>
        </div>
        <button class="toast-close" onclick="this.parentElement.remove()">
            <i class="ph-x"></i>
        </button>
    `;

    container.appendChild(toast);
    requestAnimationFrame(() => toast.classList.add('show'));
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 400);
    }, 3000);
}

/**
 * Online/Offline Management
 * Updates all form/action buttons to be "Clean and Fast"
 */
function updateOnlineStatus() {
    const isOffline = !navigator.onLine;
    const buttons = document.querySelectorAll('button[type="submit"], button.btn-confirm-web, button.btn-primary');

    if (isOffline) {
        document.body.classList.add('offline-mode');
        buttons.forEach(btn => {
            if (!btn.hasAttribute('data-original-disabled')) {
                btn.setAttribute('data-original-disabled', btn.disabled);
            }
            btn.disabled = true;
            if (!btn.querySelector('.offline-icon')) {
                const icon = document.createElement('i');
                icon.className = 'ph-wifi-slash offline-icon';
                icon.style.marginRight = '8px';
                btn.prepend(icon);
            }
        });
        showToast('Connectivity', 'You are currently offline. Submission is disabled.');
    } else {
        document.body.classList.remove('offline-mode');
        buttons.forEach(btn => {
            if (btn.hasAttribute('data-original-disabled')) {
                btn.disabled = btn.getAttribute('data-original-disabled') === 'true';
                btn.removeAttribute('data-original-disabled');
            }
            const icon = btn.querySelector('.offline-icon');
            if (icon) icon.remove();
        });

        if (document.body.classList.contains('was-offline')) {
            showToast('Connectivity', 'Network connection restored.');
            document.body.classList.remove('was-offline');
        }
    }
}

window.addEventListener('online', updateOnlineStatus);
window.addEventListener('offline', () => {
    document.body.classList.add('was-offline');
    updateOnlineStatus();
});

// Setup Initial State on DOM Load
window.addEventListener('DOMContentLoaded', () => {
    updateActiveNav(window.location.pathname);
    updateOnlineStatus();

    // Register Service Worker from JS folder
    if ('serviceWorker' in navigator) {
        // Registering SW with scope '/' ensures it works for the entire site
        navigator.serviceWorker.register('/js/sw.js', { scope: '/' })
            .catch(err => console.warn('SW Register Error (Check Service-Worker-Allowed header):', err));
    }
});
