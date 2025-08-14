export type AppConfig = {
  apiBaseUrl: string;
};

export const config: AppConfig = {
  apiBaseUrl: (import.meta.env.VITE_API_BASE_URL as string) || 'http://localhost:9999'
};


