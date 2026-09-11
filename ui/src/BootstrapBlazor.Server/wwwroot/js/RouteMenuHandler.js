const OPENED_MENUS_STORAGE_KEY = 'sidebarOpenedMenus';
const controllers = new Map();
const openedMenuKeys = new Set();

function readStoredMenuKeys() {
    try {
        const raw = sessionStorage.getItem(OPENED_MENUS_STORAGE_KEY);
        if (!raw) return;
        const keys = JSON.parse(raw);
        if (Array.isArray(keys)) keys.forEach(key => openedMenuKeys.add(String(key)));
    } catch {
        // Storage is an optional UX preference, never a runtime dependency.
    }
}

function persistMenuKeys() {
    try {
        sessionStorage.setItem(OPENED_MENUS_STORAGE_KEY, JSON.stringify([...openedMenuKeys]));
    } catch {
        // Ignore private-browsing/storage quota failures.
    }
}

function getCollapseFromToggle(toggle, root) {
    const target = toggle?.getAttribute('data-bs-target');
    if (!target || !target.startsWith('#')) return null;
    try {
        return root.querySelector(target) || document.querySelector(target);
    } catch {
        return null;
    }
}

function isExpanded(collapse) {
    return collapse?.classList.contains('show') || collapse?.classList.contains('collapsing');
}

function setExpanded(toggle, collapse, expanded) {
    if (!toggle || !collapse) return;

    const Bootstrap = window.bootstrap;
    if (Bootstrap?.Collapse) {
        const instance = Bootstrap.Collapse.getOrCreateInstance(collapse, { toggle: false });
        expanded ? instance.show() : instance.hide();
    } else {
        collapse.classList.toggle('show', expanded);
    }

    toggle.setAttribute('aria-expanded', expanded ? 'true' : 'false');
    toggle.classList.toggle('collapsed', !expanded);
}

function menuKey(toggle, collapse) {
    return toggle?.dataset.menuKey || collapse?.id || '';
}

function syncExpandedState(root) {
    root.querySelectorAll('.nav-link[data-bs-toggle="collapse"][data-bs-target]').forEach(toggle => {
        const collapse = getCollapseFromToggle(toggle, root);
        if (!collapse) return;
        const key = menuKey(toggle, collapse);
        if (key && openedMenuKeys.has(key) && !isExpanded(collapse)) {
            setExpanded(toggle, collapse, true);
        }
        toggle.setAttribute('aria-expanded', isExpanded(collapse) ? 'true' : 'false');
    });
}

function normalizePath(value) {
    if (!value) return '';
    try {
        const url = new URL(value, window.location.origin);
        return url.pathname.toLowerCase().replace(/\/$/, '') || '/';
    } catch {
        return value.split('?')[0].split('#')[0].toLowerCase().replace(/\/$/, '') || '/';
    }
}

function syncActiveRoute(root) {
    if (!root || root.classList.contains('collapsed')) return;

    const currentPath = normalizePath(window.location.pathname);
    root.querySelectorAll('.nav-item > .nav-link[data-bs-toggle="collapse"]').forEach(toggle => {
        const collapse = getCollapseFromToggle(toggle, root);
        if (!collapse) return;
        const hasActiveChild = [...collapse.querySelectorAll('.nav-link[href]')].some(link => {
            const href = link.getAttribute('href');
            if (!href || href.startsWith('javascript:') || href.startsWith('#')) return false;
            const linkPath = normalizePath(href);
            return link.classList.contains('active') || currentPath === linkPath || currentPath.startsWith(`${linkPath}/`);
        });

        toggle.classList.toggle('is-active-parent', hasActiveChild);
        if (hasActiveChild && !isExpanded(collapse)) {
            const key = menuKey(toggle, collapse);
            if (key) openedMenuKeys.add(key);
            setExpanded(toggle, collapse, true);
        }
    });
    persistMenuKeys();
}

