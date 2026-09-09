/**
 * Table Rowspan Styler
 * Xử lý border-radius cho các cell có rowspan trong table
 * CHỈ áp dụng cho route cụ thể: #page-default, #page-report-seo-performance và #page-report-seo-performance-by-pic
 * Force apply với inline style để đảm bảo override CSS
 */

(function () {
    'use strict';

    /**
     * Kiểm tra xem route hiện tại có phải là route cần xử lý không
     */
    function isTargetRoute() {
        // Kiểm tra xem có page-content với id là page-default, page-report-seo-performance hoặc page-report-seo-performance-by-pic không
        const pageContent = document.querySelector('#page-default, #page-report-seo-performance, #page-report-seo-performance-by-pic');
        return pageContent !== null;
    }

    /**
     * Xử lý border-radius cho các cell có rowspan
     */
    function processRowspanCells() {
        // CHỈ xử lý nếu đang ở route cụ thể
        if (!isTargetRoute()) {
            return;
        }

        // Tìm tất cả các table có class table-domain CHỈ trong page-default, page-report-seo-performance hoặc page-report-seo-performance-by-pic
        const pageContent = document.querySelector('#page-default, #page-report-seo-performance, #page-report-seo-performance-by-pic');
        if (!pageContent) {
            return;
        }

                const tables = pageContent.querySelectorAll('.table-domain, .table-compare');
                
                tables.forEach(table => {
                    const tbody = table.querySelector('tbody');
                    if (!tbody) return;

                    const rows = Array.from(tbody.querySelectorAll('tr'));
                    
                    // Tìm tất cả các cell có rowspan và class pic-name hoặc compare-rank-name
                    const picNameCells = Array.from(tbody.querySelectorAll('td.pic-name, td.compare-rank-name'));
            
            picNameCells.forEach((cell, cellIndex) => {
                const rowspan = parseInt(cell.getAttribute('rowspan') || '1', 10);
                const row = cell.closest('tr');
                if (!row) return;

                // Sử dụng class để xác định vị trí (support cả pic-name và compare-rank-name)
                const isFirstGroup = cell.classList.contains('pic-group-first') || cell.classList.contains('compare-group-first');
                const isLastGroup = cell.classList.contains('pic-group-last') || cell.classList.contains('compare-group-last');
                const isOnlyRow = rowspan === 1;

                // Reset tất cả border-radius trước
                cell.style.setProperty('border-radius', '0', 'important');
                cell.style.setProperty('border-top-left-radius', '0', 'important');
                cell.style.setProperty('border-top-right-radius', '0', 'important');
                cell.style.setProperty('border-bottom-left-radius', '0', 'important');
                cell.style.setProperty('border-bottom-right-radius', '0', 'important');

                // Force apply border-radius với inline style và !important
                // Figma design: rounded-bl-[20px] rounded-tl-[20px]
                // Top-left: 20px, Bottom-left: 20px
                // Top-right: 0, Bottom-right: 0
                
                if (isOnlyRow) {
                    // Row duy nhất - cả top-left và bottom-left: 20px
                    cell.style.setProperty('border-top-left-radius', '20px', 'important');
                    cell.style.setProperty('border-bottom-left-radius', '20px', 'important');
                } else if (isFirstGroup && isLastGroup) {
                    // Group duy nhất - cả top-left và bottom-left: 20px
                    cell.style.setProperty('border-top-left-radius', '20px', 'important');
                    cell.style.setProperty('border-bottom-left-radius', '20px', 'important');
                } else if (isFirstGroup && !isLastGroup) {
                    // Group đầu tiên (không phải cuối cùng) - chỉ bottom-left: 20px
                    // Không có top-left vì đây là group đầu tiên
                    cell.style.setProperty('border-bottom-left-radius', '20px', 'important');
                } else if (!isFirstGroup && isLastGroup) {
                    // Group cuối cùng (không phải đầu tiên) - cả top-left và bottom-left: 20px
                    // Có top-left vì đây là group thứ 2 trở đi
                    cell.style.setProperty('border-top-left-radius', '20px', 'important');
                    cell.style.setProperty('border-bottom-left-radius', '20px', 'important');
                } else if (!isFirstGroup && !isLastGroup) {
                    // Group giữa (không phải đầu tiên, không phải cuối cùng) - cả top-left và bottom-left: 20px
                    // Có top-left và bottom-left vì đây là group thứ 2 trở đi
                    cell.style.setProperty('border-top-left-radius', '20px', 'important');
                    cell.style.setProperty('border-bottom-left-radius', '20px', 'important');
                } else {
                    // Fallback: nếu không có class, dựa vào vị trí trong table
                    // Tìm tất cả các pic-name hoặc compare-rank-name cells để xác định vị trí
                    const allPicCells = Array.from(tbody.querySelectorAll('td.pic-name, td.compare-rank-name'));
                    const currentIndex = allPicCells.indexOf(cell);
                    const isFirst = currentIndex === 0;
                    const isLast = currentIndex === allPicCells.length - 1;
                    
                    if (isFirst && !isLast) {
                        // Cell đầu tiên - chỉ bottom-left
                        cell.style.setProperty('border-bottom-left-radius', '20px', 'important');
                    } else if (!isFirst) {
                        // Cell thứ 2 trở đi - cả top-left và bottom-left
                        cell.style.setProperty('border-top-left-radius', '20px', 'important');
                        cell.style.setProperty('border-bottom-left-radius', '20px', 'important');
                    }
                }
            });
        });
    }

    /**
     * Xử lý tất cả các table khi page load
     */
    function processAll() {
        // Đợi DOM render xong và delay thêm một chút để đảm bảo các class đã được apply
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                setTimeout(processRowspanCells, 200);
            });
        } else {
            setTimeout(processRowspanCells, 200);
        }
    }

    // Export function để có thể gọi từ Blazor
    window.tableRowspanStyler = {
        processAll: processAll,
        processRowspanCells: processRowspanCells
    };

    // Tự động chạy khi script load
    processAll();

    // Sử dụng MutationObserver để xử lý khi table được render động
    // CHỈ observe trong route cụ thể
    const observer = new MutationObserver(function (mutations) {
        // Kiểm tra route trước khi xử lý
        if (!isTargetRoute()) {
            return;
        }

        let shouldProcess = false;
        mutations.forEach(function (mutation) {
            if (mutation.addedNodes.length > 0) {
                mutation.addedNodes.forEach(function (node) {
                    if (node.nodeType === 1) { // Element node
                        // Kiểm tra xem node mới có phải là table hoặc chứa table không
                        // VÀ phải nằm trong page-default, page-report-seo-performance hoặc page-report-seo-performance-by-pic
                        if (node.matches && (
                            ((node.matches('.table-domain') || node.matches('.table-compare')) && 
                             (node.closest('#page-default') || node.closest('#page-report-seo-performance') || node.closest('#page-report-seo-performance-by-pic'))) ||
                            (node.querySelector && (node.querySelector('.table-domain') || node.querySelector('.table-compare')) &&
                             (node.closest('#page-default') || node.closest('#page-report-seo-performance') || node.closest('#page-report-seo-performance-by-pic')))
                        )) {
                            shouldProcess = true;
                        }
                    }
                });
            }
        });

        if (shouldProcess) {
            // Delay một chút để đảm bảo DOM đã được render hoàn toàn và các class đã được apply
            setTimeout(processRowspanCells, 300);
        }
    });

    // Bắt đầu observe
    if (document.body) {
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    } else {
        document.addEventListener('DOMContentLoaded', function () {
            observer.observe(document.body, {
                childList: true,
                subtree: true
            });
        });
    }
})();

