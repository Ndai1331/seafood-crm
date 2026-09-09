/**
 * Global clipboard helper.
 *
 * Under Blazor Server a click is relayed to the server over SignalR before any
 * JS interop runs, so the browser's transient user-activation has expired by the
 * time JS executes. The async Clipboard API (navigator.clipboard.writeText)
 * strictly requires that activation and throws NotAllowedError. The legacy
 * textarea + execCommand('copy') path is far more lenient and succeeds in that
 * window, so we use the async API when available and fall back to execCommand.
 *
 * Usage from C#: JS.InvokeVoidAsync("copyTextToClipboard", text)
 * Returns true on success, false otherwise.
 */
(function () {
    'use strict';

    function legacyCopy(text) {
        const ta = document.createElement('textarea');
        ta.value = text;
        ta.style.position = 'fixed';
        ta.style.top = '0';
        ta.style.left = '0';
        ta.style.opacity = '0';
        document.body.appendChild(ta);
        ta.focus();
        ta.select();
        let ok = false;
        try {
            ok = document.execCommand('copy');
        } catch (e) {
            ok = false;
        }
        document.body.removeChild(ta);
        return ok;
    }

    window.copyTextToClipboard = async function (text) {
        if (navigator.clipboard && window.isSecureContext) {
            try {
                await navigator.clipboard.writeText(text);
                return true;
            } catch (e) {
                // Activation/permission lost (typical under Blazor Server) — fall back.
            }
        }
        return legacyCopy(text);
    };

    /**
     * Trigger a browser download of a file passed as base64 (csv, xlsx, pdf, images).
     *
     * Đây là helper tải file DUY NHẤT — mọi nội dung đều đi qua base64 nên không có chỗ nhầm
     * giữa "text thô" và "base64". Trước 2026-08-18 có hai hàm `downloadTextFile` trùng tên,
     * khác contract (một nhận text, một nhận base64); bản nạp sau thắng nên trang GeoBlock
     * Checker tải về file hỏng mà vẫn báo "Thành công". Đừng thêm biến thể nhận text trở lại.
     *
     * Usage from C#: JS.InvokeVoidAsync("downloadBase64File", name, base64, mime)
     */
    window.downloadBase64File = function (filename, base64, mime) {
        const binary = atob(base64);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) {
            bytes[i] = binary.charCodeAt(i);
        }
        const blob = new Blob([bytes], { type: mime || 'application/octet-stream' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        setTimeout(() => URL.revokeObjectURL(url), 1000);
    };
})();
