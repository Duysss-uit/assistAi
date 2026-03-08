// ===========================
// AssistAi Chat Application Logic
// Handles: auth check, model loading,
// sending messages, displaying responses,
// chat history, and logout.
// ===========================

// --- Configuration ---
const API_BASE = '/api'; // Relative URL since frontend is on same server

// --- DOM Elements ---
const chatMessages = document.getElementById('chat-messages');
const welcomeScreen = document.getElementById('welcome-screen');
const chatInput = document.getElementById('chat-input');
const btnSend = document.getElementById('btn-send');
const charCount = document.getElementById('char-count');
const modelSelect = document.getElementById('model-select');
const btnNewChat = document.getElementById('btn-new-chat');
const btnLogout = document.getElementById('btn-logout');
const userName = document.getElementById('user-name');
const userAvatar = document.getElementById('user-avatar');
const historyList = document.getElementById('history-list');
const sidebar = document.getElementById('sidebar');
const sidebarToggle = document.getElementById('sidebar-toggle');
const sidebarOverlay = document.getElementById('sidebar-overlay');
const suggestions = document.getElementById('suggestions');

// --- State ---
// Stores the current conversation messages
// Each message: { role: 'user' | 'ai', content: '...' }
let currentMessages = [];

// Stores all chat sessions for history
// Each session: { id, title, messages: [...] }
let chatSessions = [];
let currentSessionId = null;

// Flag to prevent sending while AI is responding
let isWaitingForResponse = false;

// ============================
// 1. Authentication Check
// If no JWT token, redirect to login page
// ============================
function checkAuth() {
    const token = localStorage.getItem('assistai_token');
    if (!token) {
        window.location.href = '/login.html';
        return false;
    }
    return true;
}

// Run auth check on page load
if (!checkAuth()) {
    // Stop execution if not authenticated
    throw new Error('Not authenticated');
}

// ============================
// 2. Set up user display info
// ============================
function setupUserInfo() {
    const username = localStorage.getItem('assistai_username') || 'User';
    userName.textContent = username;
    // Show first letter as avatar
    userAvatar.textContent = username.charAt(0).toUpperCase();
}

setupUserInfo();

// ============================
// 3. Load available AI models
// Calls GET /api/Models to get the list
// ============================
async function loadModels() {
    const token = localStorage.getItem('assistai_token');
    try {
        const response = await fetch(`${API_BASE}/Models`, {
            headers: {
                // JWT authentication — send token in Authorization header
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.status === 401) {
            // Token expired or invalid — force re-login
            handleUnauthorized();
            return;
        }

        const models = await response.json();

        // Clear the dropdown and populate with models
        modelSelect.innerHTML = '';
        models.forEach((model, index) => {
            const option = document.createElement('option');
            option.value = model; // Full model ID like "stepfun/step-3.5-flash:free"
            // Show a cleaner name — take just the model name after the /
            const displayName = model.split('/').pop().replace(':free', ' (Free)');
            option.textContent = displayName;
            modelSelect.appendChild(option);
        });
    } catch (error) {
        console.error('Failed to load models:', error);
        modelSelect.innerHTML = '<option value="">Error loading models</option>';
    }
}

loadModels();

// ============================
// 4. Handle unauthorized (401) responses
// Clear token and redirect to login
// ============================
function handleUnauthorized() {
    localStorage.removeItem('assistai_token');
    localStorage.removeItem('assistai_username');
    window.location.href = '/login.html';
}

// ============================
// 5. Logout
// ============================
btnLogout.addEventListener('click', () => {
    localStorage.removeItem('assistai_token');
    localStorage.removeItem('assistai_username');
    localStorage.removeItem('assistai_sessions');
    window.location.href = '/login.html';
});

// ============================
// 6. Chat Input Handling
// Auto-resize textarea, character count
// ============================

// Update character count as user types
chatInput.addEventListener('input', () => {
    const len = chatInput.value.length;
    charCount.textContent = `${len} / 4000`;

    // Enable/disable send button based on input
    btnSend.disabled = len === 0 || isWaitingForResponse;

    // Auto-resize the textarea to fit content
    chatInput.style.height = 'auto';
    chatInput.style.height = Math.min(chatInput.scrollHeight, 150) + 'px';
});

// Send message on Enter (Shift+Enter for new line)
chatInput.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault(); // Prevent adding a new line
        if (!btnSend.disabled) {
            sendMessage();
        }
    }
});

// Send button click
btnSend.addEventListener('click', sendMessage);

