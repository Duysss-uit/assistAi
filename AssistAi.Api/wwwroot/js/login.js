// ===========================
// AssistAi Login Page Logic
// Handles login & register forms,
// calls the API, and stores JWT token.
// ===========================

// --- DOM Elements ---
const loginForm = document.getElementById('login-form');
const registerForm = document.getElementById('register-form');
const toggleLink = document.getElementById('toggle-link');
const toggleText = document.getElementById('toggle-text');
const brandTagline = document.getElementById('brand-tagline');
const messageEl = document.getElementById('message');

// Track which form is currently showing
let isLoginMode = true;

// --- API Base URL ---
// Since frontend is served from the same server, use relative URLs
const API_BASE = '/api';

// ============================
// Check if user is already logged in
// If they have a valid token, redirect to chat
// ============================
function checkExistingAuth() {
    const token = localStorage.getItem('assistai_token');
    if (token) {
        // User already has a token, send them to the chat page
        window.location.href = '/';
    }
}

// Run the check when page loads
checkExistingAuth();

// ============================
// Toggle between Login and Register forms
// ============================
toggleLink.addEventListener('click', (e) => {
    e.preventDefault(); // Prevent the link from navigating

    isLoginMode = !isLoginMode; // Flip the mode
    hideMessage();              // Clear any existing messages

    if (isLoginMode) {
        // Switch to Login view
        loginForm.classList.add('active');
        registerForm.classList.remove('active');
        toggleText.textContent = "Don't have an account?";
        toggleLink.textContent = "Register";
        brandTagline.textContent = "Sign in to your account";
    } else {
        // Switch to Register view
        loginForm.classList.remove('active');
        registerForm.classList.add('active');
        toggleText.textContent = "Already have an account?";
        toggleLink.textContent = "Sign In";
        brandTagline.textContent = "Create a new account";
    }
});

// ============================
// Show a message (error or success) to the user
// type: 'error' or 'success'
// ============================
function showMessage(text, type = 'error') {
    messageEl.textContent = text;
    messageEl.className = `message show ${type}`;
}

function hideMessage() {
    messageEl.className = 'message';
    messageEl.textContent = '';
}

// ============================
// Set a button to loading state (shows spinner, disables button)
// ============================
function setLoading(button, loading) {
    if (loading) {
        button.classList.add('loading');
        button.disabled = true;
    } else {
        button.classList.remove('loading');
        button.disabled = false;
    }
}

// ============================
// Handle Login Form Submission
// Sends username & password to the API,
// stores the JWT token on success.
// ============================
loginForm.addEventListener('submit', async (e) => {
    e.preventDefault(); // Prevent default form submission (page reload)
    hideMessage();

    // Get the values from the input fields
    const username = document.getElementById('login-username').value.trim();
    const password = document.getElementById('login-password').value;

    // Basic validation — make sure fields aren't empty
    if (!username || !password) {
        showMessage('Please fill in all fields');
        return;
    }

    const loginBtn = document.getElementById('login-btn');
    setLoading(loginBtn, true);

    try {
        // Send POST request to the login API endpoint
        const response = await fetch(`${API_BASE}/Auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                username: username,
                password: password
            })
        });

        // Parse the JSON response from the server
        const data = await response.json();

        if (response.ok) {
            // Success! Store the JWT token in localStorage
            // localStorage persists data even after closing the browser
            localStorage.setItem('assistai_token', data.token);
            localStorage.setItem('assistai_username', username);

            showMessage('Login successful! Redirecting...', 'success');

            // Wait a moment so the user sees the success message, then redirect
            setTimeout(() => {
                window.location.href = '/';
            }, 800);
        } else {
            // Server returned an error (wrong password, etc.)
            showMessage(data.message || 'Login failed. Please check your credentials.');
        }
    } catch (error) {
        // Network error or server is down
        console.error('Login error:', error);
        showMessage('Unable to connect to server. Please try again.');
    } finally {
        setLoading(loginBtn, false);
    }
});

// ============================
// Handle Register Form Submission
// Creates a new account, then switches to login form.
// ============================
registerForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    hideMessage();

    const username = document.getElementById('register-username').value.trim();
    const email = document.getElementById('register-email').value.trim();
    const password = document.getElementById('register-password').value;

    // Validate all fields
    if (!username || !email || !password) {
        showMessage('Please fill in all fields');
        return;
    }

    // Simple password length check
    if (password.length < 6) {
        showMessage('Password must be at least 6 characters');
        return;
    }

    const registerBtn = document.getElementById('register-btn');
    setLoading(registerBtn, true);

    try {
        const response = await fetch(`${API_BASE}/Auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                username: username,
                email: email,
                password: password
            })
        });

        const data = await response.json();

        if (response.ok) {
            showMessage('Account created! You can now sign in.', 'success');

            // Auto-switch to login form after a short delay
            setTimeout(() => {
                toggleLink.click(); // Simulate clicking the toggle link
            }, 1500);
        } else {
            showMessage(data.message || 'Registration failed. Please try again.');
        }
    } catch (error) {
        console.error('Register error:', error);
        showMessage('Unable to connect to server. Please try again.');
    } finally {
        setLoading(registerBtn, false);
    }
});
