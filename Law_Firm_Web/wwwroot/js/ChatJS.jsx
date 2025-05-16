// Improved chat.js with better error handling and reliability

// Create a single, reusable connection
let connection = null;
let reconnectAttempts = 0;
const maxReconnectAttempts = 5;

// Initialize the chat system
function initializeChat(currentUserId, receiverId) {
    // Create connection if it doesn't exist
    if (!connection) {
        try {
            connection = new signalR.HubConnectionBuilder()
                .withUrl("/chatHub")
                .withAutomaticReconnect([0, 2000, 5000, 10000, 20000]) // More refined reconnection timing
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // Set up event handlers
            setupSignalREventHandlers(currentUserId);

            // Start the connection
            startConnection();

            // Store receiver info for convenience
            sessionStorage.setItem('currentChatReceiverId', receiverId);
        } catch (error) {
            console.error("Error initializing chat:", error);
            showChatError("Failed to initialize chat. Please try refreshing the page.");
        }
    } else if (connection.state === signalR.HubConnectionState.Disconnected) {
        // If we have a connection but it's disconnected, reconnect
        startConnection();
    }
}

// Set up SignalR event handlers
function setupSignalREventHandlers(currentUserId) {
    // Handle direct messages
    connection.on("ReceiveMessage", (senderId, senderName, message) => {
        appendMessage(senderId, senderName, message, senderId === currentUserId);

        // Update unread status in the UI if needed
        updateUnreadStatus(senderId);
    });

    // Handle group messages
    connection.on("ReceiveGroupMessage", (senderId, senderName, groupName, message) => {
        appendGroupMessage(senderId, senderName, groupName, message, senderId === currentUserId);
    });

    // Handle user joined notification
    connection.on("UserJoined", (userId, userName, groupName) => {
        appendNotification(`${userName} joined the conversation`);
    });

    // Handle user left notification
    connection.on("UserLeft", (userId, userName, groupName) => {
        appendNotification(`${userName} left the conversation`);
    });

    // Handle connection state changes
    connection.onreconnecting((error) => {
        console.log("Attempting to reconnect:", error);
        appendNotification("Connection lost. Reconnecting...");
    });

    connection.onreconnected((connectionId) => {
        console.log("Reconnected with ID:", connectionId);
        appendNotification("Connection restored.");
        sessionStorage.setItem('signalrConnectionId', connectionId);
    });

    connection.onclose((error) => {
        console.log("Connection closed:", error);
        appendNotification("Connection closed. Please refresh the page to reconnect.");
    });
}

// Start the SignalR connection
function startConnection() {
    connection.start()
        .then(() => {
            console.log("Connected to SignalR hub");
            // Reset reconnect attempts on successful connection
            reconnectAttempts = 0;
            // Store the connection ID if needed for groups
            sessionStorage.setItem('signalrConnectionId', connection.connectionId);

            // If there was a visible error message, clear it
            clearChatError();
        })
        .catch(err => {
            console.error("Error connecting to SignalR hub:", err);
            reconnectAttempts++;

            if (reconnectAttempts <= maxReconnectAttempts) {
                // Try to reconnect after a delay with exponential backoff
                const delay = Math.min(1000 * Math.pow(2, reconnectAttempts), 30000);
                console.log(`Reconnect attempt ${reconnectAttempts} in ${delay}ms`);
                setTimeout(startConnection, delay);
            } else {
                console.error("Maximum reconnection attempts reached");
                showChatError("Unable to connect to chat. Please refresh the page.");
            }
        });
}

// Send a direct message
async function sendMessage(message, receiverId) {
    if (!message || !message.trim()) {
        console.warn("Attempted to send empty message");
        return;
    }

    if (!receiverId) {
        console.error("No receiver ID specified");
        return;
    }

    // First, optimistically update the UI
    const currentUserId = getCurrentUserId();
    appendMessage(currentUserId, "You", message, true, true);

    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
        console.warn("Connection not established, using API fallback");
        sendMessageViaApi(message, receiverId);
        return;
    }

    try {
        // Send via SignalR hub
        await connection.invoke("SendMessage", message, receiverId);
    } catch (err) {
        console.error("Error sending message via SignalR:", err);

        // Remove optimistic message if it was marked as pending
        removeLastPendingMessage();

        // Fallback to API
        sendMessageViaApi(message, receiverId);
    }
}

// API fallback for sending messages
async function sendMessageViaApi(message, receiverId) {
    try {
        const response = await fetch('/api/Chat/sendmessage', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({
                message: message,
                receiverId: receiverId
            })
        });

        if (!response.ok) {
            throw new Error(`API returned status: ${response.status}`);
        }

        // Update UI to show message was sent successfully
        updateLastPendingMessageStatus(true);
    } catch (apiErr) {
        console.error("API fallback also failed:", apiErr);
        // Mark message as failed in UI
        updateLastPendingMessageStatus(false);
        showChatError("Failed to send message. Please try again.");
    }
}