// ============================
// 7. Send Message
// Main function that sends user message to API
// ============================
async function sendMessage() {
    const prompt = chatInput.value.trim();
    if (!prompt || isWaitingForResponse) return;

    const model = modelSelect.value;
    if (!model) {
        showErrorInChat('Please select a model first.');
        return;
    }

    // Hide welcome screen on first message
    if (welcomeScreen) {
        welcomeScreen.style.display = 'none';
    }

    // Add user message to UI
    addMessageToUI('user', prompt);
    currentMessages.push({ role: 'user', content: prompt });

    // Clear input
    chatInput.value = '';
    chatInput.style.height = 'auto';
    charCount.textContent = '0 / 4000';
    btnSend.disabled = true;
    isWaitingForResponse = true;

    // Show typing indicator while AI is responding
    const typingEl = showTypingIndicator();

    try {
        const token = localStorage.getItem('assistai_token');

        // Send the prompt to the API
        const response = await fetch(`${API_BASE}/Chat/chat`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                model: model,
                prompt: prompt
            })
        });

        // Remove typing indicator
        typingEl.remove();

        if (response.status === 401) {
            handleUnauthorized();
            return;
        }

        if (response.status === 429) {
            // Usage limit reached
            showErrorInChat('You have reached your usage limit. Please try again later or upgrade your plan.');
            isWaitingForResponse = false;
            btnSend.disabled = false;
            return;
        }

        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.error || `Server error (${response.status})`);
        }

        const data = await response.json();
        const aiResponse = data.response || 'No response from AI.';

        // Add AI response to UI
        addMessageToUI('ai', aiResponse);
        currentMessages.push({ role: 'ai', content: aiResponse });

        // Save to chat history
        saveCurrentSession();

    } catch (error) {
        // Remove typing indicator if still present
        typingEl.remove();
        console.error('Chat error:', error);
        showErrorInChat(`Error: ${error.message}`);
    } finally {
        isWaitingForResponse = false;
        // Re-enable send if there's text in the input
        btnSend.disabled = chatInput.value.trim().length === 0;
    }
}

// ============================
// 8. Add a message to the chat UI
// role: 'user' or 'ai'
// ============================
function addMessageToUI(role, content) {
    const row = document.createElement('div');
    row.className = `message-row ${role}`;

    if (role === 'ai') {
        // AI messages include an avatar icon
        row.innerHTML = `
            <div class="message-avatar">
                <svg viewBox="0 0 40 40" fill="none">
                    <text x="50%" y="55%" text-anchor="middle" dominant-baseline="middle" fill="#fff" font-size="18" font-weight="700" font-family="Space Grotesk">Ai</text>
                </svg>
            </div>
            <div class="message-bubble">${renderMarkdown(content)}</div>
        `;
    } else {
        // User messages — simple bubble
        row.innerHTML = `<div class="message-bubble">${escapeHtml(content)}</div>`;
    }

    chatMessages.appendChild(row);
    scrollToBottom();
}

