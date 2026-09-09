/**
 * Chat Iframe Bridge - Handle communication between Blazor and React chat iframe
 */

let _dotNetRef = null;

export function initChatIframe(iframeElement, dotNetRef) {
  console.log('Initializing chat iframe bridge...');

  if (dotNetRef) _dotNetRef = dotNetRef;

  // Listen for messages from iframe
  window.addEventListener('message', (event) => {
    const { type, ...data } = event.data || {};

    switch (type) {
      case 'ready':
        console.log('✅ Chat iframe ready');
        break;

      case 'chat-completed':
        // React notifies us when a chat response is done — refresh sidebar
        console.log('💬 Chat completed, refreshing sidebar...');
        if (_dotNetRef) {
          _dotNetRef.invokeMethodAsync('OnChatCompleted').catch(e =>
            console.warn('Failed to notify Blazor chat-completed:', e)
          );
        }
        break;

      case 'error':
        console.error('❌ Chat iframe error:', data.message);
        break;

      case 'message-sent':
        console.log('📤 Message sent:', data.content);
        break;

      default:
        console.debug('Unknown iframe message:', type, data);
    }
  });

  console.log('Chat iframe bridge initialized');
}

export function sendMessageToIframe(iframeId, message) {
  const iframe = document.getElementById(iframeId);

  if (!iframe) {
    console.error(`Iframe #${iframeId} not found`);
    return;
  }

  if (!iframe.contentWindow) {
    console.error(`Iframe #${iframeId} contentWindow not available`);
    return;
  }

  try {
    iframe.contentWindow.postMessage(message, '*');
    console.log('📨 Message sent to iframe:', message.type);
  } catch (error) {
    console.error('Failed to send message to iframe:', error);
  }
}
