// Augment Window with the WebView2 host bridge so TypeScript knows about it.
interface Ds4Bridge {
  postMessage(message: unknown): void;
  addEventListener(type: 'message', listener: (event: MessageEvent) => void): void;
  removeEventListener(type: 'message', listener: (event: MessageEvent) => void): void;
}

interface Window {
  chrome?: {
    webview?: Ds4Bridge;
  };
}
