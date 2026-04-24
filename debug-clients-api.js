// Debug clients API endpoints
// Run this in browser console on http://localhost:3000

async function debugClientsAPI() {
    console.log('=== Debugging Clients API ===');
    
    // Get the access token from window
    const accessToken = window.__ACCESS_TOKEN__;
    console.log('Access token available:', !!accessToken);
    
    if (!accessToken) {
        console.error('No access token found! User might not be properly authenticated.');
        return;
    }
    
    // Test 1: Get clients
    console.log('\n--- Testing GET /clients ---');
    try {
        const response = await fetch('http://localhost:5036/api/v1/clients?page=1&pageSize=20', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${accessToken}`,
                'Content-Type': 'application/json',
            }
        });
        
        console.log('GET clients status:', response.status);
        console.log('GET clients headers:', Object.fromEntries(response.headers.entries()));
        
        const data = await response.json();
        console.log('GET clients response:', data);
        
        if (response.ok) {
            console.log('✅ GET clients successful');
            console.log('Data structure:', Object.keys(data));
            if (data.data) {
                console.log('Number of clients:', data.data.length);
            }
        } else {
            console.log('❌ GET clients failed:', data);
        }
    } catch (error) {
        console.error('GET clients error:', error);
    }
    
    // Test 2: Create client with detailed error info
    console.log('\n--- Testing POST /clients ---');
    try {
        const clientData = {
            name: 'Test Client',
            email: 'test@example.com',
            phone: '123-456-7890',
            company: 'Test Company',
            status: 0, // Hot = 0, Warm = 1, Cold = 2
            leadScore: 80,
            notes: 'Test notes'
        };
        
        console.log('Sending client data:', clientData);
        
        const response = await fetch('http://localhost:5036/api/v1/clients', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${accessToken}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(clientData)
        });
        
        console.log('POST client status:', response.status);
        console.log('POST client headers:', Object.fromEntries(response.headers.entries()));
        
        const data = await response.json();
        console.log('POST client response:', data);
        
        if (response.ok) {
            console.log('✅ POST client successful');
        } else {
            console.log('❌ POST client failed:', data);
            console.log('Validation errors:', data.errors);
        }
    } catch (error) {
        console.error('POST client error:', error);
    }
}

// Run the debug
debugClientsAPI();
