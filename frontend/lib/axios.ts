import axios, { AxiosInstance, AxiosError, InternalAxiosRequestConfig } from 'axios';

// Create axios instance with base configuration
const axiosInstance: AxiosInstance = axios.create({
  baseURL: 'http://localhost:5036/api/v1',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Important for cookies
});

// Request interceptor: Add JWT token to outgoing requests
axiosInstance.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    // Get access token from memory (will be managed by AuthContext)
    const accessToken = getAccessToken();
    
    console.log('Request interceptor - Token available:', !!accessToken);
    console.log('Request interceptor - URL:', config.url);
    
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
      console.log('Request interceptor - Authorization header added');
    } else {
      console.log('Request interceptor - No token available');
    }
    
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Response interceptor: Handle 401 errors and token refresh
axiosInstance.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    
    // If error is 401 and we haven't tried to refresh yet
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        // Try to refresh the token
        const newAccessToken = await refreshAccessToken();
        
        if (newAccessToken) {
          // Store the new token
          setAccessToken(newAccessToken);
          
          // Retry the original request with the new token
          originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
          return axiosInstance(originalRequest);
        }
      } catch (refreshError) {
        // Refresh failed, redirect to login
        redirectToLogin();
        return Promise.reject(refreshError);
      }
    }
    
    return Promise.reject(error);
  }
);

// Helper functions (imported from AuthContext)
function getAccessToken(): string | null {
  if (typeof window !== 'undefined') {
    const token = (window as any).__ACCESS_TOKEN__ || null;
    console.log('getAccessToken - Retrieved token:', !!token);
    return token;
  }
  console.log('getAccessToken - Window not available');
  return null;
}

function setAccessToken(token: string): void {
  if (typeof window !== 'undefined') {
    (window as any).__ACCESS_TOKEN__ = token;
  }
}

async function refreshAccessToken(): Promise<string | null> {
  try {
    // Get refresh token from httpOnly cookie
    const response = await axios.post('http://localhost:5036/api/v1/auth/refresh', {}, {
      withCredentials: true, // Important for httpOnly cookies
    });
    
    return response.data.accessToken || null;
  } catch (error) {
    return null;
  }
}

function redirectToLogin(): void {
  if (typeof window !== 'undefined') {
    window.location.href = '/login';
  }
}

export default axiosInstance;
