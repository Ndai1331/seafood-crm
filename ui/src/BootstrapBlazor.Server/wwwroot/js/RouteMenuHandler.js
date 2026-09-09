// Note: Data and EventHandler are not used in this module, but kept for consistency
// If needed, import from: /_content/BootstrapBlazor/modules/data.js

// Flag để tránh gọi expandActiveMenu nhiều lần đồng thời
let isExpandingMenu = false;

// Track các menu đã được user manually đóng (không tự động mở lại)
const manuallyClosedMenus = new Set();

// Tự động mở menu cha khi URL khớp với menu con
function expandActiveMenu(retryCount = 0) {
    // Tránh gọi nhiều lần đồng thời (nhưng cho phép retry)
    if (isExpandingMenu && retryCount === 0) {
        return;
    }
    
    const sidebar = document.getElementById('sidebar');
    if (!sidebar) {
        if (retryCount < 30) {
            setTimeout(() => expandActiveMenu(retryCount + 1), 100);
        }
        return;
    }
    
    // Check if menu items are rendered
    const menuItems = sidebar.querySelectorAll('.nav-item');
    if ((!menuItems || menuItems.length === 0) && retryCount < 30) {
        setTimeout(() => expandActiveMenu(retryCount + 1), 100);
        return;
    }
    
    // Chỉ expand khi sidebar không collapsed
    const isCollapsed = sidebar.classList.contains('collapsed');
    if (isCollapsed) {
        return;
    }
    
    // Kiểm tra Bootstrap có sẵn sàng không
    if (typeof bootstrap === 'undefined' || !bootstrap.Collapse) {
        // Retry nếu Bootstrap chưa load
        if (retryCount < 15) {
            setTimeout(() => expandActiveMenu(retryCount + 1), 100);
        }
        return;
    }
    
    // Set flag để tránh gọi lại (chỉ khi không phải retry)
    if (retryCount === 0) {
        isExpandingMenu = true;
    }
    
    // Lấy current URL path và normalize (chỉ lấy pathname, bỏ query string và hash)
    const currentPath = window.location.pathname.toLowerCase().replace(/\/$/, '');
    
    // Tìm tất cả NavLink trong collapse (menu con) - tìm cả trong collapse chưa mở
    const allNavLinks = sidebar.querySelectorAll('.nav-link[href]');
    const childNavLinks = Array.from(allNavLinks).filter(link => {
        const href = link.getAttribute('href');
        // Loại bỏ các link không hợp lệ
        if (!href || href === '#' || href === '' || href.startsWith('#') || 
            href.startsWith('javascript:') || href.startsWith('mailto:') || href.startsWith('tel:')) {
            return false;
        }
        // Chỉ lấy những NavLink nằm trong collapse (menu con)
        return link.closest('.collapse') !== null;
    });
    
    let foundActiveChild = false;
    let activeCollapse = null;
    let activeParentNavLink = null;
    
    // Ưu tiên tìm NavLink có class "active" (Blazor tự động thêm khi route khớp)
    childNavLinks.forEach(childLink => {
        // Kiểm tra xem NavLink có class "active" không (Blazor tự động thêm)
        const hasActiveClass = childLink.classList.contains('active');
        
        if (hasActiveClass) {
            foundActiveChild = true;
            
            // Tìm collapse chứa NavLink này
            const collapse = childLink.closest('.collapse');
            if (collapse) {
                // Tìm parent nav-link có data-bs-toggle="collapse"
                const parentNavItem = collapse.closest('.nav-item');
                if (parentNavItem) {
                    const parentNavLink = parentNavItem.querySelector('.nav-link[data-bs-toggle="collapse"]');
                    if (parentNavLink) {
                        const targetId = parentNavLink.getAttribute('data-bs-target');
                        if (targetId && collapse.id === targetId.substring(1)) {
                            activeCollapse = collapse;
                            activeParentNavLink = parentNavLink;
                        }
                    }
                }
            }
        }
    });
    
    // Nếu không tìm thấy bằng class "active", thử so sánh href với current URL
    if (!foundActiveChild) {
        childNavLinks.forEach(childLink => {
            const href = childLink.getAttribute('href');
            if (href && href !== '#') {
                // Parse href để lấy pathname (bỏ query string và hash nếu có)
                let hrefPath = href;
                try {
                    // Nếu href là relative path, sử dụng trực tiếp
                    if (href.startsWith('/')) {
                        hrefPath = href.split('?')[0].split('#')[0];
                    } else if (href.includes('://')) {
                        // Nếu là absolute URL, parse nó
                        const url = new URL(href);
                        hrefPath = url.pathname;
                    } else {
                        // Relative path
                        hrefPath = href.split('?')[0].split('#')[0];
                    }
                } catch (e) {
                    // Fallback: chỉ lấy phần trước ? và #
                    hrefPath = href.split('?')[0].split('#')[0];
                }
                
                // Normalize href path
                const normalizedHref = hrefPath.toLowerCase().replace(/\/$/, '');
                
                // So sánh chính xác hơn
                const hrefMatches = currentPath === normalizedHref || 
                                   (normalizedHref !== '' && currentPath.startsWith(normalizedHref + '/')) ||
                                   (normalizedHref === '' && currentPath === '');
                
                if (hrefMatches) {
                    foundActiveChild = true;
                    
                    // Tìm collapse chứa NavLink này
                    const collapse = childLink.closest('.collapse');
                    if (collapse) {
                        // Tìm parent nav-link có data-bs-toggle="collapse"
                        const parentNavItem = collapse.closest('.nav-item');
                        if (parentNavItem) {
                            const parentNavLink = parentNavItem.querySelector('.nav-link[data-bs-toggle="collapse"]');
                            if (parentNavLink) {
                                const targetId = parentNavLink.getAttribute('data-bs-target');
                                if (targetId && collapse.id === targetId.substring(1)) {
                                    activeCollapse = collapse;
                                    activeParentNavLink = parentNavLink;
                                }
                            }
                        }
                    }
                }
            }
        });
    }
    
    // Đóng tất cả các menu cha trước (chỉ giữ menu chứa active child mở)
    const allCollapses = sidebar.querySelectorAll('.collapse');
    const allParentNavLinks = sidebar.querySelectorAll('.nav-link[data-bs-toggle="collapse"]');
    
    // Đóng tất cả collapse (trừ collapse chứa active child)
    allCollapses.forEach(collapse => {
        if (collapse !== activeCollapse && collapse.classList.contains('show')) {
            try {
                const bsCollapse = bootstrap.Collapse.getOrCreateInstance(collapse, {
                    toggle: false
                });
                bsCollapse.hide();
            } catch (e) {
                collapse.classList.remove('show');
            }
        }
    });
    
    // Reset aria-expanded cho tất cả parent nav-links (trừ parent của active)
    allParentNavLinks.forEach(parentNavLink => {
        if (parentNavLink !== activeParentNavLink) {
            parentNavLink.setAttribute('aria-expanded', 'false');
        }
    });
    
    // Mở menu cha chứa menu con active (chỉ khi có active child thực sự)
    if (foundActiveChild && activeCollapse && activeParentNavLink) {
        // Kiểm tra xem menu này có được user manually đóng không
        const collapseId = activeCollapse.id;
        if (!manuallyClosedMenus.has(collapseId)) {
            try {
                // Sử dụng Bootstrap Collapse API để mở
                const bsCollapse = bootstrap.Collapse.getOrCreateInstance(activeCollapse, {
                    toggle: false
                });
                if (!activeCollapse.classList.contains('show')) {
                    bsCollapse.show();
                }
                // Đảm bảo aria-expanded được set
                activeParentNavLink.setAttribute('aria-expanded', 'true');
                // Xóa khỏi manually closed set vì đã tự động mở lại do có active child
                manuallyClosedMenus.delete(collapseId);
            } catch (e) {
                // Fallback: thêm class show nếu API không hoạt động
                activeCollapse.classList.add('show');
                activeParentNavLink.setAttribute('aria-expanded', 'true');
                manuallyClosedMenus.delete(collapseId);
            }
        }
        
        // Reset flag sau khi mở thành công
        setTimeout(() => {
            isExpandingMenu = false;
        }, 100);
    } else {
        // Nếu không tìm thấy active child, KHÔNG tự động mở menu nào cả
        // Chỉ retry nếu retryCount còn nhỏ (có thể Blazor chưa render xong)
        if (retryCount < 5) {
            // Reset flag để cho phép retry
            isExpandingMenu = false;
            // Tăng delay mỗi lần retry
            const delay = Math.min(300 + (retryCount * 100), 1000);
            setTimeout(() => expandActiveMenu(retryCount + 1), delay);
        } else {
            // Reset flag sau khi hết retry
            isExpandingMenu = false;
        }
    }
}