// ============================
// 9. Basic Markdown Renderer
// Converts simple markdown to HTML for AI responses
// ============================
function renderMarkdown(text) {
    // First, escape HTML to prevent XSS attacks
    let html = escapeHtml(text);

    // Code blocks: ```code```
    html = html.replace(/```(\w*)\n([\s\S]*?)```/g, (match, lang, code) => {
        return `<pre><code>${code.trim()}</code></pre>`;
    });

    // Inline code: `code`
    html = html.replace(/`([^`]+)`/g, '<code>$1</code>');

    // Bold: **text** or __text__
    html = html.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');
    html = html.replace(/__(.+?)__/g, '<strong>$1</strong>');

    // Italic: *text* or _text_
    html = html.replace(/(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)/g, '<em>$1</em>');

    // Blockquotes: > text
    html = html.replace(/^&gt; (.+)$/gm, '<blockquote>$1</blockquote>');

    // Unordered lists: - item or * item
    html = html.replace(/^[-*] (.+)$/gm, '<li>$1</li>');
    html = html.replace(/(<li>.*<\/li>\n?)+/g, '<ul>$&</ul>');

    // Ordered lists: 1. item
    html = html.replace(/^\d+\. (.+)$/gm, '<li>$1</li>');

    // Links: [text](url)
    html = html.replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2" target="_blank" rel="noopener">$1</a>');

    // Paragraphs: split on double newlines
    html = html.replace(/\n\n/g, '</p><p>');
    html = `<p>${html}</p>`;

    // Single newlines as line breaks
    html = html.replace(/\n/g, '<br>');

    // Clean up empty paragraphs
    html = html.replace(/<p><\/p>/g, '');

    return html;
}

// ============================
// 10. Escape HTML to prevent XSS
// Converts special characters to safe equivalents
// ============================
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// ============================
// 11. Typing Indicator
// Shows three bouncing dots while AI is thinking
// ============================
function showTypingIndicator() {
    const row = document.createElement('div');
    row.className = 'message-row ai';
    row.innerHTML = `
        <div class="message-avatar">
            <svg viewBox="0 0 40 40" fill="none">
                <text x="50%" y="55%" text-anchor="middle" dominant-baseline="middle" fill="#fff" font-size="18" font-weight="700" font-family="Space Grotesk">Ai</text>
            </svg>
        </div>
        <div class="message-bubble">
            <div class="typing-indicator">
                <div class="typing-dot"></div>
                <div class="typing-dot"></div>
                <div class="typing-dot"></div>
            </div>
        </div>
    `;
    chatMessages.appendChild(row);
    scrollToBottom();
    return row;
}

// ============================
// 12. Show error in chat area
// ============================
function showErrorInChat(message) {
    const errorEl = document.createElement('div');
    errorEl.className = 'message-error';
    errorEl.textContent = message;
    chatMessages.appendChild(errorEl);
    scrollToBottom();
}

// ============================
// 13. Scroll chat to bottom
// Smoothly scrolls so latest message is visible
// ============================
function scrollToBottom() {
    chatMessages.scrollTo({
        top: chatMessages.scrollHeight,
        behavior: 'smooth'
    });
}

// ============================
// 14. Chat History Management
// Saves/loads chat sessions to localStorage
// ============================

// Generate a unique ID for each chat session
function generateId() {
    return Date.now().toString(36) + Math.random().toString(36).slice(2);
}

// Save the current conversation to history
function saveCurrentSession() {
    if (currentMessages.length === 0) return;

    if (!currentSessionId) {
        // Create a new session
        currentSessionId = generateId();
        // Use the first user message as the chat title
        const firstUserMsg = currentMessages.find(m => m.role === 'user');
        const title = firstUserMsg
            ? firstUserMsg.content.slice(0, 40) + (firstUserMsg.content.length > 40 ? '...' : '')
            : 'New Chat';

        chatSessions.unshift({
            id: currentSessionId,
            title: title,
            messages: [...currentMessages]
        });
    } else {
        // Update existing session
        const session = chatSessions.find(s => s.id === currentSessionId);
        if (session) {
            session.messages = [...currentMessages];
        }
    }

    // Save to localStorage (persists across page reloads)
    localStorage.setItem('assistai_sessions', JSON.stringify(chatSessions));
    renderChatHistory();
}

// Load chat sessions from localStorage
function loadChatHistory() {
    try {
        const saved = localStorage.getItem('assistai_sessions');
        if (saved) {
            chatSessions = JSON.parse(saved);
        }
    } catch (e) {
        chatSessions = [];
    }
    renderChatHistory();
}

// Render the chat history list in the sidebar
function renderChatHistory() {
    if (chatSessions.length === 0) {
        historyList.innerHTML = '<div class="history-empty">No chat history yet</div>';
        return;
    }

    historyList.innerHTML = '';
    chatSessions.forEach(session => {
        const item = document.createElement('div');
        item.className = `history-item ${session.id === currentSessionId ? 'active' : ''}`;
        item.innerHTML = `
            <svg class="history-item-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/></svg>
            <span>${escapeHtml(session.title)}</span>
        `;
        // Click to load a previous chat session
        item.addEventListener('click', () => loadSession(session.id));
        historyList.appendChild(item);
    });
}

// Load a specific chat session
function loadSession(sessionId) {
    const session = chatSessions.find(s => s.id === sessionId);
    if (!session) return;

    currentSessionId = sessionId;
    currentMessages = [...session.messages];

    // Clear chat area
    chatMessages.innerHTML = '';

    // Hide welcome screen
    if (welcomeScreen) {
        welcomeScreen.style.display = 'none';
    }

    // Re-render all messages from the session
    session.messages.forEach(msg => {
        addMessageToUI(msg.role, msg.content);
    });

    renderChatHistory();

    // Close sidebar on mobile
    closeSidebar();
}

// Load history on page load
loadChatHistory();

// ============================
// 15. New Chat
// ============================
btnNewChat.addEventListener('click', () => {
    currentSessionId = null;
    currentMessages = [];

    // Clear the chat area and show welcome screen
    chatMessages.innerHTML = '';
    if (welcomeScreen) {
        chatMessages.appendChild(welcomeScreen);
        welcomeScreen.style.display = '';
    }

    renderChatHistory();
    chatInput.focus();

    // Close sidebar on mobile
    closeSidebar();
});

// ============================
// 16. Suggestion Cards
// When user clicks a suggestion, auto-send that prompt
// ============================
if (suggestions) {
    suggestions.addEventListener('click', (e) => {
        const card = e.target.closest('.suggestion-card');
        if (!card) return;

        const prompt = card.getAttribute('data-prompt');
        if (prompt) {
            chatInput.value = prompt;
            chatInput.dispatchEvent(new Event('input')); // Trigger input handlers
            sendMessage();
        }
    });
}

// ============================
// 17. Mobile Sidebar Toggle
// ============================
function closeSidebar() {
    sidebar.classList.remove('open');
    sidebarOverlay.classList.remove('active');
}

sidebarToggle.addEventListener('click', () => {
    sidebar.classList.toggle('open');
    sidebarOverlay.classList.toggle('active');
});

sidebarOverlay.addEventListener('click', closeSidebar);
