// Test cookie setting directly
// Run this in browser console on http://localhost:3000

async function testCookieSetting() {
    console.log('=== Testing Cookie Setting ===');
    
    // Test login with detailed cookie inspection
    try {
        console.log('Before login - Current cookies:', document.cookie);
        
        const loginResponse = await fetch('http://localhost:5036/api/v1/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',
            body: JSON.stringify({
                email: 'aaaa@email.com',
                password: 'password123' // Use the actual password
            })
        });
        
        console.log('Login status:', loginResponse.status);
        console.log('Login headers:', Object.fromEntries(loginResponse.headers.entries()));
        
        const loginData = await loginResponse.json();
        console.log('Login response:', loginData);
        
        // Check cookies immediately after login
        console.log('After login - Current cookies:', document.cookie);
        console.log('Has refreshToken:', document.cookie.includes('refreshToken'));
        
        // Wait a moment and check again
        setTimeout(() => {
            console.log('After delay - Current cookies:', document.cookie);
            console.log('Has refreshToken after delay:', document.cookie.includes('refreshToken'));
        }, 1000);
        
        // Test if we can access the cookie directly
        const cookies = document.cookie.split(';').map(c => c.trim());
        const refreshTokenCookie = cookies.find(c => c.startsWith('refreshToken='));
        console.log('Refresh token cookie found:', refreshTokenCookie);
        
    } catch (error) {
        console.error('Error testing cookie setting:', error);
    }
}

// Test the refresh endpoint directly
async function testRefreshEndpoint() {
    console.log('\n=== Testing Refresh Endpoint ===');
    
    try {
        const response = await fetch('http://localhost:5036/api/v1/auth/refresh', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include'
        });
        
        console.log('Refresh status:', response.status);
        console.log('Refresh headers:', Object.fromEntries(response.headers.entries()));
        
        if (response.ok) {
            const data = await response.json();
            console.log('Refresh response:', data);
        } else {
            const errorData = await response.json();
            console.log('Refresh error:', errorData);
        }
        
    } catch (error) {
        console.error('Error testing refresh:', error);
    }
}

// Run both tests
testCookieSetting();
setTimeout(testRefreshEndpoint, 2000);