function bindRoot(root) {
    if (!root || controllers.has(root)) return;

    const onClick = event => {
        const toggle = event.target.closest('.nav-link[data-bs-toggle="collapse"][data-bs-target]');
        if (!toggle || !root.contains(toggle)) return;

        // Parent items are controls, not navigation links. Stop Bootstrap's data API
        // from running a second toggle after this controller handles the click.
        event.preventDefault();
        event.stopPropagation();

        const collapse = getCollapseFromToggle(toggle, root);
        if (!collapse) return;

        const expanded = !isExpanded(collapse);
        const key = menuKey(toggle, collapse);
        if (key) {
            expanded ? openedMenuKeys.add(key) : openedMenuKeys.delete(key);
            persistMenuKeys();
        }
        setExpanded(toggle, collapse, expanded);
    };

    root.addEventListener('click', onClick, true);
    controllers.set(root, { onClick });
    syncExpandedState(root);
    syncActiveRoute(root);
}

function unbindRoot(root) {
    const controller = controllers.get(root);
    if (!controller) return;
    root.removeEventListener('click', controller.onClick, true);
    controllers.delete(root);
}

function getRoots() {
    return [document.getElementById('sidebar'), document.getElementById('sidebar-mobile')].filter(Boolean);
}

function applySidebarCollapsedState() {
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.getElementById('main-content');
    const toggleIcon = document.querySelector('#sidebar-toggle-desktop i');
    if (!sidebar || !mainContent) return;

    const collapsed = sidebar.classList.contains('collapsed');
    mainContent.classList.toggle('sidebar-collapsed', collapsed);
    if (toggleIcon) {
        toggleIcon.classList.remove('fa-chevron-left', 'fa-chevron-right', 'fas');
        toggleIcon.classList.add('fa-solid', 'fa-bars');
    }
}

export function init() {
    readStoredMenuKeys();
    getRoots().forEach(root => bindRoot(root));
    applySidebarCollapsedState();
    getRoots().forEach(root => syncExpandedState(root));
    syncActiveRoute(document.getElementById('sidebar'));
}

export function handleRouteChange() {
    getRoots().forEach(root => {
        bindRoot(root);
        syncExpandedState(root);
        syncActiveRoute(root);
    });
    applySidebarCollapsedState();
}

export function restoreSidebarState() {
    const sidebar = document.getElementById('sidebar');
    if (!sidebar) return;
    const savedCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
    sidebar.classList.toggle('collapsed', savedCollapsed);
    applySidebarCollapsedState();
    if (!savedCollapsed) getRoots().forEach(root => syncExpandedState(root));
}

export function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    if (!sidebar) return;
    const collapsed = !sidebar.classList.contains('collapsed');
    sidebar.classList.toggle('collapsed', collapsed);
    localStorage.setItem('sidebarCollapsed', collapsed ? 'true' : 'false');
    applySidebarCollapsedState();
    if (!collapsed) getRoots().forEach(root => syncExpandedState(root));
}

export function applyActiveParentMenuStyles() {
    getRoots().forEach(root => syncActiveRoute(root));
}

export function setupCollapsedSubmenuHover() {
    // Kept as a compatibility entry point for older layout lifecycle calls.
    init();
}

window.seafoodSidebar = {
    init,
    handleRouteChange,
    restoreSidebarState,
    toggleSidebar,
    applyActiveParentMenuStyles,
    dispose: () => getRoots().forEach(unbindRoot)
};

window.handleRouteChange = handleRouteChange;
window.expandActiveMenu = handleRouteChange;
window.restoreSidebarState = restoreSidebarState;
window.setupCollapsedSubmenuHover = setupCollapsedSubmenuHover;
window.applyActiveParentMenuStyles = applyActiveParentMenuStyles;
window.toggleSidebar = toggleSidebar;

window.seafoodAudit = window.seafoodAudit || {
    getUserAgent: () => navigator.userAgent || ''
};

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init, { once: true });
} else {
    init();
}