// Function để handle route change - expose ra window để gọi từ C#
export function handleRouteChange() {
    // Đợi một chút để Blazor render và đánh dấu active NavLink
    setTimeout(() => {
        if (typeof applyActiveParentMenuStyles === 'function') {
            applyActiveParentMenuStyles();
        }
        expandActiveMenu();
    }, 400);
}

// Expose function ra window để có thể gọi từ C# hoặc inline script
window.handleRouteChange = handleRouteChange;
window.expandActiveMenu = expandActiveMenu;

// Setup observer để detect khi NavLink trở thành active
function setupActiveMenuObserver() {
    const sidebar = document.getElementById('sidebar');
    if (!sidebar) {
        setTimeout(setupActiveMenuObserver, 100);
        return;
    }
    
    // Nếu đã setup rồi, không setup lại
    if (sidebar.dataset.activeObserverSetup) {
        return;
    }
    sidebar.dataset.activeObserverSetup = 'true';
    
    // Observe changes to class attribute on nav-links (để detect khi NavLink trở thành active)
    const activeObserver = new MutationObserver((mutations) => {
        let hasActiveChange = false;
        mutations.forEach((mutation) => {
            if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                const target = mutation.target;
                if (target.classList && target.classList.contains('nav-link')) {
                    // Kiểm tra xem có phải là menu con không (nằm trong collapse)
                    const isChildLink = target.closest('.collapse');
                    if (isChildLink) {
                        // Kiểm tra xem class "active" có được thêm vào không
                        const wasActive = mutation.oldValue && mutation.oldValue.includes('active');
                        const isActive = target.classList.contains('active');
                        if (isActive && !wasActive) {
                            hasActiveChange = true;
                        }
                    }
                }
            }
        });
        
        if (hasActiveChange && !isExpandingMenu) {
            // Đợi một chút để đảm bảo Blazor đã cập nhật xong
            // Chỉ expand nếu có active child thực sự
            setTimeout(() => {
                expandActiveMenu();
            }, 300);
        }
    });
    
    // Observe tất cả nav-links hiện có
    const observeNavLinks = () => {
        const navLinks = sidebar.querySelectorAll('.nav-link');
        navLinks.forEach(link => {
            activeObserver.observe(link, { 
                attributes: true, 
                attributeFilter: ['class'],
                attributeOldValue: true 
            });
        });
    };
    
    observeNavLinks();
    
    // Observe new nav-links being added
    const navObserver = new MutationObserver(() => {
        observeNavLinks();
    });
    navObserver.observe(sidebar, { childList: true, subtree: true });
    
    // Kiểm tra active menu ngay sau khi setup observer (trường hợp NavLink đã có class active từ đầu)
    // Chỉ expand nếu có active child thực sự
    setTimeout(() => {
        expandActiveMenu();
    }, 500);
}

