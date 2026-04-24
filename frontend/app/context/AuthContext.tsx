'use client';

import React, { createContext, useContext, useEffect, useState, useRef, ReactNode } from 'react';
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
  name: string;
  companyName?: string;
}

// Create context
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Provider component
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isInitialized, setIsInitialized] = useState(false);
  const initializationRef = useRef(false);

  // Store access token in memory
  let accessToken: string | null = null;

  // Update global access token for axios interceptors
  const updateGlobalAccessToken = (token: string | null) => {
    accessToken = token;
    console.log('AuthContext - Updating token:', !!token);
    if (typeof window !== 'undefined') {
      (window as any).__ACCESS_TOKEN__ = token;
      console.log('AuthContext - Token stored in window');
    }
  };

  // Initialize auth state on mount
  useEffect(() => {
    if (!isInitialized) {
      initializeAuth();
    }
  }, [isInitialized]);

  // Prevent re-initialization if user is already authenticated
  useEffect(() => {
    if (user && isInitialized) {
      console.log('AuthContext - User already authenticated, skipping re-initialization');
      return;
    }
  }, [user, isInitialized]);

  const initializeAuth = async () => {
    // Prevent multiple initializations due to React strict mode
    if (initializationRef.current) {
      console.log('AuthContext - Initialization already in progress, skipping');
      return;
    }
    
    initializationRef.current = true;
    console.log('AuthContext - Initializing auth...');
    
    // If user is already authenticated, don't re-initialize
    if (user) {
      console.log('AuthContext - User already authenticated, skipping initialization');
      setIsLoading(false);
      setIsInitialized(true);
      initializationRef.current = false;
      return;
    }
    
    try {
      // Check for refresh token in localStorage first (fallback), then cookies
      const hasRefreshToken = typeof window !== 'undefined' && 
        (localStorage.getItem('refreshToken') || 
         document.cookie.includes('refreshToken') || 
         document.cookie.includes('access_token'));
      
      console.log('AuthContext - Has refresh token:', hasRefreshToken);
      console.log('AuthContext - localStorage refreshToken:', localStorage.getItem('refreshToken'));
      console.log('AuthContext - Document cookie:', document.cookie);
      
      if (hasRefreshToken) {
        console.log('AuthContext - Attempting to refresh token...');
        await refreshToken();
        console.log('AuthContext - Token refresh successful');
      } else {
        console.log('AuthContext - No refresh token found, setting user to null');
        setUser(null);
      }
    } catch (error) {
      console.log('AuthContext - Error during initialization:', error);
      // No valid session, continue with user as null
      setUser(null);
    } finally {
      console.log('AuthContext - Initialization complete, setting isLoading to false');
      setIsLoading(false);
      setIsInitialized(true);
      initializationRef.current = false;
    }
  };

  const login = async (email: string, password: string) => {
    setIsLoading(true);
    try {
      const response = await axios.post('http://localhost:5036/api/v1/auth/login', {
        email,
        password,
      }, {
        withCredentials: true, // Important for httpOnly cookies
      });

      const { user: userData, accessToken: token, refreshToken } = response.data;
      
      setUser(userData);
      updateGlobalAccessToken(token);
      
      // Store refresh token in localStorage as fallback
      if (typeof window !== 'undefined' && refreshToken) {
        localStorage.setItem('refreshToken', refreshToken);
        console.log('AuthContext - Refresh token stored in localStorage');
      }
      
      return response.data;
    } catch (err: any) {
      setUser(null);
      updateGlobalAccessToken(null);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const register = async (userData: RegisterData) => {
    setIsLoading(true);
    try {
      const response = await axios.post('http://localhost:5036/api/v1/auth/register', userData, {
        withCredentials: true,
      });

      const { user: newUser, accessToken: token, refreshToken } = response.data;
      
      setUser(newUser);
      updateGlobalAccessToken(token);
      
      // Store refresh token in localStorage as fallback
      if (typeof window !== 'undefined' && refreshToken) {
        localStorage.setItem('refreshToken', refreshToken);
        console.log('AuthContext - Refresh token stored in localStorage');
      }
      
      return response.data;
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
      // Get refresh token from localStorage for logout API call
      const refreshToken = typeof window !== 'undefined' ? localStorage.getItem('refreshToken') : null;
      
      // Call logout endpoint with refresh token if available
      if (refreshToken) {
        await axios.post('http://localhost:5036/api/v1/auth/logout', { refreshToken }, {
          withCredentials: true,
        });
      } else {
        // Fallback to empty body for cookie-based logout
        await axios.post('http://localhost:5036/api/v1/auth/logout', {}, {
          withCredentials: true,
        });
      }
    } catch (error) {
      // Continue with logout even if API call fails
      console.error('Logout API call failed:', error);
    } finally {
      setUser(null);
      updateGlobalAccessToken(null);
      
      // Clear refresh token from localStorage
      if (typeof window !== 'undefined') {
        localStorage.removeItem('refreshToken');
        console.log('AuthContext - Refresh token cleared from localStorage');
      }
      
      // Redirect to login page
      if (typeof window !== 'undefined') {
        window.location.href = '/login';
      }
    }
  };

  const refreshToken = async () => {
    try {
      // Try to get refresh token from localStorage first, then fallback to empty body (for cookies)
      const refreshToken = typeof window !== 'undefined' ? localStorage.getItem('refreshToken') : null;
      
      const requestBody = refreshToken ? { refreshToken } : {};
      
      const response = await axios.post('http://localhost:5036/api/v1/auth/refresh', requestBody, {
        withCredentials: true,
      });

      const { user: userData, accessToken: token, refreshToken: newRefreshToken } = response.data;
      
      setUser(userData);
      updateGlobalAccessToken(token);
      
      // Update refresh token in localStorage if a new one is provided
      if (typeof window !== 'undefined' && newRefreshToken) {
        localStorage.setItem('refreshToken', newRefreshToken);
        console.log('AuthContext - New refresh token stored in localStorage');
      }
      
      return response.data;
    } catch (error) {
      console.log('AuthContext - Refresh token failed:', error);
      setUser(null);
      updateGlobalAccessToken(null);
      // Clear refresh token from localStorage on failure
      if (typeof window !== 'undefined') {
        localStorage.removeItem('refreshToken');
      }
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
