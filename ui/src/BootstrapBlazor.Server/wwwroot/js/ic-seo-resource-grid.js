// Handle paste event from Excel for IC SEO Resource Grid Editor
// ES6 Module export
export function parseExcelPaste(text) {
    if (!text || typeof text !== 'string') {
        return "null";
    }

    // Split by newlines and filter empty lines
    const lines = text.split(/\r?\n/).filter(line => line.trim().length > 0);
    
    if (lines.length === 0) {
        return "null";
    }

    // First line is headers
    const headers = lines[0].split('\t').map(h => h.trim()).filter(h => h.length > 0);
    
    if (headers.length === 0) {
        return "null";
    }

    // Rest are data rows
    const rows = [];
    for (let i = 1; i < lines.length; i++) {
        const values = lines[i].split('\t');
        const row = {};
        
        // Map values to headers
        headers.forEach((header, index) => {
            row[header] = (values[index]?.trim() || '').toString();
        });
        
        // Only add row if it has at least one non-empty value
        const hasData = Object.values(row).some(val => val && val.trim().length > 0);
        if (hasData) {
            rows.push(row);
        }
    }

    return JSON.stringify({
        headers: headers,
        rows: rows
    });
}

// Also expose to window for backward compatibility
if (typeof window !== 'undefined') {
    window.icSeoResourceGrid = {
        parseExcelPaste: parseExcelPaste
    };
}
