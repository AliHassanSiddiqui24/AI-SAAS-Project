// Force logout and clear all authentication data
// Run this in browser console on http://localhost:3000

async function forceLogout() {
    console.log('=== Forcing Logout ===');
    
    // Clear local storage
    localStorage.clear();
    sessionStorage.clear();
    
    // Clear the access token from window
    if (window.__ACCESS_TOKEN__) {
        delete window.__ACCESS_TOKEN__;
        console.log('Cleared window.__ACCESS_TOKEN__');
    }
    
    // Call logout endpoint to clear server-side session
    try {
        const response = await fetch('http://localhost:5036/api/v1/auth/logout', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include'
        });
        
        console.log('Logout response status:', response.status);
        if (response.ok) {
            const data = await response.json();
            console.log('Logout response:', data);
        }
    } catch (error) {
        console.log('Logout endpoint error (expected if not authenticated):', error.message);
    }
    
    // Check cookies after logout
    console.log('Cookies after logout:', document.cookie);
    console.log('Has refreshToken after logout:', document.cookie.includes('refreshToken'));
    
    // Redirect to login page
    console.log('Redirecting to login page...');
    window.location.href = '/login';
}

// Run the force logout
forceLogout();