// Send a message to a group
async function sendMessageToGroup(groupName, message) {
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
        console.error("Cannot send message: connection not established");
        showChatError("Connection not established. Please refresh the page.");
        return;
    }

    try {
        await connection.invoke("SendMessageToGroup", groupName, message);
    } catch (err) {
        console.error("Error sending group message:", err);

        // Fallback to API
        try {
            await fetch('/api/Chat/sendmessagetogroup', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    groupName: groupName,
                    message: message
                })
            });
        } catch (apiErr) {
            console.error("API fallback also failed:", apiErr);
            showChatError("Failed to send group message. Please try again.");
        }
    }
}

// Join a group
async function joinGroup(groupName) {
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
        console.error("Cannot join group: connection not established");
        return;
    }

    try {
        await connection.invoke("JoinGroup", groupName);
    } catch (err) {
        console.error("Error joining group:", err);

        // Fallback to API
        try {
            await fetch('/api/Chat/joingroup', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    groupName: groupName,
                    connectionId: connection.connectionId
                })
            });
        } catch (apiErr) {
            console.error("API fallback also failed:", apiErr);
        }
    }
}

// Leave a group
async function leaveGroup(groupName) {
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
        console.error("Cannot leave group: connection not established");
        return;
    }

    try {
        await connection.invoke("LeaveGroup", groupName);
    } catch (err) {
        console.error("Error leaving group:", err);

        // Fallback to API
        try {
            await fetch('/api/Chat/leavegroup', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    groupName: groupName,
                    connectionId: connection.connectionId
                })
            });
        } catch (apiErr) {
            console.error("API fallback also failed:", apiErr);
        }
    }
}

