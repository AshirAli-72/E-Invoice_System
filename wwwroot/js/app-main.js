/**
 * app-main.js - E-Invoice System Core Logic
 * Handles sidebar, navigation, online/offline status, and toast notifications.
 */

// --- Global State ---
// --- Global State ---
const sidebar = document.querySelector('.sidebar');
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

    const sidebarBtn = e.target.closest('#sidebarToggle');
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
        showToast('Connectivity', 'You are currently offline. Please check your connection.');
    } else {
        document.body.classList.remove('offline-mode');
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
