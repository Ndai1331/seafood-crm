window.alignTabsHeaderAndBody = function() {
    // Tìm tất cả các tabs trong component EvaluateSeoPerformance
    const tabs = document.querySelectorAll('.tabs');
    
    tabs.forEach(tab => {
        const tabsHeader = tab.querySelector('.tabs-header');
        const tabsBody = tab.querySelector('.tabs-body');
        
        if (tabsHeader && tabsBody) {
            // Lấy width của tabs-header (bao gồm padding và border)
            const headerRect = tabsHeader.getBoundingClientRect();
            const headerWidth = headerRect.width;
            
            // Lấy width của tab container
            const tabRect = tab.getBoundingClientRect();
            const tabWidth = tabRect.width;
            
            // Set width cho tabs-body để match với tabs-header
            // Sử dụng tabWidth để đảm bảo alignment
            tabsBody.style.width = tabWidth + 'px';
            tabsBody.style.maxWidth = tabWidth + 'px';
            tabsBody.style.minWidth = tabWidth + 'px';
            
            // Đảm bảo padding = 0 và margin-top = 30px để tạo khoảng cách
            tabsBody.style.padding = '0';
            tabsBody.style.margin = '0';
            tabsBody.style.marginTop = '30px';
            tabsBody.style.boxSizing = 'border-box';
            
            // Đảm bảo tabs-header cũng có width 100%
            tabsHeader.style.width = '100%';
            tabsHeader.style.boxSizing = 'border-box';
            
            // Đảm bảo các phần tử con cũng có width 100%
            // Lấy các phần tử con trực tiếp (children)
            const bodyChildren = Array.from(tabsBody.children).filter(child => child.tagName === 'DIV');
            bodyChildren.forEach(child => {
                child.style.width = '100%';
                child.style.margin = '0';
                child.style.padding = '0';
                child.style.boxSizing = 'border-box';
            });
        }
    });
};

// Chạy khi DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', window.alignTabsHeaderAndBody);
} else {
    window.alignTabsHeaderAndBody();
}

// Chạy lại khi window resize
window.addEventListener('resize', window.alignTabsHeaderAndBody);

// Sử dụng MutationObserver để theo dõi thay đổi DOM
if (typeof MutationObserver !== 'undefined') {
    const observer = new MutationObserver(function(mutations) {
        window.alignTabsHeaderAndBody();
    });
    
    // Quan sát thay đổi trong body
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
}

