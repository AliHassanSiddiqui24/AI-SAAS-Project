// Test script to verify authentication endpoints
// Run this in browser console on http://localhost:3000

async function testAuth() {
    console.log('=== Testing Authentication ===');
    
    // Check current cookies
    console.log('Current cookies:', document.cookie);
    console.log('Has refreshToken:', document.cookie.includes('refreshToken'));
    
    // Test registration
    try {
        console.log('\n--- Testing Registration ---');
        const registerResponse = await fetch('http://localhost:5036/api/v1/auth/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',
            body: JSON.stringify({
                name: 'Debug User',
                email: 'debug@example.com',
                password: 'password123',
                companyName: 'Debug Company'
            })
        });
        
        console.log('Registration status:', registerResponse.status);
        const registerData = await registerResponse.json();
        console.log('Registration response:', registerData);
        
        // Check cookies after registration
        console.log('Cookies after registration:', document.cookie);
        console.log('Has refreshToken after registration:', document.cookie.includes('refreshToken'));
        
        // Test login
        console.log('\n--- Testing Login ---');
        const loginResponse = await fetch('http://localhost:5036/api/v1/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'include',
            body: JSON.stringify({
                email: 'debug@example.com',
                password: 'password123'
            })
        });
        
        console.log('Login status:', loginResponse.status);
        const loginData = await loginResponse.json();
        console.log('Login response:', loginData);
        
        // Check cookies after login
        console.log('Cookies after login:', document.cookie);
        console.log('Has refreshToken after login:', document.cookie.includes('refreshToken'));
        
    } catch (error) {
        console.error('Error during auth test:', error);
    }
}

// Run the test
testAuth();
