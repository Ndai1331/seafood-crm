// jspreadsheet CE v5 wrapper for Blazor interop
const sheetInstances = {};
const sheetColumns = {};
let cssLoaded = false;

const MIN_ROWS = 20;
const EXTRA_COLS = 5; // trailing empty columns like Google Sheets

/**
 * Dynamically load jspreadsheet v5 CSS only when sheet is used.
 * Prevents global style leaking onto other pages.
 */
function ensureCssLoaded() {
    if (cssLoaded) return;
    cssLoaded = true;
    const cssFiles = ['css/jsuites.css', 'css/jspreadsheet.css'];
    for (const href of cssFiles) {
        if (!document.querySelector(`link[href*="${href}"]`)) {
            const link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = href;
            document.head.appendChild(link);
        }
    }
}

/**
 * Pad data to ensure minimum row count for consistent height
 */
function padData(data, totalCols) {
    const rows = data && data.length > 0 ? [...data] : [];
    for (let i = 0; i < rows.length; i++) {
        const row = rows[i] || [];
        while (row.length < totalCols) row.push('');
        rows[i] = row;
    }
    while (rows.length < MIN_ROWS) {
        rows.push(new Array(totalCols).fill(''));
    }
    return rows;
}

/**
 * Create trailing empty column definitions for Google Sheet-like appearance.
 * Preserves source array for dropdown columns.
 */
function buildColumnsWithPadding(columns) {
    const cols = columns.map(c => {
        const col = { title: c.title, width: c.width, type: c.type || 'text', readOnly: c.readOnly };
        if (c.source && c.source.length > 0) {
            col.source = c.source;
        }
        return col;
    });
    for (let i = 0; i < EXTRA_COLS; i++) {
        cols.push({ title: ' ', width: 120, type: 'text', readOnly: true });
    }
    return cols;
}

/**
 * Check if an element is visible (not hidden by d-none or display:none)
 */
function isElementVisible(el) {
    if (!el) return false;
    const style = window.getComputedStyle(el);
    if (style.display === 'none') return false;
    // Walk up the DOM to check parent visibility (e.g. BootstrapBlazor Tab d-none)
    let parent = el.parentElement;
    while (parent) {
        const ps = window.getComputedStyle(parent);
        if (ps.display === 'none') return false;
        parent = parent.parentElement;
    }
    return true;
}

/**
 * Check if the element for a given sheet ID is currently visible in the DOM
 * Called from Blazor to decide whether to init/reinit the sheet
 */
export function isSheetVisible(elementId) {
    const el = document.getElementById(elementId);
    return isElementVisible(el);
}

/**
 * Initialize a jspreadsheet v5 instance on the given element
 * @param {string} elementId - DOM element ID
 * @param {Array} columns - Column definitions [{title, width, type, readOnly, ...}]
 * @param {Array} data - 2D array of row data
 * @param {boolean} readOnly - Whether the sheet is read-only
 * @param {object} dotnetRef - .NET object reference for callbacks
 */
export function initSheet(elementId, columns, data, readOnly, dotnetRef) {
    ensureCssLoaded();
    disposeSheet(elementId);

    const el = document.getElementById(elementId);
    if (!el) {
        console.warn('[seo-sheet] Element not found:', elementId);
        return;
    }

    // Skip init if element is hidden (parent tab has d-none) — Blazor will retry when visible
    if (!isElementVisible(el)) {
        console.log('[seo-sheet] Skipping init — element hidden:', elementId);
        return false;
    }

    return createSheet(el, elementId, columns, data, readOnly, dotnetRef);
}

/**
 * Force-initialize a sheet, skipping visibility checks.
 * Used when Blazor guarantees the element will be visible (e.g. after tab switch).
 * If element not found yet, returns false so Blazor can retry.
 */
export function forceInitSheet(elementId, columns, data, readOnly, dotnetRef) {
    ensureCssLoaded();
    disposeSheet(elementId);

    const el = document.getElementById(elementId);
    if (!el) {
        console.warn('[seo-sheet] forceInitSheet: element not found:', elementId);
        return false;
    }

    // Remove display:none inline style if present (from IsLoading toggle)
    if (el.style.display === 'none') {
        el.style.display = '';
    }

    console.log('[seo-sheet] forceInitSheet', elementId, 'rows:', data?.length ?? 0);
    return createSheet(el, elementId, columns, data, readOnly, dotnetRef);
}

/**
 * Internal: create the jspreadsheet instance on a DOM element
 */
