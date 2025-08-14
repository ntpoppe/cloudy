import { createContext } from 'react';
import type { User } from '@/types';

export type AuthContextValue = {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  register: (username: string, email: string, password: string) => Promise<void>;
};

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);


