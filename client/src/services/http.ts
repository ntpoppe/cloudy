import { config } from '@/lib/config';

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

type RequestOptions = {
  headers?: Record<string, string>;
  query?: Record<string, string | number | boolean | null | undefined>;
  body?: unknown;
  signal?: AbortSignal;
  credentials?: RequestCredentials; // defaults to 'include'
};

function buildUrl(path: string, query?: RequestOptions['query']): string {
  const base = config.apiBaseUrl.replace(/\/+$/, '');
  const p = path.replace(/^\/+/, '');
  const url = new URL(`${base}/${p}`);
  if (query) {
    Object.entries(query).forEach(([key, value]) => {
      if (value === undefined || value === null) return;
      url.searchParams.set(key, String(value));
    });
  }
  return url.toString();
}

async function request<T>(method: HttpMethod, path: string, options: RequestOptions = {}): Promise<T> {
  const { headers, query, body, signal, credentials } = options;
  const url = buildUrl(path, query);
  const init: RequestInit = {
    method,
    headers: {
      Accept: 'application/json',
      ...(body ? { 'Content-Type': 'application/json' } : {}),
      ...(headers || {}),
    },
    body: body ? JSON.stringify(body) : undefined,
    credentials: credentials ?? 'include',
    signal,
  };
  const res = await fetch(url, init);
  const contentType = res.headers.get('content-type') || '';
  const isJson = contentType.includes('application/json');

  if (!res.ok) {
    type MaybeErrorBody = { message?: string; error?: string } | unknown | undefined;
    const errorBody: MaybeErrorBody = isJson
      ? await res.json().catch(() => undefined)
      : await res.text().catch(() => undefined);

    const message = ((): string => {
      if (errorBody && typeof errorBody === 'object' && 'message' in (errorBody as Record<string, unknown>)) {
        const m = (errorBody as Record<string, unknown>).message;
        if (typeof m === 'string' && m.trim()) return m;
      }
      if (errorBody && typeof errorBody === 'object' && 'error' in (errorBody as Record<string, unknown>)) {
        const e = (errorBody as Record<string, unknown>).error;
        if (typeof e === 'string' && e.trim()) return e;
      }
      if (typeof errorBody === 'string' && errorBody.trim()) return errorBody;
      return res.statusText || 'Request failed';
    })();

    class ApiError extends Error {
      status: number;
      body: unknown;
      constructor(msg: string, status: number, body: unknown) {
        super(msg);
        this.name = 'ApiError';
        this.status = status;
        this.body = body;
      }
    }

    throw new ApiError(message, res.status, errorBody);
  }
  if (res.status === 204) {
    return undefined as unknown as T;
  }
  return (isJson ? await res.json() : await res.text()) as T;
}

export const http = {
  get: <T>(path: string, options?: RequestOptions) => request<T>('GET', path, options),
  post: <T>(path: string, body?: unknown, options?: Omit<RequestOptions, 'body'>) =>
    request<T>('POST', path, { ...(options || {}), body }),
  put: <T>(path: string, body?: unknown, options?: Omit<RequestOptions, 'body'>) =>
    request<T>('PUT', path, { ...(options || {}), body }),
  patch: <T>(path: string, body?: unknown, options?: Omit<RequestOptions, 'body'>) =>
    request<T>('PATCH', path, { ...(options || {}), body }),
  delete: <T>(path: string, options?: RequestOptions) => request<T>('DELETE', path, options),
};


