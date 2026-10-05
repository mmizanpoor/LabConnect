declare global {
  interface Window {
    __APP_CONFIG__?: {
      apiUrl?: string;
      devApiUrl?: string;
    };
  }
}

const DEV_SERVER_PORTS = new Set(['4500', '4200', '4300']);

function normalizeApiUrl(url: string): string {
  return url.endsWith('/') ? url : `${url}/`;
}

export function resolveApiUrl(): string {
  const config = window.__APP_CONFIG__;

  if (config?.apiUrl?.trim()) {
    return normalizeApiUrl(config.apiUrl.trim());
  }

  const { port } = window.location;
  if (port && DEV_SERVER_PORTS.has(port)) {
    const devApiUrl = config?.devApiUrl?.trim() || 'http://localhost:5299';
    return normalizeApiUrl(devApiUrl);
  }

  return normalizeApiUrl(window.location.origin);
}
