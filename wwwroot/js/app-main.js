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
const prefetched = new Map();
async function prefetchPage(url) {
    if (prefetched.has(url)) return;
    prefetched.set(url, null); // Mark as pending
    try {
        const res = await fetch(url, { priority: 'low' });
        if (res.ok) prefetched.set(url, await res.text());
    } catch { prefetched.delete(url); }
}

// ─── Fast Navigation (SPA Feel) ─────────────────────────────────────────────
async function fastNavigate(url, pushState = true) {
    if (!url || url.startsWith('#') || url.includes('/Account/Logout')) return false;

    NavProgress.start();
    try {
        let html;
        if (typeof prefetched === 'object' && prefetched instanceof Map && prefetched.has(url)) {
            html = prefetched.get(url);
        } else {
            const res = await fetch(url);
            if (!res.ok) { window.location.href = url; return; }
            html = await res.text();
        }

        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const newMain = doc.querySelector('.main-content');
        const oldMain = document.querySelector('.main-content');

        if (newMain && oldMain) {
            if (pushState) window.history.pushState({}, '', url);
            
            // OPTIMIZED: Delay 'loading' dimming to avoid flicker on fast pages
            let showLoading = true;
            const loadingTimer = setTimeout(() => { if (showLoading) oldMain.classList.add('loading'); }, 150);

            setTimeout(() => {
                showLoading = false; // Page is ready, don't show dimming if not already shown
                clearTimeout(loadingTimer);

                // Swap Title
                document.title = doc.title;

                // Swap Main Content
                oldMain.innerHTML = newMain.innerHTML;

                // Extract and execute scripts
                const scripts = oldMain.querySelectorAll('script');
                scripts.forEach(oldScript => {
                    const newScript = document.createElement('script');
                    Array.from(oldScript.attributes).forEach(attr => newScript.setAttribute(attr.name, attr.value));
                    newScript.appendChild(document.createTextNode(oldScript.innerHTML));
                    oldScript.parentNode.replaceChild(newScript, oldScript);
                });

                // Re-run animations and UI logic
                requestAnimationFrame(() => {
                    oldMain.classList.remove('loading');
                    animatePageEntry();
                    updateActiveNav(window.location.pathname);
                    setupNavLinks(); 
                    
                    // Dispatch a custom event for pages that need to know navigation happened
                    window.dispatchEvent(new CustomEvent('fastnav:success', { detail: { url } }));
                    
                    NavProgress.finish();
                });
            }, 50); // Keep minor delay for innerHTML swap consistency
            return true;
        } else {
            window.location.href = url; // Fallback to full load if containers are missing
            return false;
        }
    } catch (err) {
        console.error('Fast Navigation Error:', err);
        window.location.href = url; // Fallback to full load
    }
    return false;
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
    // Select all internal links that aren't logout or external
    const allLinks = document.querySelectorAll('a[href]:not([target="_blank"]):not([href^="http"]):not([href^="javascript"]):not([href^="#"]):not([href*="/Account/Logout"])');

    allLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (!href) return;

        // Prefetch on hover (using our new fetch-based prefetch)
        link.addEventListener('mouseenter', () => prefetchPage(href), { passive: true });

        // Fast Navigate on click
        link.addEventListener('click', (e) => {
            if (e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
            e.preventDefault();
            
            // Instantly update active state in sidebar
            const sidebarNav = document.querySelector('.sidebar-nav');
            if (sidebarNav) {
                sidebarNav.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));
                const navItem = link.closest('.nav-item');
                if (navItem) navItem.classList.add('active');
            }

            fastNavigate(href);

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
    
    // Smoothly lift and fade in from the .loading state
    main.style.transform = 'translateY(6px)';
    main.style.transition = 'opacity 0.4s cubic-bezier(0.4, 0, 0.2, 1), transform 0.4s cubic-bezier(0.4, 0, 0.2, 1)';
    
    requestAnimationFrame(() => {
        main.style.opacity = '1';
        main.style.transform = 'translateY(0)';
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

// ─── Form Interception (Fast Delete/Actions) ──────────────────────────────────
document.addEventListener('submit', async (e) => {
    const form = e.target;
    const btn = e.submitter || form.querySelector('button[type="submit"]');

    // Only intercept internal POST forms that aren't logout or external
    if (form.method.toLowerCase() !== 'post' || form.action.includes('/Account/Logout') || form.getAttribute('data-turbo') === 'false') return;

    e.preventDefault();
    if (btn) btn.classList.add('processing');

    // Show progress bar
    NavProgress.start();
    
    try {
        const formData = new FormData(form);
        const response = await fetch(form.action || window.location.href, {
            method: 'POST',
            body: formData,
            headers: { 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || '' }
        });

        if (response.ok) {
            // Success! Refresh the current page content
            await fastNavigate(window.location.pathname + window.location.search, false);
            showToast('Success', 'Action completed successfully.', 'success');
        } else {
            // If failed (e.g. validation error), fallback
            if (btn) btn.classList.remove('processing');
            form.submit();
        }
    } catch (err) {
        console.error('Form Submission Error:', err);
        if (btn) btn.classList.remove('processing');
        form.submit(); 
    } finally {
        NavProgress.finish();
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
    NavProgress.finish();
    updateActiveNav(window.location.pathname);
    setupNavLinks();
    animatePageEntry();
    updateOnlineStatus();

    window.addEventListener('popstate', () => {
        fastNavigate(window.location.pathname, false);
    });

    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register('/js/sw.js', { scope: '/' })
            .catch(err => console.warn('SW Register Error:', err));
    }
});

// Expose globally for _Layout.cshtml inline script
window.showToast = showToast;
