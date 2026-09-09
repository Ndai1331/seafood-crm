/**
 * IC SEO Resource — inline style enforcer for .seo-input-size elements.
 * CSS specificity cannot override BootstrapBlazor component styles,
 * so we apply inline styles via JS after each render.
 */

const CONTAINER_ID = 'page-ic-seo-resource-management';
let _observer = null;

/** Height spec matching Figma design */
const H = '54px';

/** Apply uniform 54px height to all .seo-input-size elements and their wrappers */
function applyStyles() {
    const root = document.getElementById(CONTAINER_ID);
    if (!root) return;

    // 1) All elements with .seo-input-size class — enforce height
    root.querySelectorAll('.seo-input-size').forEach(el => {
        el.style.setProperty('height', H, 'important');
        el.style.setProperty('min-height', H, 'important');
        el.style.setProperty('max-height', H, 'important');
        el.style.setProperty('box-sizing', 'border-box', 'important');
    });

    // 2) .form-floating wrappers (BootstrapInput)
    root.querySelectorAll('.form-floating.seo-input-size, .form-floating:has(.seo-input-size)').forEach(el => {
        el.style.setProperty('height', H, 'important');
        el.style.setProperty('min-height', H, 'important');
        el.style.setProperty('max-height', H, 'important');
    });

    // 3) .form-floating inner .form-control
    root.querySelectorAll('.form-floating.seo-input-size .form-control, .form-floating:has(.seo-input-size) .form-control').forEach(el => {
        el.style.setProperty('height', H, 'important');
        el.style.setProperty('min-height', H, 'important');
        el.style.setProperty('max-height', H, 'important');
        el.style.setProperty('padding-top', '18px', 'important');
        el.style.setProperty('padding-right', '14px', 'important');
        el.style.setProperty('padding-bottom', '6px', 'important');
        el.style.setProperty('padding-left', '14px', 'important');
        el.style.setProperty('font-size', '15px', 'important');
        el.style.setProperty('border-radius', '10px', 'important');
    });

    // 4) .select wrappers (Select, DateTimeRange, DateTimePicker)
    root.querySelectorAll('.select.seo-input-size').forEach(el => {
        el.style.setProperty('height', H, 'important');
        el.style.setProperty('min-height', H, 'important');
        el.style.setProperty('max-height', H, 'important');
    });

    // 5) .form-select inside .select
    root.querySelectorAll('.select.seo-input-size .form-select').forEach(el => {
        el.style.setProperty('height', H, 'important');
        el.style.setProperty('min-height', H, 'important');
        el.style.setProperty('max-height', H, 'important');
        el.style.setProperty('padding-top', '18px', 'important');
        el.style.setProperty('padding-right', '14px', 'important');
        el.style.setProperty('padding-bottom', '16px', 'important');
        el.style.setProperty('padding-left', '14px', 'important');
    });

    // 6) .select-generic wrappers
    root.querySelectorAll('.select-generic.seo-input-size').forEach(el => {
        el.style.setProperty('height', H, 'important');
        el.style.setProperty('min-height', H, 'important');
        el.style.setProperty('max-height', H, 'important');
    });
    root.querySelectorAll('.select-generic.seo-input-size .dropdown-toggle').forEach(el => {
        el.style.setProperty('height', H, 'important');
        el.style.setProperty('min-height', H, 'important');
        el.style.setProperty('max-height', H, 'important');
    });

    // 7) DateTimePicker input — class seo-input-size is on wrapper, not on input
    root.querySelectorAll('.select.datetime-picker.seo-input-size > input.dropdown-toggle, .select.datetime-picker.seo-input-size > input.datetime-picker-input').forEach(el => {
        el.style.setProperty('height', H, 'important');
        el.style.setProperty('min-height', H, 'important');
        el.style.setProperty('max-height', H, 'important');
        // el.style.setProperty('padding-top', '14px', 'important');
        // el.style.setProperty('padding-right', '48px', 'important');
        // el.style.setProperty('padding-bottom', '14px', 'important');
        // el.style.setProperty('padding-left', '14px', 'important');
        el.style.setProperty('font-size', '15px', 'important');
        el.style.setProperty('border-radius', '10px', 'important');
        el.style.setProperty('border-width', '0.5px', 'important');
        el.style.setProperty('border-style', 'solid', 'important');
        el.style.setProperty('border-color', 'rgba(145, 158, 171, 0.2)', 'important');
    });

    // 8) DateTimePicker calendar icon — inline-right
    root.querySelectorAll('.select.datetime-picker.seo-input-size > .datetime-picker-bar, .select.datetime-picker.seo-input-size > i.datetime-picker-bar').forEach(el => {
        el.style.setProperty('position', 'absolute', 'important');
        el.style.setProperty('right', '12px', 'important');
        el.style.setProperty('top', '50%', 'important');
        el.style.setProperty('transform', 'translateY(-50%)', 'important');
        el.style.setProperty('pointer-events', 'none');
        el.style.setProperty('z-index', '2');
    });

    // 9) DateTimePicker wrapper needs position relative for icon
    root.querySelectorAll('.select.datetime-picker.seo-input-size').forEach(el => {
        el.style.setProperty('position', 'relative');
        el.style.setProperty('overflow', 'visible');
        el.style.setProperty('padding-top', '0', 'important');
        el.style.setProperty('padding-bottom', '0', 'important');
    });

    // 10) Buttons — same height, icon+text inline
    root.querySelectorAll('.btn.seo-input-size').forEach(el => {
        el.style.setProperty('height', H, 'important');
        el.style.setProperty('min-height', H, 'important');
        el.style.setProperty('max-height', H, 'important');
        el.style.setProperty('display', 'inline-flex', 'important');
        el.style.setProperty('flex-direction', 'row', 'important');
        el.style.setProperty('align-items', 'center', 'important');
        el.style.setProperty('justify-content', 'center', 'important');
        el.style.setProperty('gap', '6px', 'important');
        el.style.setProperty('white-space', 'nowrap', 'important');
        el.style.setProperty('border-radius', '10px', 'important');
    });

    // 11) input.form-control.seo-input-size (BootstrapInput direct)
    root.querySelectorAll('input.form-control.seo-input-size').forEach(el => {
        el.style.setProperty('height', H, 'important');
        el.style.setProperty('min-height', H, 'important');
        el.style.setProperty('max-height', H, 'important');
        el.style.setProperty('padding-top', '18px', 'important');
        el.style.setProperty('padding-right', '14px', 'important');
        el.style.setProperty('padding-bottom', '16px', 'important');
        el.style.setProperty('padding-left', '14px', 'important');
        el.style.setProperty('font-size', '15px', 'important');
        el.style.setProperty('border-radius', '10px', 'important');
        el.style.setProperty('border-width', '0.5px', 'important');
        el.style.setProperty('border-style', 'solid', 'important');
        el.style.setProperty('border-color', 'rgba(145, 158, 171, 0.2)', 'important');
    });
}

/**
 * Initialize the style enforcer — call from Blazor after each render.
 * Sets up a MutationObserver to re-apply when DOM changes (tab switches, etc.)
 */
export function init() {
    applyStyles();

    // Observe DOM changes to re-apply after Blazor re-renders
    if (_observer) _observer.disconnect();

    const root = document.getElementById(CONTAINER_ID);
    if (!root) return;

    _observer = new MutationObserver(() => {
        // Debounce: requestAnimationFrame to batch rapid mutations
        requestAnimationFrame(applyStyles);
    });

    _observer.observe(root, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ['class']
    });
}

/** Cleanup observer */
export function dispose() {
    if (_observer) {
        _observer.disconnect();
        _observer = null;
    }
}
