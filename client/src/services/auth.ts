import { http, authTokenStore } from './http';
import type { User } from '@/types';

export type LoginPayload = { usernameOrEmail: string; password: string };
export type AuthResponse = { token: string; user: User };

export const authService = {
  async login(payload: LoginPayload): Promise<User> {
    const { token, user } = await http.post<AuthResponse>('/api/auth/login', payload);
    authTokenStore.set(token);
    return user;
  },
  async logout(): Promise<void> {
    authTokenStore.set(null);
  },
  async getMe(): Promise<User> {
    return await http.get<User>('/api/auth/me');
  },
};