function createSheet(el, elementId, columns, data, readOnly, dotnetRef) {
    console.log('[seo-sheet] createSheet', elementId, 'rows:', data?.length ?? 0);

    // Clear any existing content
    el.innerHTML = '';

    const paddedCols = buildColumnsWithPadding(columns);
    const totalCols = paddedCols.length;

    // If any column is editable (e.g. Active dropdown), the sheet must be editable
    // Per-column readOnly handles protection for other columns
    const hasEditableCol = paddedCols.some(c => !c.readOnly);
    const sheetEditable = readOnly ? hasEditableCol : true;

    // v5 uses worksheets array
    const options = {
        worksheets: [{
            data: padData(data, totalCols),
            columns: paddedCols,
            editable: sheetEditable,
            allowInsertRow: !readOnly,
            allowDeleteRow: !readOnly,
            allowInsertColumn: false,
            allowDeleteColumn: false,
            columnSorting: true,
            tableOverflow: true,
            tableHeight: '500px',
            defaultColWidth: 120,
        }],
        contextMenu: !readOnly ? undefined : () => false,
        // Always attach onchange when there are editable columns (e.g. dropdown)
        onchange: sheetEditable ? (_worksheet, _cell, x, y, value) => {
            if (dotnetRef) {
                dotnetRef.invokeMethodAsync('OnCellChanged', parseInt(x), parseInt(y), value?.toString() ?? '');
            }
        } : undefined
    };

    const sheet = jspreadsheet(el, options);
    sheetInstances[elementId] = sheet;
    sheetColumns[elementId] = columns;

    return true;
}

/**
 * Get selected row indices from the sheet
 */
export function getSelectedRows(elementId) {
    const sheet = sheetInstances[elementId];
    if (!sheet || !sheet[0]) return [];
    const selected = sheet[0].getSelectedRows(true);
    return selected || [];
}

/**
 * Set data on an existing sheet instance (v5: access first worksheet)
 */
export function setData(elementId, data) {
    const sheet = sheetInstances[elementId];
    if (!sheet || !sheet[0]) {
        console.warn('[seo-sheet] setData: no instance for', elementId);
        return;
    }
    console.log('[seo-sheet] setData', elementId, 'rows:', data?.length ?? 0);
    const origCols = sheetColumns[elementId] ?? [];
    const totalCols = origCols.length + EXTRA_COLS;
    const rows = padData(data, totalCols);
    sheet[0].setData(rows);
}

/**
 * Get all data from the sheet as a 2D array (strips trailing padding columns)
 */
export function getData(elementId) {
    const sheet = sheetInstances[elementId];
    if (!sheet || !sheet[0]) return [];
    const data = sheet[0].getData();
    const realColCount = (sheetColumns[elementId] ?? []).length;
    if (realColCount > 0 && data) {
        return data.map(row => row.slice(0, realColCount));
    }
    return data;
}

/**
 * Insert an empty row at the end of the sheet
 */
export function insertRow(elementId) {
    const sheet = sheetInstances[elementId];
    if (!sheet || !sheet[0]) return;
    sheet[0].insertRow();
}

/**
 * Delete selected rows from the sheet
 * @returns {number} Number of rows deleted
 */
export function deleteSelectedRows(elementId) {
    const sheet = sheetInstances[elementId];
    if (!sheet || !sheet[0]) return 0;

    const selected = sheet[0].getSelectedRows(true);
    if (!selected || selected.length === 0) return 0;

    // Delete from bottom to top to maintain correct indices
    const sorted = [...selected].sort((a, b) => b - a);
    for (const row of sorted) {
        sheet[0].deleteRow(row, 1);
    }
    return sorted.length;
}

/**
 * Get row count of the sheet
 */
export function getRowCount(elementId) {
    const sheet = sheetInstances[elementId];
    if (!sheet || !sheet[0]) return 0;
    const data = sheet[0].getData();
    return data ? data.length : 0;
}

/**
 * Set all cells in a column to the same value (used for Select All / Deselect All checkboxes)
 */
export function setColumnValues(elementId, colIndex, value) {
    const sheet = sheetInstances[elementId];
    if (!sheet || !sheet[0]) return;
    const data = sheet[0].getData();
    if (!data || data.length === 0) return;
    for (let row = 0; row < data.length; row++) {
        sheet[0].setValueFromCoords(colIndex, row, value, true);
    }
}

/**
 * Dispose/destroy a sheet instance (v5: destroy each worksheet)
 */
export function disposeSheet(elementId) {
    const sheet = sheetInstances[elementId];
    if (sheet) {
        try {
            if (Array.isArray(sheet)) {
                sheet.forEach(ws => { try { ws.destroy(); } catch {} });
            } else {
                sheet.destroy();
            }
        } catch {}
        delete sheetInstances[elementId];
        delete sheetColumns[elementId];
    }
}
