import { useState, useEffect, useRef, FormEvent } from 'react'
import { marked } from 'marked'
import DOMPurify from 'dompurify'
import { isIframe, CHAT_ENDPOINT, getStandaloneUserId } from './config'

interface Message {
  id: string
  role: 'user' | 'assistant'
  content: string
}

interface ToolContext {
  systemPromptSupplement?: string
  mcpServers?: string[]
}

function App() {
  const [messages, setMessages] = useState<Message[]>([])
  const [input, setInput] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string>()
  const [toolContext, setToolContext] = useState<ToolContext | null>(null)
  const [userId, setUserId] = useState<number>(0)
  const [conversationId] = useState<string>(() => crypto.randomUUID().replace(/-/g, ''))
  const messagesEndRef = useRef<HTMLDivElement>(null)

  // Notify parent that iframe is ready (only in iframe mode)
  useEffect(() => {
    if (!isIframe) return
    window.parent.postMessage({ type: 'ready' }, '*')
  }, [])

  // Standalone mode: get userId from URL param
  useEffect(() => {
    if (!isIframe) {
      setUserId(getStandaloneUserId())
    }
  }, [])

  // Listen for messages from Blazor parent (iframe mode only)
  useEffect(() => {
    if (!isIframe) return // Skip postMessage listener in standalone mode

    const handleMessage = (event: MessageEvent) => {
      if (event.origin !== window.location.origin) {
        console.warn('Ignored message from untrusted origin:', event.origin)
        return
      }

      if (event.data.type === 'tool-selected') {
        setToolContext({
          systemPromptSupplement: event.data.tool.systemPromptSupplement,
          mcpServers: event.data.tool.mcpServers,
        })
      } else if (event.data.type === 'clear-messages') {
        setMessages([])
      } else if (event.data.type === 'set-user-id') {
        setUserId(event.data.userId || 0)
      }
    }

    window.addEventListener('message', handleMessage)
    return () => window.removeEventListener('message', handleMessage)
  }, [])

  // Auto scroll to bottom
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!input.trim() || isLoading) return

    const userMessage: Message = {
      id: Date.now().toString(),
      role: 'user',
      content: input.trim(),
    }

    setMessages(prev => [...prev, userMessage])
    setInput('')
    setIsLoading(true)
    setError(undefined)

    const assistantMessage: Message = {
      id: (Date.now() + 1).toString(),
      role: 'assistant',
      content: '',
    }

    setMessages(prev => [...prev, assistantMessage])

    try {
      const response = await fetch(CHAT_ENDPOINT, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          messages: [...messages, userMessage].map(m => ({
            role: m.role,
            content: m.content,
          })),
          userId,
          conversationId,
          toolContext,
        }),
      })

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`)
      }

      const reader = response.body?.getReader()
      const decoder = new TextDecoder()

      if (!reader) throw new Error('No response body')

      while (true) {
        const { done, value } = await reader.read()
        if (done) break

        const chunk = decoder.decode(value)
        const lines = chunk.split('\n')

        for (const line of lines) {
          if (!line.startsWith('data: ')) continue

          const data = line.slice(6).trim()
          if (!data || data === '[DONE]') continue

          try {
            const parsed = JSON.parse(data)

            // Normalize both SSE formats:
            // Blazor proxy: {type:'text', value:'...'} + [DONE]
            // Sidecar direct: {type:'delta', text:'...'} + {type:'done'}
            if (parsed.type === 'text' || parsed.type === 'delta') {
              const chunk = parsed.value || parsed.text
              setMessages(prev => {
                const updated = [...prev]
                const last = updated[updated.length - 1]
                if (last.role === 'assistant') {
                  last.content += chunk
                }
                return updated
              })
            } else if (parsed.type === 'done') {
              // Explicit done from sidecar direct mode
              break
            } else if (parsed.type === 'error') {
              const errMsg = parsed.value || parsed.message
              setError(errMsg)
            }
          } catch (err) {
            console.warn('Failed to parse SSE data:', data, err)
          }
        }
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unknown error'
      setError(`Lỗi kết nối: ${message}`)
      console.error('Chat error:', err)
    } finally {
      setIsLoading(false)
      // Notify Blazor parent that chat completed → triggers sidebar refresh (iframe only)
      if (isIframe && userId > 0) {
        window.parent.postMessage({ type: 'chat-completed', conversationId }, '*')
      }
    }
  }

  const renderMarkdown = (content: string) => {
    const html = marked(content) as string
    return DOMPurify.sanitize(html)
  }

  return (
    <div className="flex flex-col h-full bg-gray-50">
      {/* Tool Badge */}
      {toolContext && (
        <div className="bg-blue-50 border-b border-blue-200 px-4 py-2">
          <div className="flex items-center gap-2 text-sm">
            <span className="font-medium text-blue-700">🔧 Tool Context:</span>
            {toolContext.mcpServers && toolContext.mcpServers.length > 0 && (
              <span className="px-2 py-1 bg-blue-100 text-blue-800 rounded-md text-xs font-medium">
                {toolContext.mcpServers.join(', ')}
              </span>
            )}
          </div>
        </div>
      )}

      {/* Messages */}
      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        {messages.length === 0 && (
          <div className="text-center text-gray-500 mt-8">
            <p className="text-lg">Xin chào! Tôi có thể giúp gì cho bạn?</p>
          </div>
        )}

        {messages.map((msg) => (
          <div
            key={msg.id}
            className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}
          >
            <div
              className={`max-w-[80%] rounded-lg px-4 py-3 ${msg.role === 'user'
                ? 'bg-blue-500 text-white'
                : 'bg-white shadow-sm border border-gray-200'
                }`}
            >
              {msg.role === 'assistant' ? (
                <div
                  dangerouslySetInnerHTML={{ __html: renderMarkdown(msg.content) }}
                  className="prose prose-sm max-w-none"
                />
              ) : (
                <p className="whitespace-pre-wrap">{msg.content}</p>
              )}
            </div>
          </div>
        ))}

        {isLoading && (
          <div className="flex justify-start">
            <div className="bg-white border border-gray-200 rounded-lg px-4 py-3 shadow-sm">
              <div className="flex space-x-2">
                <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce"></div>
                <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '0.2s' }}></div>
                <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '0.4s' }}></div>
              </div>
            </div>
          </div>
        )}

        <div ref={messagesEndRef} />
      </div>

      {/* Input */}
      <div className="border-t border-gray-200 bg-white p-4">
        {error && (
          <div className="mb-3 p-3 bg-red-50 border border-red-200 text-red-600 rounded-lg text-sm">
            <strong>Lỗi:</strong> {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="flex gap-3">
          <textarea
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault()
                handleSubmit(e)
              }
            }}
            placeholder="Nhập tin nhắn... (Enter để gửi, Shift+Enter để xuống dòng)"
            disabled={isLoading}
            className="flex-1 px-4 py-2 border border-gray-300 rounded-lg resize-none focus:outline-none focus:ring-2 focus:ring-blue-500"
            rows={3}
          />
          <button
            type="submit"
            disabled={isLoading || !input.trim()}
            className="px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors font-medium"
          >
            Gửi
          </button>
        </form>
      </div>
    </div>
  )
}

export default App
