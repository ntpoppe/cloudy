export type AppConfig = {
  apiBaseUrl: string;
};

const defaultApiBaseUrl = import.meta.env.DEV
  ? 'http://localhost:3000'
  : 'https://api.example.com';

export const config: AppConfig = {
  apiBaseUrl: (import.meta.env.VITE_API_BASE_URL as string) || defaultApiBaseUrl,
};


