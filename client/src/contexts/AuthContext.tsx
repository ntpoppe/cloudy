import React, { useEffect, useMemo, useState } from 'react';
import { authService } from '@/services/auth';
import { authTokenStore } from '@/services/http';
import type { User } from '@/types';
import { AuthContext, type AuthContextValue } from './AuthContextCore';

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let isMounted = true;
    (async () => {
      try {
        if (authTokenStore.get()) {
          const me = await authService.getMe();
          if (isMounted) setUser(me);
        }
      } catch {
        if (isMounted) setUser(null);
      } finally {
        if (isMounted) setIsLoading(false);
      }
    })();
    return () => {
      isMounted = false;
    };
  }, []);

  const login = async (usernameOrEmail: string, password: string) => {
    setIsLoading(true);
    try {
      const me = await authService.login({ usernameOrEmail, password });
      setUser(me);
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    try {
      await authService.logout();
    } finally {
      setUser(null);
    }
  };

  const register = async (username: string, email: string, password: string) => {
    try {
      const me = await authService.register({ username, email, password });
      setUser(me);
    } finally {
      setIsLoading(false);
    }
  }

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: !!user,
      isLoading,
      login,
      logout,
      register,
    }),
    [user, isLoading]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

