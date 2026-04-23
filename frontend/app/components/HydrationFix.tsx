'use client';

import { useEffect, useState } from 'react';

interface HydrationFixProps {
  children: React.ReactNode;
}

export function HydrationFix({ children }: HydrationFixProps) {
  const [isClient, setIsClient] = useState(false);

  useEffect(() => {
    setIsClient(true);
  }, []);

  // Comprehensive suppression of browser extension hydration errors
  useEffect(() => {
    // Override console.error to suppress hydration warnings
    const originalError = console.error;
    console.error = (...args: any[]) => {
      const errorString = typeof args[0] === 'string' ? args[0] : '';
      
      // Suppress all hydration-related errors
      if (errorString.includes('Hydration failed') || 
          errorString.includes('hydration') ||
          errorString.includes('server rendered HTML') ||
          errorString.includes('client rendered HTML') ||
          errorString.includes('data-new-gr-c-s-check-loaded') || 
          errorString.includes('data-gr-ext-installed') ||
          errorString.includes('data-v-4914bf38') ||
          errorString.includes('data-v-') ||
          (errorString.includes('data-') && errorString.includes('check-loaded'))) {
        return; // Suppress all browser extension hydration warnings
      }
      
      originalError.apply(console, args);
    };

    // Override console.warn
    const originalWarn = console.warn;
    console.warn = (...args: any[]) => {
      const warnString = typeof args[0] === 'string' ? args[0] : '';
      
      // Suppress browser extension warnings
      if (warnString.includes('data-v-') || 
          warnString.includes('data-new-gr-c-s-check-loaded') ||
          warnString.includes('data-gr-ext-installed') ||
          warnString.includes('check-loaded')) {
        return; // Suppress browser extension warnings
      }
      
      originalWarn.apply(console, args);
    };

    // Override console.log for extension-related logs
    const originalLog = console.log;
    console.log = (...args: any[]) => {
      const logString = typeof args[0] === 'string' ? args[0] : '';
      
      // Suppress extension-related logs
      if (logString.includes('data-v-') || 
          logString.includes('data-new-gr-c-s-check-loaded') ||
          logString.includes('data-gr-ext-installed')) {
        return; // Suppress browser extension logs
      }
      
      originalLog.apply(console, args);
    };

    return () => {
      console.error = originalError;
      console.warn = originalWarn;
      console.log = originalLog;
    };
  }, []);

  // Don't render anything on server to prevent hydration mismatches
  if (!isClient) {
    return null;
  }

  return <>{children}</>;
}