// Lắng nghe khi Blazor navigation thay đổi
function setupNavigationListener() {
    if (window.Blazor && !window._navigationHandlerSetup) {
        window._navigationHandlerSetup = true;
        let lastLocation = window.location.href;
        
        const handleNavigation = () => {
            // Đợi lâu hơn để Blazor render và đánh dấu active NavLink
            setTimeout(() => {
                if (typeof applyActiveParentMenuStyles === 'function') {
                    applyActiveParentMenuStyles();
                }
                expandActiveMenu();
            }, 500);
        };
        
        // Lắng nghe popstate (back/forward button)
        window.addEventListener('popstate', handleNavigation);
        
        // Lắng nghe location change bằng cách check định kỳ
        const locationCheckInterval = setInterval(() => {
            if (window.location.href !== lastLocation) {
                lastLocation = window.location.href;
                handleNavigation();
            }
        }, 100);
        
        // Lắng nghe khi Blazor location thay đổi qua SignalR
        if (window.Blazor.navigateTo) {
            const originalNavigateTo = window.Blazor.navigateTo;
            window.Blazor.navigateTo = function(...args) {
                const result = originalNavigateTo.apply(this, args);
                lastLocation = window.location.href;
                handleNavigation();
                return result;
            };
        }
        
        // Lắng nghe click trên NavLink để detect navigation (chỉ cho menu con, không phải menu cha)
        document.addEventListener('click', (e) => {
            const navLink = e.target.closest('a.nav-link[href]');
            if (navLink && navLink.getAttribute('href') && navLink.getAttribute('href') !== '#' && navLink.getAttribute('href') !== 'javascript:void(0);') {
                // Chỉ handle navigation cho menu con (không phải menu cha có collapse)
                const isParentMenu = navLink.hasAttribute('data-bs-toggle') && navLink.getAttribute('data-bs-toggle') === 'collapse';
                if (!isParentMenu) {
                    setTimeout(() => {
                        lastLocation = window.location.href;
                        handleNavigation();
                    }, 300);
                }
            }
        }, true);
    }
}

export function init(id) {
    // Setup observer và navigation listener
    setupActiveMenuObserver();
    setupNavigationListener();
    
    // Gọi expandActiveMenu ngay sau khi init (chỉ mở nếu có active child)
    setTimeout(() => {
        expandActiveMenu();
    }, 800);
}

// Function để track khi user manually đóng menu
export function trackManualMenuClose(collapseId) {
    if (collapseId) {
        manuallyClosedMenus.add(collapseId);
    }
}

// Function để clear manual close state (khi có active child)
export function clearManualMenuClose(collapseId) {
    if (collapseId) {
        manuallyClosedMenus.delete(collapseId);
    }
}

// Expose functions
window.trackManualMenuClose = trackManualMenuClose;
window.clearManualMenuClose = clearManualMenuClose;

export function dispose(id) {
    // Cleanup nếu cần
    isExpandingMenu = false;
}

