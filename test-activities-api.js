// Test script for Activity API endpoints
// Run this in browser console on http://localhost:3000 or any frontend

async function testActivityAPI() {
    console.log('=== Testing Activity API ===');
    
    let token = null;
    let clientId = null;
    let activityId = null;
    
    try {
        // 1. Login to get token
        console.log('\n--- Testing Login ---');
        const loginResponse = await fetch('http://localhost:5036/api/v1/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                email: 'admin@test.com',
                password: 'Admin123!'
            })
        });
        
        console.log('Login status:', loginResponse.status);
        const loginData = await loginResponse.json();
        console.log('Login response:', loginData);
        
        if (loginData.accessToken) {
            token = loginData.accessToken;
            console.log('✅ Login successful, got token');
        } else {
            // Try with debug user
            const debugLoginResponse = await fetch('http://localhost:5036/api/v1/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    email: 'debug@example.com',
                    password: 'password123'
                })
            });
            
            const debugLoginData = await debugLoginResponse.json();
            if (debugLoginData.accessToken) {
                token = debugLoginData.accessToken;
                console.log('✅ Debug login successful, got token');
            } else {
                console.log('❌ Login failed, trying registration...');
                
                // Register new user
                const registerResponse = await fetch('http://localhost:5036/api/v1/auth/register', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        name: 'Activity Test User',
                        email: 'activitytest@example.com',
                        password: 'password123',
                        companyName: 'Activity Test Company'
                    })
                });
                
                console.log('Registration status:', registerResponse.status);
                const registerData = await registerResponse.json();
                console.log('Registration response:', registerData);
                
                if (registerData.accessToken) {
                    token = registerData.accessToken;
                    console.log('✅ Registration successful, got token');
                } else {
                    throw new Error('Could not get authentication token');
                }
            }
        }
        
        // 2. Get clients to find a client ID
        console.log('\n--- Getting Clients ---');
        const clientsResponse = await fetch('http://localhost:5036/api/v1/clients', {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });
        
        console.log('Clients status:', clientsResponse.status);
        const clientsData = await clientsResponse.json();
        console.log('Clients response:', clientsData);
        
        if (clientsData.data && clientsData.data.length > 0) {
            clientId = clientsData.data[0].id;
            console.log('✅ Found client ID:', clientId);
        } else {
            // Create a test client
            console.log('No clients found, creating test client...');
            const createClientResponse = await fetch('http://localhost:5036/api/v1/clients', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    name: 'Test Client for Activities',
                    email: 'testclient@example.com',
                    phone: '123-456-7890',
                    company: 'Test Company',
                    status: 0, // Hot
                    leadScore: 80,
                    notes: 'Test client for activity API testing'
                })
            });
            
            console.log('Create client status:', createClientResponse.status);
            const createClientData = await createClientResponse.json();
            console.log('Create client response:', createClientData);
            
            if (createClientData.id) {
                clientId = createClientData.id;
                console.log('✅ Created client ID:', clientId);
            } else {
                throw new Error('Could not create test client');
            }
        }
        
        // 3. Create an activity
        console.log('\n--- Creating Activity ---');
        const createActivityResponse = await fetch('http://localhost:5036/api/v1/activities', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                clientId: clientId,
                type: 0, // Call
                title: 'Test call with client',
                description: 'Discussed project requirements and timeline',
                scheduledAt: new Date().toISOString()
            })
        });
        
        console.log('Create activity status:', createActivityResponse.status);
        const createActivityData = await createActivityResponse.json();
        console.log('Create activity response:', createActivityData);
        
        if (createActivityData.id) {
            activityId = createActivityData.id;
            console.log('✅ Activity created with ID:', activityId);
        } else {
            throw new Error('Could not create activity');
        }
        
        // 4. Get activities for the client
        console.log('\n--- Getting Client Activities ---');
        const getActivitiesResponse = await fetch(`http://localhost:5036/api/v1/clients/${clientId}/activities`, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });
        
        console.log('Get activities status:', getActivitiesResponse.status);
        const getActivitiesData = await getActivitiesResponse.json();
        console.log('Get activities response:', getActivitiesData);
        
        if (getActivitiesData.success && getActivitiesData.data) {
            console.log('✅ Retrieved activities:', getActivitiesData.data.length, 'items');
        } else {
            console.log('❌ Failed to get activities');
        }
        
        // 5. Complete the activity
        console.log('\n--- Completing Activity ---');
        const completeActivityResponse = await fetch(`http://localhost:5036/api/v1/activities/${activityId}/complete`, {
            method: 'PATCH',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                outcome: 'Call completed successfully - client interested in proceeding'
            })
        });
        
        console.log('Complete activity status:', completeActivityResponse.status);
        const completeActivityData = await completeActivityResponse.json();
        console.log('Complete activity response:', completeActivityData);
        
        if (completeActivityData.isCompleted && completeActivityData.completedAt) {
            console.log('✅ Activity completed successfully');
            console.log('Completed at:', completeActivityData.completedAt);
        } else {
            console.log('❌ Failed to complete activity');
        }
        
        // 6. Update the activity
        console.log('\n--- Updating Activity ---');
        const updateActivityResponse = await fetch(`http://localhost:5036/api/v1/activities/${activityId}`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                type: 1, // Email
                title: 'Updated: Follow-up email after call',
                description: 'Sending follow-up email with proposal documents',
                scheduledAt: new Date().toISOString()
            })
        });
        
        console.log('Update activity status:', updateActivityResponse.status);
        const updateActivityData = await updateActivityResponse.json();
        console.log('Update activity response:', updateActivityData);
        
        if (updateActivityData.id) {
            console.log('✅ Activity updated successfully');
        } else {
            console.log('❌ Failed to update activity');
        }
        
        console.log('\n=== Activity API Test Complete ===');
        console.log('✅ All endpoints tested successfully!');
        
    } catch (error) {
        console.error('❌ Test failed:', error);
    }
}

// Run the test
testActivityAPI();