// Append a direct message to the chat window
function appendMessage(senderId, senderName, message, isSent, isPending = false) {
    const chatMessages = document.getElementById('chat-messages');
    if (!chatMessages) return;

    const messageDiv = document.createElement('div');
    messageDiv.className = `chat-message ${isSent ? 'sent' : 'received'} ${isPending ? 'pending' : ''}`;
    if (isPending) {
        messageDiv.dataset.pending = "true";
    }

    const now = new Date();
    const timeStr = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

    messageDiv.innerHTML = `
        <div class="message-header">
            <strong>${isSent ? 'You' : senderName || senderId}</strong>
            <span class="timestamp">${timeStr}</span>
            ${isPending ? '<span class="status-indicator">Sending...</span>' : ''}
        </div>
        <div class="message-content">${message}</div>
    `;

    chatMessages.appendChild(messageDiv);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

// Append a group message to the chat window
function appendGroupMessage(senderId, senderName, groupName, message, isSent) {
    const chatMessages = document.getElementById('chat-messages');
    if (!chatMessages) return;

    const messageDiv = document.createElement('div');
    messageDiv.className = `chat-message group-message ${isSent ? 'sent' : 'received'}`;

    const now = new Date();
    const timeStr = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

    messageDiv.innerHTML = `
        <div class="message-header">
            <strong>${isSent ? 'You' : senderName || senderId}</strong>
            <span class="group-name">${groupName}</span>
            <span class="timestamp">${timeStr}</span>
        </div>
        <div class="message-content">${message}</div>
    `;

    chatMessages.appendChild(messageDiv);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

// Append a notification message
function appendNotification(message) {
    const chatMessages = document.getElementById('chat-messages');
    if (!chatMessages) return;

    const notificationDiv = document.createElement('div');
    notificationDiv.className = 'chat-notification';
    notificationDiv.textContent = message;

    chatMessages.appendChild(notificationDiv);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

// Show chat error message
function showChatError(message) {
    // Look for existing error div
    let errorDiv = document.getElementById('chat-error');

    // Create if it doesn't exist
    if (!errorDiv) {
        errorDiv = document.createElement('div');
        errorDiv.id = 'chat-error';
        errorDiv.className = 'chat-error';

        // Insert at top of chat container
        const chatContainer = document.getElementById('chat-container');
        if (chatContainer) {
            chatContainer.insertBefore(errorDiv, chatContainer.firstChild);
        }
    }

    errorDiv.textContent = message;
    errorDiv.style.display = 'block';
}

// Clear chat error message
function clearChatError() {
    const errorDiv = document.getElementById('chat-error');
    if (errorDiv) {
        errorDiv.style.display = 'none';
    }
}

// Remove last pending message
function removeLastPendingMessage() {
    const chatMessages = document.getElementById('chat-messages');
    if (!chatMessages) return;

    const pendingMessages = chatMessages.querySelectorAll('.chat-message.pending');
    if (pendingMessages.length > 0) {
        // Remove the last pending message
        const lastPending = pendingMessages[pendingMessages.length - 1];
        chatMessages.removeChild(lastPending);
    }
}

// Update status of the last pending message
function updateLastPendingMessageStatus(success) {
    const chatMessages = document.getElementById('chat-messages');
    if (!chatMessages) return;

    const pendingMessages = chatMessages.querySelectorAll('.chat-message.pending');
    if (pendingMessages.length > 0) {
        // Update the last pending message
        const lastPending = pendingMessages[pendingMessages.length - 1];

        // Remove pending class
        lastPending.classList.remove('pending');

        if (success) {
            // Update status indicator if it exists
            const statusIndicator = lastPending.querySelector('.status-indicator');
            if (statusIndicator) {
                statusIndicator.textContent = 'Sent';

                // Make it fade out after 2 seconds
                setTimeout(() => {
                    statusIndicator.style.opacity = '0';
                    setTimeout(() => {
                        statusIndicator.remove();
                    }, 500);
                }, 2000);
            }
        } else {
            // Mark as failed
            lastPending.classList.add('failed');

            // Update status indicator if it exists
            const statusIndicator = lastPending.querySelector('.status-indicator');
            if (statusIndicator) {
                statusIndicator.textContent = 'Failed';
                statusIndicator.className = 'status-indicator error';
            }
        }
    }
}

// Update unread status in contacts list
function updateUnreadStatus(senderId) {
    // Find contact in list
    const contactItem = document.querySelector(`[data-contact-id="${senderId}"]`);
    if (contactItem) {
        // Add unread indicator or update count
        let unreadBadge = contactItem.querySelector('.unread-badge');
        if (!unreadBadge) {
            unreadBadge = document.createElement('span');
            unreadBadge.className = 'unread-badge';
            unreadBadge.textContent = '1';
            contactItem.appendChild(unreadBadge);
        } else {
            // Increment count
            const currentCount = parseInt(unreadBadge.textContent, 10) || 0;
            unreadBadge.textContent = (currentCount + 1).toString();
        }
    }
}

// Helper to get current user ID from the container
function getCurrentUserId() {
    const chatContainer = document.getElementById('chat-container');
    return chatContainer ? chatContainer.dataset.currentUser : null;
}

// Helper to get current receiver ID
function getCurrentReceiverId() {
    const chatContainer = document.getElementById('chat-container');
    return chatContainer ? chatContainer.dataset.receiver : sessionStorage.getItem('currentChatReceiverId');
}

// Check if we need to load more messages (for pagination)
function setupMessageScroll() {
    const chatMessages = document.getElementById('chat-messages');
    if (!chatMessages) return;

    // Add scroll event listener for infinite scroll
    chatMessages.addEventListener('scroll', function () {
        // If we're at the top, load more messages
        if (chatMessages.scrollTop === 0) {
            loadMoreMessages();
        }
    });
}

// Load more messages (for pagination)
async function loadMoreMessages() {
    const currentUserId = getCurrentUserId();
    const receiverId = getCurrentReceiverId();
    const chatMessages = document.getElementById('chat-messages');

    if (!currentUserId || !receiverId || !chatMessages) return;

    // Get the oldest message ID
    const firstMessage = chatMessages.querySelector('.chat-message');
    if (!firstMessage) return;

    // Get the ID of the oldest message (would need to be added to your data attributes)
    const oldestMessageId = firstMessage.dataset.messageId;
    if (!oldestMessageId) return;

    try {
        // Show loading indicator
        const loadingDiv = document.createElement('div');
        loadingDiv.className = 'chat-notification loading';
        loadingDiv.textContent = 'Loading earlier messages...';
        chatMessages.insertBefore(loadingDiv, chatMessages.firstChild);

        // Get previous messages
        const response = await fetch(`/api/Chat/messages/${receiverId}?before=${oldestMessageId}`);
        if (!response.ok) {
            throw new Error('Failed to load more messages');
        }

        const olderMessages = await response.json();

        // Remove loading indicator
        chatMessages.removeChild(loadingDiv);

        // Get current scroll height
        const scrollHeightBefore = chatMessages.scrollHeight;

        // Add older messages to the top
        olderMessages.forEach(msg => {
            const messageDiv = document.createElement('div');
            messageDiv.className = `chat-message ${msg.senderId === currentUserId ? 'sent' : 'received'}`;
            messageDiv.dataset.messageId = msg.id;

            messageDiv.innerHTML = `
                <div class="message-header">
                    <strong>${msg.senderId === currentUserId ? 'You' : msg.senderName}</strong>
                    <span class="timestamp">${new Date(msg.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
                </div>
                <div class="message-content">${msg.message}</div>
            `;

            chatMessages.insertBefore(messageDiv, chatMessages.firstChild);
        });

        // Maintain scroll position
        const newScrollHeight = chatMessages.scrollHeight;
        chatMessages.scrollTop = newScrollHeight - scrollHeightBefore;
    } catch (error) {
        console.error('Error loading more messages:', error);

        // Remove loading indicator if it exists
        const loadingDiv = document.querySelector('.chat-notification.loading');
        if (loadingDiv) {
            chatMessages.removeChild(loadingDiv);
        }
    }
}

// Export functions for use in other scripts
window.chatFunctions = {
    initializeChat,
    sendMessage,
    sendMessageToGroup,
    joinGroup,
    leaveGroup,
    loadMoreMessages
};