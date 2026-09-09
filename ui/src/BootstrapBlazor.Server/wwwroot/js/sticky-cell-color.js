/**
 * Sticky Cell Color Styler
 * Xử lý màu background cho header và các cell sticky có rowspan trong table
 * CHỈ áp dụng cho route: #page-report-detail-seo-performance
 * Header: màu đậm theo tab color
 * Rowspan cells: nhạt hơn 20% so với header
 */

(function () {
    'use strict';

    // 7 màu header cho 7 tab (màu đậm)
    const HEADER_COLORS = {
        blue: '#2d69b6',      // Tab 1: Summary - Blue
        red: '#ef504b',       // Tab 2: PUB SEO-G - Red
        green: '#f9751f',     // Tab 3: PUB SEO-S - Orange
        orange: '#ff9800',    // Tab 4: PUB S ĐÈ/F - Orange
        purple: '#9c27b0',    // Tab 5: PUB G ĐÈ - Purple
        teal: '#009688',      // Tab 6: BRANDS-MARTECH-G - Teal
        pink: '#e91e63'       // Tab 7: BRANDS-MARTECH-S - Pink
    };

    /**
     * Chuyển đổi hex color sang RGB
     */
    function hexToRgb(hex) {
        const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16)
        } : null;
    }

    /**
     * Chuyển đổi RGB sang hex
     */
    function rgbToHex(r, g, b) {
        return "#" + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1);
    }

    /**
     * Làm nhạt màu bằng cách mix với white
     * @param {string} hex - Màu hex
     * @param {number} percent - Phần trăm nhạt hơn (0-100)
     * @returns {string} - Màu hex đã được làm nhạt
     */
    function lightenColor(hex, percent) {
        const rgb = hexToRgb(hex);
        if (!rgb) return hex;

        // Mix với white (255, 255, 255)
        const r = Math.round(rgb.r + (255 - rgb.r) * (percent / 100));
        const g = Math.round(rgb.g + (255 - rgb.g) * (percent / 100));
        const b = Math.round(rgb.b + (255 - rgb.b) * (percent / 100));

        return rgbToHex(r, g, b);
    }

    /**
     * Kiểm tra xem route hiện tại có phải là route cần xử lý không
     */
    function isTargetRoute() {
        // Kiểm tra xem có page-content với id là page-report-detail-seo-performance hoặc page-report-detail-seo-performance-bod không
        const pageContent = document.querySelector('#page-report-detail-seo-performance, #page-report-detail-seo-performance-bod');
        return pageContent !== null;
    }

    /**
     * Xác định tab color từ class của table
     * @returns {object} - {headerColor, cellColor}
     */
    function getTabColors(table) {
        let headerColor = null;
        let tabType = null;

        // Kiểm tra tất cả 7 màu
        if (table.classList.contains('blue')) {
            headerColor = HEADER_COLORS.blue;
            tabType = 'blue';
        } else if (table.classList.contains('red')) {
            headerColor = HEADER_COLORS.red;
            tabType = 'red';
        } else if (table.classList.contains('green')) {
            headerColor = HEADER_COLORS.green;
            tabType = 'green';
        } else if (table.classList.contains('orange')) {
            headerColor = HEADER_COLORS.orange;
            tabType = 'orange';
        } else if (table.classList.contains('purple')) {
            headerColor = HEADER_COLORS.purple;
            tabType = 'purple';
        } else if (table.classList.contains('teal')) {
            headerColor = HEADER_COLORS.teal;
            tabType = 'teal';
        } else if (table.classList.contains('pink')) {
            headerColor = HEADER_COLORS.pink;
            tabType = 'pink';
        }

        if (!headerColor) {
            return null;
        }

        // Tính toán màu nhạt hơn 20% cho rowspan cells (body)
        const cellColor = lightenColor(headerColor, 20);
        
        // Tính toán màu nhạt hơn 40% cho footer cells (giảm thêm 20% so với header)
        const footerColor = lightenColor(headerColor, 40);

        return {
            headerColor: headerColor,
            cellColor: cellColor,      // Body: nhạt hơn 20% so với header
            footerColor: footerColor,  // Footer: nhạt hơn 40% so với header
            tabType: tabType
        };
    }

    /**
     * Xử lý màu cho header và các cell sticky có rowspan
     */
    function processStickyCells() {
        // CHỈ xử lý nếu đang ở route cụ thể
        if (!isTargetRoute()) {
            return;
        }

        // Tìm page content - hỗ trợ cả hai route
        const pageContent = document.querySelector('#page-report-detail-seo-performance, #page-report-detail-seo-performance-bod');
        if (!pageContent) {
            return;
        }

        // Tìm tất cả các table có class table-freeze
        const tables = pageContent.querySelectorAll('.table-freeze');
        
        tables.forEach(table => {
            const colors = getTabColors(table);
            if (!colors) {
                return; // Không có tab color, bỏ qua
            }

            // Áp dụng màu cho toàn bộ header (tất cả th trong thead) - đồng nhất với màu cột đầu tiên
            const thead = table.querySelector('thead');
            if (thead) {
                const allHeaderCells = thead.querySelectorAll('th');
                allHeaderCells.forEach(cell => {
                    cell.style.setProperty('background-color', colors.headerColor, 'important');
                    cell.style.setProperty('color', '#ffffff', 'important');
                });
            }

            // Áp dụng màu cho các cell sticky có rowspan (td.sticky[rowspan])
            const stickyCells = table.querySelectorAll('td.sticky[rowspan]');
            stickyCells.forEach(cell => {
                const rowspan = parseInt(cell.getAttribute('rowspan') || '1', 10);
                if (rowspan > 1) {
                    // Áp dụng màu nhạt hơn 20% với inline style để đảm bảo override CSS
                    cell.style.setProperty('background-color', colors.cellColor, 'important');
                    cell.style.setProperty('color', '#263238', 'important');
                }
            });

            // Áp dụng màu cho footer row (row "Tổng")
            // Tìm tất cả các row trong tbody
            const tbody = table.querySelector('tbody');
            if (tbody) {
                const allRows = Array.from(tbody.querySelectorAll('tr'));
                
                // Tìm row "Tổng" - thường là row cuối cùng hoặc có text "Tổng"
                allRows.forEach((row, index) => {
                    const isLastRow = index === allRows.length - 1;
                    const hasTotalText = row.textContent && row.textContent.trim().includes('Tổng');
                    
                    if (isLastRow || hasTotalText) {
                        // Tìm tất cả các cell trong row này
                        const allCellsInRow = row.querySelectorAll('td');
                        
                        allCellsInRow.forEach((td, cellIndex) => {
                            // Bỏ qua cell đầu tiên nếu có inline style="background-color: #FFF !important" hoặc empty
                            if (cellIndex === 0) {
                                const inlineStyle = td.getAttribute('style') || '';
                                // Kiểm tra nếu cell đầu tiên có style="background-color: #FFF !important" hoặc empty
                                if (inlineStyle.includes('background-color') && 
                                    (inlineStyle.includes('#FFF') || inlineStyle.includes('#fff') || 
                                     inlineStyle.includes('rgb(255, 255, 255)') || inlineStyle.includes('rgb(255,255,255)'))) {
                                    // Giữ nguyên màu trắng cho cell đầu tiên
                                    return;
                                }
                                // Nếu cell đầu tiên empty hoặc không có nội dung, cũng giữ nguyên
                                if (!td.textContent || td.textContent.trim() === '') {
                                    return;
                                }
                            }
                            
                            // Áp dụng màu nhạt hơn 40% (giảm thêm 20% so với header) cho TẤT CẢ các cell trong row "Tổng"
                            td.style.setProperty('background-color', colors.footerColor, 'important');
                            td.style.setProperty('color', '#263238', 'important');
                        });
                    }
                });
            }
        });
    }

    /**
     * Xử lý tất cả các table khi page load
     */
    function processAll() {
        // Đợi DOM render xong và delay thêm một chút để đảm bảo các class đã được apply
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                setTimeout(processStickyCells, 200);
            });
        } else {
            setTimeout(processStickyCells, 200);
        }
    }

    // Export function để có thể gọi từ Blazor
    window.stickyCellColorStyler = {
        processAll: processAll,
        processStickyCells: processStickyCells
    };

    // Tự động chạy khi script load
    processAll();

    // Lắng nghe sự kiện click tab để re-apply màu khi chuyển tab
    function setupTabChangeListener() {
        // Tìm tất cả các tab item
        const tabItems = document.querySelectorAll('.tabs-item, .tabs-item-body');
        tabItems.forEach(tabItem => {
            tabItem.addEventListener('click', function() {
                // Delay một chút để đảm bảo tab content đã được render
                setTimeout(function() {
                    if (isTargetRoute()) {
                        processStickyCells();
                    }
                }, 300);
            });
        });
    }

    // Setup tab change listener khi DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            setTimeout(setupTabChangeListener, 500);
        });
    } else {
        setTimeout(setupTabChangeListener, 500);
    }

    // Sử dụng MutationObserver để xử lý khi table được render động
    // CHỈ observe trong route cụ thể
    const observer = new MutationObserver(function (mutations) {
        // Kiểm tra route trước khi xử lý
        if (!isTargetRoute()) {
            return;
        }

        let shouldProcess = false;
        mutations.forEach(function (mutation) {
            // Detect khi table được thêm vào DOM
            if (mutation.addedNodes.length > 0) {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType === 1) { // Element node
                        // Kiểm tra xem node mới có phải là table hoặc chứa table không
                        // VÀ phải nằm trong page-report-detail-seo-performance hoặc page-report-detail-seo-performance-bod
                        if (node.matches && (
                            (node.matches('.table-freeze') && 
                             (node.closest('#page-report-detail-seo-performance') || node.closest('#page-report-detail-seo-performance-bod'))) ||
                            (node.querySelector && node.querySelector('.table-freeze') &&
                             (node.closest('#page-report-detail-seo-performance') || node.closest('#page-report-detail-seo-performance-bod')))
                        )) {
                            shouldProcess = true;
                        }
                    }
                });
            }
            
            // Detect khi class của table thay đổi (khi chuyển tab, class color có thể thay đổi)
            if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                const target = mutation.target;
                if (target && target.classList && target.classList.contains('table-freeze')) {
                    // Kiểm tra xem table có nằm trong route cụ thể không
                    if (target.closest('#page-report-detail-seo-performance') || target.closest('#page-report-detail-seo-performance-bod')) {
                        shouldProcess = true;
                    }
                }
            }
        });

        if (shouldProcess) {
            // Delay một chút để đảm bảo DOM đã được render hoàn toàn
            setTimeout(processStickyCells, 300);
        }
    });

    // Bắt đầu observe
    if (document.body) {
        observer.observe(document.body, {
            childList: true,
            subtree: true,
            attributes: true,
            attributeFilter: ['class'] // Chỉ observe thay đổi class attribute
        });
    } else {
        document.addEventListener('DOMContentLoaded', function () {
            observer.observe(document.body, {
                childList: true,
                subtree: true,
                attributes: true,
                attributeFilter: ['class'] // Chỉ observe thay đổi class attribute
            });
        });
    }
})();

