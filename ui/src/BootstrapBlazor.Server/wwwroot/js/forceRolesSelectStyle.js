window.forceRolesSelectStyle = function() {
    // Force override styles for roles-select elements
    const selectElements = document.querySelectorAll('#tab-kpi-content .roles-select, #tab-kpi-content select.roles-select, #tab-kpi-content input.roles-select');
    selectElements.forEach(function(el) {
        if (el && el.tagName === 'SELECT') {
            el.style.setProperty('background-color', '#2D69B6', 'important');
            el.style.setProperty('background', '#2D69B6', 'important');
            el.style.setProperty('color', 'white', 'important');
            el.style.setProperty('border', '1px solid #fff', 'important');
            el.style.setProperty('border-radius', '8px', 'important');
            el.style.setProperty('padding', '0 40px 0 14px', 'important');
            el.style.setProperty('font-family', "'Public Sans', sans-serif", 'important');
            el.style.setProperty('font-size', '15px', 'important');
            el.style.setProperty('font-weight', '400', 'important');
            el.style.setProperty('line-height', '22px', 'important');
            el.style.setProperty('min-height', '40px', 'important');
            el.style.setProperty('box-sizing', 'border-box', 'important');
            el.style.setProperty('appearance', 'none', 'important');
            el.style.setProperty('-webkit-appearance', 'none', 'important');
            el.style.setProperty('-moz-appearance', 'none', 'important');
        }
    });
    
    // Ensure icon is visible
    const icons = document.querySelectorAll('#tab-kpi-content .roles-select-icon, #tab-kpi-content .roles-select-wrapper i');
    icons.forEach(function(icon) {
        if (icon) {
            icon.style.setProperty('position', 'absolute', 'important');
            icon.style.setProperty('right', '14px', 'important');
            icon.style.setProperty('top', '50%', 'important');
            icon.style.setProperty('transform', 'translateY(-50%)', 'important');
            icon.style.setProperty('color', 'white', 'important');
            icon.style.setProperty('pointer-events', 'none', 'important');
            icon.style.setProperty('font-size', '14px', 'important');
            icon.style.setProperty('z-index', '10', 'important');
            icon.style.setProperty('display', 'block', 'important');
            icon.style.setProperty('visibility', 'visible', 'important');
            icon.style.setProperty('opacity', '1', 'important');
        }
    });
    
    // Also handle any input elements with class roles-select
    const inputElements = document.querySelectorAll('#tab-kpi-content input[type="text"].roles-select, #tab-kpi-content input.form-control.roles-select');
    inputElements.forEach(function(el) {
        if (el) {
            el.style.setProperty('background-color', '#2D69B6', 'important');
            el.style.setProperty('background', '#2D69B6', 'important');
            el.style.setProperty('color', 'white', 'important');
            el.style.setProperty('border', '1px solid #fff', 'important');
            el.style.setProperty('border-radius', '8px', 'important');
        }
    });
};

// Run on page load
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', window.forceRolesSelectStyle);
} else {
    window.forceRolesSelectStyle();
}

// Also run after a short delay to catch dynamically rendered elements
setTimeout(window.forceRolesSelectStyle, 500);
setTimeout(window.forceRolesSelectStyle, 1000);

