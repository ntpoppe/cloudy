import { http } from './http';
import type { User } from '@/types';

export type LoginPayload = { email: string; password: string };
export type AuthResponse = { user: User };

export const authService = {
  async login(payload: LoginPayload): Promise<User> {
    const { user } = await http.post<AuthResponse>('/auth/login', payload);
    return user;
  },
  async logout(): Promise<void> {
    await http.post<void>('/auth/logout');
  },
  async getMe(): Promise<User> {
    const { user } = await http.get<AuthResponse>('/auth/me');
    return user;
  },
};


