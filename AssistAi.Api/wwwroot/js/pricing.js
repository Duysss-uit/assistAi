// ===========================
// AssistAi Pricing Page Logic
// Handles: auth check, Stripe checkout redirect,
// success/cancel banners, and current plan display.
// ===========================

// --- Configuration ---
const API_BASE = '/api';

// ============================
// 1. Authentication Check
// User should be logged in to upgrade
// ============================
function getToken() {
    return localStorage.getItem('assistai_token');
}

// ============================
// 2. Check URL params for success/cancel banners
// Stripe redirects back here with ?success=true or ?canceled=true
// ============================
function checkUrlParams() {
    const params = new URLSearchParams(window.location.search);

    if (params.get('success') === 'true') {
        // Show success banner
        document.getElementById('banner-success').style.display = 'block';
        // Remove the query params from URL (looks cleaner)
        window.history.replaceState({}, '', '/pricing.html');
    }

    if (params.get('canceled') === 'true') {
        // Show cancel banner
        document.getElementById('banner-cancel').style.display = 'block';
        window.history.replaceState({}, '', '/pricing.html');
    }
}

checkUrlParams();

// ============================
// 3. Upgrade to Pro Button
// Calls POST /api/payment/checkout to get Stripe checkout URL,
// then redirects the user to Stripe's hosted payment page.
// ============================
const btnUpgrade = document.getElementById('btn-upgrade');
const btnUpgradeText = document.getElementById('btn-upgrade-text');

btnUpgrade.addEventListener('click', async () => {
    const token = getToken();

    // If user is not logged in, redirect to login first
    if (!token) {
        window.location.href = '/login.html';
        return;
    }

    // Show loading state on button
    btnUpgrade.disabled = true;
    btnUpgradeText.textContent = 'Redirecting to Stripe...';

    try {
        // Call our backend to create a Stripe Checkout Session
        // The backend returns a Stripe-hosted payment URL
        const response = await fetch(`${API_BASE}/payment/checkout`, {
            method: 'POST',
            headers: {
                // Send JWT token so server knows who is upgrading
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        if (response.status === 401) {
            // Token expired — redirect to login
            localStorage.removeItem('assistai_token');
            window.location.href = '/login.html';
            return;
        }

        if (!response.ok) {
            const err = await response.json().catch(() => ({}));
            throw new Error(err.message || 'Failed to create checkout session');
        }

        const data = await response.json();

        // Redirect user to Stripe's hosted checkout page
        if (data.url) {
            window.location.href = data.url;
        } else {
            throw new Error('No checkout URL returned from server');
        }

    } catch (error) {
        console.error('Checkout error:', error);
        alert(`Error: ${error.message}\n\nPlease try again.`);
        // Reset button
        btnUpgrade.disabled = false;
        btnUpgradeText.textContent = 'Upgrade to Pro';
    }
});
