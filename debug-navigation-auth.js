// Debug authentication state during navigation
// Run this in browser console on http://localhost:3000

console.log('=== Navigation Auth Debug ===');

// Monitor localStorage changes
const originalSetItem = localStorage.setItem;
localStorage.setItem = function(key, value) {
    if (key === 'refreshToken') {
        console.log('🔄 localStorage refreshToken updated:', value ? 'Token set' : 'Token cleared');
    }
    return originalSetItem.call(this, key, value);
};

// Monitor window.__ACCESS_TOKEN__ changes
let currentAccessToken = window.__ACCESS_TOKEN__;
Object.defineProperty(window, '__ACCESS_TOKEN__', {
    get: function() {
        return currentAccessToken;
    },
    set: function(value) {
        console.log('🔄 window.__ACCESS_TOKEN__ updated:', value ? 'Token set' : 'Token cleared');
        currentAccessToken = value;
    }
});

// Check current auth state
function checkAuthState() {
    console.log('\n--- Current Auth State ---');
    console.log('localStorage refreshToken:', localStorage.getItem('refreshToken') ? 'Present' : 'Missing');
    console.log('window.__ACCESS_TOKEN__:', window.__ACCESS_TOKEN__ ? 'Present' : 'Missing');
    console.log('Document cookies:', document.cookie);
    
    // Try to get React context state (if available)
    const authContext = document.querySelector('[data-auth-context]');
    if (authContext) {
        console.log('React AuthContext available');
    }
}

// Initial check
checkAuthState();

// Monitor page navigation changes
let navigationCount = 0;
const originalPushState = history.pushState;
history.pushState = function(...args) {
    navigationCount++;
    console.log(`\n📍 Navigation #${navigationCount}: ${args[2] || 'Unknown route'}`);
    setTimeout(checkAuthState, 100); // Check state after navigation
    return originalPushState.apply(this, args);
};

// Monitor popstate (back/forward navigation)
window.addEventListener('popstate', function() {
    navigationCount++;
    console.log(`\n📍 Navigation #${navigationCount}: Back/Forward navigation`);
    setTimeout(checkAuthState, 100);
});

console.log('🔍 Navigation monitoring active. Navigate between pages to see auth state changes.');
console.log('💡 Try: Dashboard → Clients → Dashboard → Clients');
