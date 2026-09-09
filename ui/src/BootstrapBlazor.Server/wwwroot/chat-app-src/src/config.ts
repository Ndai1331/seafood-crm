/**
 * Environment detection for dual-mode React chat app.
 * - Iframe mode: embedded in Blazor UI at task9.pro/agent
 * - Standalone mode: served directly by task9-agent at agent.task9.pro
 */

export const isIframe = (() => {
    try {
        return window.self !== window.top
    } catch {
        // Cross-origin iframe throws SecurityError — treat as iframe
        return true
    }
})()

/**
 * SSE chat endpoint differs between modes:
 * - Iframe: Blazor proxy at /api/agent-chat/stream (transforms SSE format)
 * - Standalone: Direct sidecar at /api/chat
 */
export const CHAT_ENDPOINT = isIframe
    ? '/api/agent-chat/stream'
    : '/api/chat'

/**
 * Get userId from URL param for standalone mode.
 * In iframe mode, userId comes via postMessage from Blazor parent.
 */
export function getStandaloneUserId(): number {
    const params = new URLSearchParams(window.location.search)
    return parseInt(params.get('userId') || '0')
}
