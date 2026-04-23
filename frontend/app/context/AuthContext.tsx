'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import axios from 'axios';
import axiosInstance from '@/lib/axios';

// Types
interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  tenantId?: string;
}

interface AuthContextType {
  user: User | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (userData: RegisterData) => Promise<void>;
  logout: () => void;
  refreshToken: () => Promise<void>;
}

interface RegisterData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  tenantName?: string;
}

// Create context
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Provider component
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Store access token in memory
  let accessToken: string | null = null;

  // Update global access token for axios interceptors
  const updateGlobalAccessToken = (token: string | null) => {
    accessToken = token;
    if (typeof window !== 'undefined') {
      (window as any).__ACCESS_TOKEN__ = token;
    }
  };

  // Initialize auth state on mount
  useEffect(() => {
    initializeAuth();
  }, []);

  const initializeAuth = async () => {
    try {
      // Try to refresh token on app load
      await refreshToken();
    } catch (error) {
      // No valid session, continue with user as null
      setUser(null);
    } finally {
      setIsLoading(false);
    }
  };

  const login = async (email: string, password: string) => {
    setIsLoading(true);
    try {
      const response = await axios.post('http://localhost:5035/api/v1/auth/login', {
        email,
        password,
      }, {
        withCredentials: true, // Important for httpOnly cookies
      });

      const { user: userData, accessToken: token } = response.data;
      
      setUser(userData);
      updateGlobalAccessToken(token);
    } catch (error) {
      setUser(null);
      updateGlobalAccessToken(null);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const register = async (userData: RegisterData) => {
    setIsLoading(true);
    try {
      const response = await axios.post('http://localhost:5035/api/v1/auth/register', userData, {
        withCredentials: true,
      });

      const { user: newUser, accessToken: token } = response.data;
      
      setUser(newUser);
      updateGlobalAccessToken(token);
    } catch (error) {
      setUser(null);
      updateGlobalAccessToken(null);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    try {
      // Call logout endpoint to clear httpOnly cookie
      await axios.post('http://localhost:5035/api/v1/auth/logout', {}, {
        withCredentials: true,
      });
    } catch (error) {
      // Continue with logout even if API call fails
      console.error('Logout API call failed:', error);
    } finally {
      setUser(null);
      updateGlobalAccessToken(null);
      
      // Redirect to login page
      if (typeof window !== 'undefined') {
        window.location.href = '/login';
      }
    }
  };

  const refreshToken = async () => {
    try {
      const response = await axios.post('http://localhost:5035/api/v1/auth/refresh', {}, {
        withCredentials: true,
      });

      const { user: userData, accessToken: token } = response.data;
      
      setUser(userData);
      updateGlobalAccessToken(token);
    } catch (error) {
      setUser(null);
      updateGlobalAccessToken(null);
      throw error;
    }
  };

  const value: AuthContextType = {
    user,
    isLoading,
    login,
    register,
    logout,
    refreshToken,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

// Hook to use auth context
export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

// Export for axios.ts
export function getAccessToken(): string | null {
  if (typeof window !== 'undefined') {
    return (window as any).__ACCESS_TOKEN__ || null;
  }
  return null;
}

export function setAccessToken(token: string): void {
  if (typeof window !== 'undefined') {
    (window as any).__ACCESS_TOKEN__ = token;
  }
}
