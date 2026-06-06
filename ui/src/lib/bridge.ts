import type { IncomingMessage } from './types';

// Single abstraction over window.chrome.webview. In dev (vite serve) the host
// is absent — we expose a no-op so the UI still renders for design work.
export interface Bridge {
  send(cmd: Record<string, unknown>): void;
  subscribe(handler: (msg: IncomingMessage) => void): () => void;
  readonly inHost: boolean;
}

export function createBridge(): Bridge {
  const wv = window.chrome?.webview;
  if (!wv) {
    // Dev fallback: log sends, do not subscribe.
    return {
      inHost: false,
      send: (cmd) => console.debug('[bridge:send (no host)]', cmd),
      subscribe: () => () => {},
    };
  }

  return {
    inHost: true,
    send: (cmd) => wv.postMessage(cmd),
    subscribe: (handler) => {
      const listener = (e: MessageEvent) => {
        try {
          const data = typeof e.data === 'string' ? JSON.parse(e.data) : e.data;
          if (data && typeof data === 'object' && typeof data.type === 'string') {
            handler(data as IncomingMessage);
          }
        } catch (err) {
          console.warn('Failed to parse bridge message', err, e.data);
        }
      };
      wv.addEventListener('message', listener);
      return () => wv.removeEventListener('message', listener);
    },
  };
}
