/**
 * Floating Label Positioner
 * Điều chỉnh vị trí label để đè lên border của input theo Figma design
 * Xử lý các component: Select, SelectGeneric, DateTimeRange, MultiSelectGeneric
 */

(function () {
    'use strict';

    /**
     * Điều chỉnh vị trí label cho một component
     * @param {HTMLElement} componentElement - Element của component (select, datetime-range, etc.)
     */
    function adjustLabelPosition(componentElement) {
        // Chỉ xử lý nếu component có class seo-input-size (class chính)
        const hasSeoInputSize = componentElement.classList.contains('seo-input-size') || 
                                 componentElement.querySelector('.seo-input-size') !== null;
        
        if (!hasSeoInputSize) {
            return;
        }

        // Tìm parent container (col-*) - bao gồm tất cả các col classes
        let parentCol = componentElement.closest('[class*="col-"]');
        if (!parentCol) {
            return;
        }

        // Đảm bảo parent container có position relative
        const parentStyle = window.getComputedStyle(parentCol);
        if (parentStyle.position === 'static') {
            parentCol.style.position = 'relative';
        }

        // Tìm label trước component (BootstrapLabel được render trước component)
        let label = parentCol.querySelector('.form-label');
        if (!label) {
            return;
        }

        // Kiểm tra xem label đã được xử lý chưa (tránh xử lý lại)
        if (label.dataset.floatingLabelProcessed === 'true') {
            return;
        }

        // Tính toán top position
        // Label cần đè lên border trên của input
        // Component có seo-input-size, chúng ta sẽ override padding-top về 0
        // Vậy label luôn ở top: -6px để đè lên border (border ở top: 0 của component)
        const labelTop = -6;

        // Áp dụng style cho label
        // Label container bắt đầu từ 16px (14px padding-left của input + 2px)
        // Text có padding-left: 5px để tạo khoảng cách từ border trái
        label.style.position = 'absolute';
        label.style.top = labelTop + 'px';
        label.style.left = '16px'; // 14px padding-left của input + 2px = 16px
        label.style.margin = '0';
        label.style.padding = '0 2px 0 5px'; // padding-left: 5px để tạo khoảng cách từ border trái
        label.style.backgroundColor = '#ffffff'; // Label text có background trắng để che border
        label.style.zIndex = '10'; /* Phải cao hơn input border để label nằm trên border */
        label.style.pointerEvents = 'none';
        label.style.display = 'block';
        label.style.transform = 'none';
        label.style.whiteSpace = 'nowrap';
        label.style.fontFamily = "'Public Sans', 'Roboto', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif";
        label.style.fontSize = '12px';
        label.style.fontWeight = '600';
        label.style.lineHeight = '12px';
        label.style.color = '#637381';

        // Background mask được xử lý bởi CSS ::before pseudo-element
        // Không cần tạo mask element bằng JS

        // Đánh dấu đã xử lý
        label.dataset.floatingLabelProcessed = 'true';

        // Kiểm tra xem component có class seo-input-size không
        const hasSeoInputSizeClass = componentElement.classList.contains('seo-input-size');

        // Override padding-top của component nếu có seo-input-size để label position đúng
        if (hasSeoInputSizeClass) {
            componentElement.style.paddingTop = '0';
            componentElement.style.paddingBottom = '0';
        }

        // Xử lý riêng cho DateTimeRange: đảm bảo input có đủ padding và height đúng, text căn giữa
        if (componentElement.classList.contains('datetime-range')) {
            // Đảm bảo container có height cố định 54px
            componentElement.style.height = '54px';
            componentElement.style.minHeight = '54px';
            componentElement.style.maxHeight = '54px';
            componentElement.style.overflow = 'hidden';
            
            const control = componentElement.querySelector('.datetime-range-control');
            if (control) {
                control.style.height = '100%';
                control.style.maxHeight = '54px';
                control.style.overflow = 'hidden';
                control.style.display = 'flex';
                control.style.alignItems = 'center';
            }
            
            // Đảm bảo position-relative div cũng căn giữa
            const positionRelative = componentElement.querySelector('.position-relative');
            if (positionRelative) {
                positionRelative.style.display = 'flex';
                positionRelative.style.alignItems = 'center';
                positionRelative.style.height = '100%';
            }
            
            // Đảm bảo icon căn giữa
            const icon = componentElement.querySelector('.range-bar');
            if (icon) {
                icon.style.display = 'flex';
                icon.style.alignItems = 'center';
            }
            
            // Đảm bảo range-separator căn giữa
            const separator = componentElement.querySelector('.range-separator');
            if (separator) {
                separator.style.display = 'flex';
                separator.style.alignItems = 'center';
                separator.style.height = '100%';
            }
            
            const inputs = componentElement.querySelectorAll('.datetime-range-input');
            inputs.forEach(input => {
                // Căn giữa text theo chiều dọc: (54px - 22px line-height) / 2 = 16px padding mỗi bên
                input.style.paddingTop = '16px';
                input.style.paddingBottom = '16px';
                input.style.height = 'auto';
                input.style.maxHeight = '54px';
                input.style.boxSizing = 'border-box';
                input.style.overflow = 'hidden';
                input.style.textOverflow = 'ellipsis';
                input.style.whiteSpace = 'nowrap';
                input.style.textAlign = 'center';
                input.style.verticalAlign = 'middle';
            });
        }

        // Xử lý riêng cho seo-input-size: đảm bảo padding được áp dụng đúng
        if (hasSeoInputSizeClass) {
            // Đảm bảo component container không có padding
            componentElement.style.paddingTop = '0';
            componentElement.style.paddingBottom = '0';
            
            // Đảm bảo input bên trong có padding đúng
            const formSelect = componentElement.querySelector('.form-select');
            if (formSelect) {
                formSelect.style.padding = '18px 14px 16px 14px';
                formSelect.style.height = '54px';
                formSelect.style.minHeight = '54px';
            }
            
            // Xử lý cho MultiSelectGeneric
            const dropdownToggle = componentElement.querySelector('.dropdown-toggle');
            if (dropdownToggle && componentElement.classList.contains('multi-select')) {
                dropdownToggle.style.padding = '18px 14px 16px 14px';
                dropdownToggle.style.height = '54px';
                dropdownToggle.style.minHeight = '54px';
                dropdownToggle.style.maxHeight = '54px';
                dropdownToggle.style.overflow = 'hidden';
                dropdownToggle.style.display = 'flex';
                dropdownToggle.style.alignItems = 'center';
                dropdownToggle.style.boxSizing = 'border-box';
                
                // Đảm bảo multi-select-items không vượt quá và có padding để tránh label
                const multiSelectItems = dropdownToggle.querySelector('.multi-select-items');
                if (multiSelectItems) {
                    multiSelectItems.style.maxHeight = '100%';
                    multiSelectItems.style.overflow = 'hidden';
                    multiSelectItems.style.display = 'flex';
                    multiSelectItems.style.flexWrap = 'wrap';
                    multiSelectItems.style.alignItems = 'center';
                    multiSelectItems.style.gap = '4px';
                    multiSelectItems.style.flex = '1';
                    multiSelectItems.style.minWidth = '0';
                    // Padding để tránh items đè lên label floating và border
                    multiSelectItems.style.paddingTop = '10px';
                    multiSelectItems.style.paddingBottom = '0';
                    multiSelectItems.style.paddingLeft = '2px'; // Padding nhẹ bên trái để không chồng lấn border
                    multiSelectItems.style.paddingRight = '2px'; // Padding nhẹ bên phải để không chồng lấn border
                    multiSelectItems.style.boxSizing = 'border-box';
                }
            }
        }
        
        // Xử lý riêng cho MultiSelectGeneric (kể cả không có seo-input-size)
        if (componentElement.classList.contains('multi-select')) {
            const dropdownToggle = componentElement.querySelector('.dropdown-toggle');
            if (dropdownToggle) {
                // Đảm bảo height cố định 54px
                dropdownToggle.style.height = '54px';
                dropdownToggle.style.minHeight = '54px';
                dropdownToggle.style.maxHeight = '54px';
                dropdownToggle.style.overflow = 'hidden';
                dropdownToggle.style.display = 'flex';
                dropdownToggle.style.alignItems = 'center';
                dropdownToggle.style.boxSizing = 'border-box';
                
                // Đảm bảo multi-select-items không vượt quá và có padding để tránh label
                const multiSelectItems = dropdownToggle.querySelector('.multi-select-items');
                if (multiSelectItems) {
                    multiSelectItems.style.maxHeight = '100%';
                    multiSelectItems.style.overflow = 'hidden';
                    multiSelectItems.style.display = 'flex';
                    multiSelectItems.style.flexWrap = 'wrap';
                    multiSelectItems.style.alignItems = 'center';
                    multiSelectItems.style.gap = '4px';
                    multiSelectItems.style.flex = '1';
                    multiSelectItems.style.minWidth = '0';
                    // Padding để tránh items đè lên label floating và border
                    multiSelectItems.style.paddingTop = '10px';
                    multiSelectItems.style.paddingBottom = '0';
                    multiSelectItems.style.paddingLeft = '2px'; // Padding nhẹ bên trái để không chồng lấn border
                    multiSelectItems.style.paddingRight = '2px'; // Padding nhẹ bên phải để không chồng lấn border
                    multiSelectItems.style.boxSizing = 'border-box';
                }
            }
        }
    }

    /**
     * Xử lý button với seo-input-size
     */
    function adjustButtonSize(buttonElement) {
        // Kiểm tra xem button có class seo-input-size không
        if (!buttonElement.classList.contains('seo-input-size')) {
            return;
        }

        // Đảm bảo button có height 54px
        buttonElement.style.height = '54px';
        buttonElement.style.minHeight = '54px';
        buttonElement.style.maxHeight = '54px';
        buttonElement.style.padding = '0 16px';
        buttonElement.style.display = 'flex';
        buttonElement.style.alignItems = 'center';
        buttonElement.style.justifyContent = 'center';
        buttonElement.style.boxSizing = 'border-box';
        buttonElement.style.fontSize = '15px';
        buttonElement.style.lineHeight = '22px';
        buttonElement.style.borderRadius = '8px';
    }

    /**
     * Xử lý tất cả các component trên page
     */
    function processAllComponents() {
        // Tìm tất cả các component có class seo-input-size (class chính)
        const selectors = [
            '.select.dropdown.seo-input-size',
            '.select.datetime-range.seo-input-size',
            '.select.form-control.seo-input-size',
            '.multi-select.seo-input-size',
            '.seo-input-size.select',
            '.seo-input-size.multi-select',
            // Fallback: tìm tất cả select và multi-select (sẽ được filter bởi adjustLabelPosition)
            '.select.dropdown',
            '.select.datetime-range',
            '.select.form-control',
            '.multi-select'
        ];

        selectors.forEach(selector => {
            const components = document.querySelectorAll(selector);
            components.forEach(component => {
                adjustLabelPosition(component);
            });
        });

        // Xử lý tất cả các button với seo-input-size
        const buttons = document.querySelectorAll('.btn.seo-input-size, button.seo-input-size');
        buttons.forEach(button => {
            adjustButtonSize(button);
        });
    }

    /**
     * Xử lý khi DOM đã sẵn sàng
     */
    function init() {
        // Xử lý ngay lập tức nếu DOM đã sẵn sàng
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', processAllComponents);
        } else {
            processAllComponents();
        }

        // Sử dụng MutationObserver để xử lý khi component được render động
        const observer = new MutationObserver(function (mutations) {
            let shouldProcess = false;
            mutations.forEach(function (mutation) {
                if (mutation.addedNodes.length > 0) {
                    mutation.addedNodes.forEach(function (node) {
                        if (node.nodeType === 1) { // Element node
                            // Kiểm tra xem node mới có phải là component cần xử lý không
                            if (node.matches && (
                                node.matches('.select.dropdown') ||
                                node.matches('.select.datetime-range') ||
                                node.matches('.select.form-control') ||
                                node.matches('.multi-select') ||
                                node.matches('.btn.seo-input-size') ||
                                node.matches('button.seo-input-size') ||
                                node.querySelector('.select.dropdown') ||
                                node.querySelector('.select.datetime-range') ||
                                node.querySelector('.select.form-control') ||
                                node.querySelector('.multi-select') ||
                                node.querySelector('.btn.seo-input-size') ||
                                node.querySelector('button.seo-input-size')
                            )) {
                                shouldProcess = true;
                            }
                        }
                    });
                }
            });

            if (shouldProcess) {
                // Delay một chút để đảm bảo DOM đã được render hoàn toàn
                setTimeout(processAllComponents, 100);
            }
        });

        // Bắt đầu observe
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });

        // Xử lý lại khi Blazor render xong (nếu có)
        if (window.Blazor) {
            // Hook vào Blazor render events
            const originalRender = window.Blazor.render;
            if (originalRender) {
                window.Blazor.render = function () {
                    const result = originalRender.apply(this, arguments);
                    setTimeout(processAllComponents, 50);
                    return result;
                };
            }
        }
    }

    // Khởi tạo
    init();

    // Export function để có thể gọi từ bên ngoài
    window.floatingLabelPositioner = {
        processAll: processAllComponents,
        adjust: adjustLabelPosition
    };

})();

