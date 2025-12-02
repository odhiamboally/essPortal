/**
 * Unified Session Manager - FIXED VERSION
 * 
 * Key changes:
 * 1. Removed mousemove from activity tracking (too sensitive)
 * 2. Added debouncing to activity tracking
 * 3. KeepAlive is sent based on user activity, not just time
 * 4. Session status now reflects actual cookie expiration
 */

const SessionManager = (function () {
    'use strict';

    let config = {
        sessionTimeoutMinutes: 15,
        idleTimeoutMinutes: 5,
        warningThresholdSeconds: 30,
        criticalThresholdSeconds: 10,
        keepAliveIntervalSeconds: 30,
        checkIntervalSeconds: 15,
        useLockScreen: true,
        debugMode: false
    };

    // State
    let lastActivity = Date.now();
    let isLocked = false;
    let isWarningShown = false;
    let idleCheckInterval = null;
    let keepAliveInterval = null;
    let warningCountdownInterval = null;
    let activityDebounceTimer = null;

    // ========================================
    // Utility Functions
    // ========================================

    function debugLog(message) {
        if (config.debugMode) {
            console.log('[SessionManager] ' + new Date().toISOString() + ' - ' + message);
            //console.log('[SessionManager] ' + new Date().toLocaleTimeString() + ' - ' + message);
        }
    }

    function getCSRFToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
            document.querySelector('meta[name="__RequestVerificationToken"]')?.content || 
            window.csrfToken || '';
    }

    function getHeaders() {
        const headers = {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        };
        const token = getCSRFToken();
        if (token) {
            headers['X-CSRF-TOKEN'] = token;
            headers['RequestVerificationToken'] = token;
        }
        return headers;
    }

    function formatTime(seconds) {
        const mins = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    }

    function showNotification(message, type) {

        if (typeof window.showToast === 'function') {

            window.showToast(message, type === 'success' ? 'success' : 'danger');

        } else if (typeof Swal !== 'undefined') {

            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: type === 'success' ? 'success' : 'error',
                title: message,
                showConfirmButton: false,
                timer: 3000
            });
        } else {
            console.log('[SessionManager] ' + type + ': ' + message);
        }
    }

    // ========================================
    // User Info (for lock screen display)
    // ========================================
    function getUserInfo() {
        // Try multiple sources for user info
        const userName =
            document.querySelector('[data-user-name]')?.dataset.userName ||
            document.querySelector('.user-display-name')?.textContent?.trim() ||
            document.querySelector('.user-name')?.textContent?.trim() ||
            window.currentUser?.name ||
            'User';

        let userInitials =
            document.querySelector('[data-user-initials]')?.dataset.userInitials ||
            document.querySelector('.user-initials')?.textContent?.trim() ||
            window.currentUser?.initials;

        // Generate initials from name if not found
        if (!userInitials && userName !== 'User') {
            const parts = userName.split(' ').filter(p => p.length > 0);
            if (parts.length >= 2) {
                userInitials = (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
            } else if (parts.length === 1) {
                userInitials = parts[0].substring(0, 2).toUpperCase();
            }
        }

        return {
            name: userName,
            initials: userInitials || 'U'
        };
    }

    // ========================================
    // Activity Tracking
    // ========================================

    function trackActivity() {

        // Don't track if locked
        if (isLocked) return;

        // Debounce: only register activity once per second
        if (activityDebounceTimer) return;

        activityDebounceTimer = setTimeout(() => {
            activityDebounceTimer = null;
        }, 1000);

        lastActivity = Date.now();
        debugLog('User activity detected');

        // If warning is showing and user is active, hide it
        if (isWarningShown) {
            hideWarning();
        }
        
    }

    function getIdleTimeSeconds() {
        return (Date.now() - lastActivity) / 1000;
    }

    // ========================================
    // Idle Check (Client-Side Timer)
    // ========================================

    function checkIdle() {
        if (isLocked) return;

        const idleSeconds = getIdleTimeSeconds();
        const lockThreshold = config.idleTimeoutMinutes * 60;
        const warningThreshold = lockThreshold - config.warningBeforeLockSeconds;

        debugLog(`Idle: ${Math.floor(idleSeconds)}s / Lock at: ${lockThreshold}s`);

        // Time to show lock screen
        if (idleSeconds >= lockThreshold) {
            debugLog('Idle timeout reached - showing lock screen');
            hideWarning();
            showLockScreen();
            return;
        }

        // Time to show warning
        if (idleSeconds >= warningThreshold && !isWarningShown) {
            const timeUntilLock = Math.floor(lockThreshold - idleSeconds);
            showWarning(timeUntilLock);
        }
    }

    // ========================================
    // Keep-Alive (Keep Session Cookie Alive)
    // ========================================

    async function sendKeepAlive() {

        try {

            debugLog('Sending keep-alive...');

            const response = await fetch('/Auth/KeepAlive', {
                method: 'POST',
                headers: getHeaders()
            });

            if (response.status === 401) {
                debugLog('Session expired during lock');
                handleRealSessionExpired();
                return;
            }

            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    debugLog('Keep-alive successful - session extended');
                    
                }
                else {
                    debugLog('Keep-alive failed: ' + data.message);
                }
            }
        } catch (error) {
            debugLog('Keep-alive error: ' + error.message);
        }

    }

    // ========================================
    // Warning Modal
    // ========================================
    function showWarning(secondsUntilLock) {

        if (isWarningShown || isLocked) return;

        isWarningShown = true;

        debugLog('Showing warning: ' + secondsUntilLock + 's until lock');

        if (typeof Swal !== 'undefined') {

            Swal.fire({
                title: '⏱️ Inactivity Warning',
                html: `
                    <div style="text-align: center;">
                        <p>Your screen will lock in:</p>
                        <div id="warning-countdown" style="font-size: 2.5em; font-weight: bold; color: #ffc107; margin: 15px 0;">
                            ${formatTime(secondsUntilLock)}
                        </div>
                        <p>Click <strong>Continue</strong> to stay logged in.</p>
                    </div>
                `,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: '✓ Continue Session',
                cancelButtonText: 'Logout',
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                allowOutsideClick: false,
                allowEscapeKey: false,

                didOpen: () => {

                    const countdownElement = document.getElementById('warning-countdown');

                    warningCountdownInterval = setInterval(() => {
                        remainingSeconds--;

                        if (countdownElement) {
                            countdownElement.textContent = formatTime(remainingSeconds);

                            // Turn red when critical
                            if (remainingSeconds <= 10) {
                                countdownElement.style.color = '#dc3545';
                            }
                        }

                        // Lock when countdown reaches 0
                        if (remainingSeconds <= 0) {
                            Swal.close();
                            showLockScreen();
                        }

                    }, 1000);

                },

                willClose: () => {

                    if (warningCountdownInterval) {
                        clearInterval(warningCountdownInterval);
                        warningCountdownInterval = null;
                    }
                }

            }).then((result) => {

                isWarningShown = false;

                if (result.isConfirmed) {

                    // User clicked "I'm Here" - reset activity
                    trackActivity();

                } else if (result.dismiss === Swal.DismissReason.cancel) {

                    // User clicked "Logout"
                    logout();
                }
                // If dismissed by clicking outside or pressing Escape, also treat as activity
                else if (result.dismiss === Swal.DismissReason.backdrop ||
                    result.dismiss === Swal.DismissReason.esc) {
                    trackActivity();
                }

            });

        } else {

            // Fallback for no SweetAlert
            const stayActive = confirm(`Session expires in ${formatTime(secondsUntilLock)}. Stay logged in?`);

            isWarningShown = false;

            if (stayActive) {

                trackActivity();

            } else {

                logout();
            }
        }

    }

    function hideWarning() {

        if (!isWarningShown) return;

        isWarningShown = false;

        if (warningCountdownInterval) {
            clearInterval(warningCountdownInterval);
            warningCountdownInterval = null;
        }

        if (typeof Swal !== 'undefined') {
            Swal.close();
        }

        debugLog('Warning hidden - user is active');
    }

    // ========================================
    // Lock Screen (Pure Overlay)
    // ========================================
    function showLockScreen() {

        if (isLocked) return;

        isLocked = true;

        debugLog('Showing lock screen');

        const userInfo = getUserInfo();

        // Create overlay
        const lockOverlay = document.createElement('div');
        lockOverlay.id = 'session-lock-overlay';
        lockOverlay.innerHTML = `
            <style>
                #session-lock-overlay {
                    position: fixed;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    z-index: 999999;
                    animation: lockFadeIn 0.3s ease;
                }
                @keyframes lockFadeIn {
                    from { opacity: 0; }
                    to { opacity: 1; }
                }
                @keyframes lockFadeOut {
                    from { opacity: 1; }
                    to { opacity: 0; }
                }
                .lock-content {
                    background: rgba(255, 255, 255, 0.95);
                    padding: 40px 50px;
                    border-radius: 16px;
                    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
                    text-align: center;
                    max-width: 400px;
                    width: 90%;
                }
                .lock-avatar {
                    width: 80px;
                    height: 80px;
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    border-radius: 50%;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    margin: 0 auto 20px;
                    font-size: 32px;
                    color: white;
                    font-weight: 600;
                }
                .lock-content h2 {
                    margin: 0 0 8px 0;
                    color: #1a1a2e;
                    font-size: 24px;
                    font-weight: 600;
                }
                .lock-user-name {
                    color: #666;
                    margin-bottom: 8px;
                    font-size: 16px;
                }
                .lock-message {
                    color: #888;
                    margin-bottom: 24px;
                    font-size: 14px;
                }
                .lock-input-group {
                    margin-bottom: 20px;
                }
                .lock-input {
                    width: 100%;
                    padding: 14px 16px;
                    border: 2px solid #e0e0e0;
                    border-radius: 8px;
                    font-size: 16px;
                    transition: border-color 0.2s, box-shadow 0.2s;
                    box-sizing: border-box;
                }
                .lock-input:focus {
                    outline: none;
                    border-color: #667eea;
                    box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
                }
                .lock-input.error {
                    border-color: #dc3545;
                    animation: shake 0.5s ease;
                }
                @keyframes shake {
                    0%, 100% { transform: translateX(0); }
                    25% { transform: translateX(-5px); }
                    75% { transform: translateX(5px); }
                }
                .lock-error {
                    color: #dc3545;
                    font-size: 13px;
                    margin-top: 8px;
                    display: none;
                    text-align: left;
                }
                .lock-error.visible {
                    display: block;
                }
                .lock-btn {
                    width: 100%;
                    padding: 14px;
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    color: white;
                    border: none;
                    border-radius: 8px;
                    font-size: 16px;
                    font-weight: 600;
                    cursor: pointer;
                    transition: transform 0.2s, box-shadow 0.2s;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    gap: 8px;
                }
                .lock-btn:hover:not(:disabled) {
                    transform: translateY(-2px);
                    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
                }
                .lock-btn:disabled {
                    opacity: 0.7;
                    cursor: not-allowed;
                    transform: none;
                }
                .lock-spinner {
                    display: none;
                    width: 18px;
                    height: 18px;
                    border: 2px solid rgba(255,255,255,0.3);
                    border-top-color: white;
                    border-radius: 50%;
                    animation: spin 0.8s linear infinite;
                }
                .lock-btn.loading .lock-spinner {
                    display: block;
                }
                @keyframes spin {
                    to { transform: rotate(360deg); }
                }
                .lock-footer {
                    margin-top: 24px;
                    padding-top: 20px;
                    border-top: 1px solid #eee;
                }
                .lock-switch-link {
                    color: #667eea;
                    text-decoration: none;
                    font-size: 14px;
                    font-weight: 500;
                    cursor: pointer;
                }
                .lock-switch-link:hover {
                    text-decoration: underline;
                }
            </style>
            <div class="lock-content">
                <div class="lock-avatar">${userInfo.initials}</div>
                <h2>Screen Locked</h2>
                <p class="lock-user-name">${userInfo.name}</p>
                <p class="lock-message">Enter your password to unlock</p>
                <div class="lock-input-group">
                    <input type="password" 
                           id="lock-password" 
                           class="lock-input" 
                           placeholder="Password" 
                           autocomplete="current-password" />
                    <div id="lock-error" class="lock-error"></div>
                </div>
                <button type="button" id="lock-unlock-btn" class="lock-btn">
                    <span class="lock-spinner"></span>
                    <span>Unlock</span>
                </button>
                <div class="lock-footer">
                    <a id="lock-signout-link" class="lock-switch-link">Sign out and use different account</a>
                </div>
            </div>
        `;

        document.body.appendChild(lockOverlay);

        // Setup event handlers
        const passwordInput = document.getElementById('lock-password');
        const unlockBtn = document.getElementById('lock-unlock-btn');
        const signoutLink = document.getElementById('lock-signout-link');

        // Focus password input
        setTimeout(() => passwordInput?.focus(), 100);

        // Handle Enter key
        passwordInput?.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                handleUnlock();
            }
        });

        // Handle unlock button
        unlockBtn?.addEventListener('click', handleUnlock);

        // Handle sign out
        signoutLink?.addEventListener('click', (e) => {
            e.preventDefault();
            logout();
        });

        debugLog('Lock screen shown - keep-alive will continue');

    }

    function hideLockScreen() {

        const lockOverlay = document.getElementById('session-lock-overlay');
        if (lockOverlay) {
            lockOverlay.style.animation = 'fadeOut 0.3s ease';
            setTimeout(() => lockOverlay.remove(), 300);
        }

        isLocked = false;
        lastActivity = Date.now();

        debugLog('Lock screen hidden');
    }

    // ========================================
    // Unlock Handler
    // ========================================

    async function handleUnlock(e) {

        e.preventDefault();

        const passwordInput = document.getElementById('lock-password');
        const unlockBtn = document.getElementById('lock-unlock-btn');
        const errorDiv = document.getElementById('lock-error');
        const password = passwordInput?.value;

        if (!password) {

            passwordInput.classList.add('error');

            if (errorDiv) {
                errorDiv.textContent = 'Please enter your password';
                errorDiv.classList.add('visible');
            }

            return;
        }

        // Show loading state
        if (unlockBtn) {
            unlockBtn.disabled = true;
            unlockBtn.classList.add('loading');
        }

        passwordInput?.classList.remove('error');
        errorDiv?.classList.remove('visible');

        try {

            // Session is still alive

            const response = await fetch('/Auth/Unlock', {
                method: 'POST',
                headers: getHeaders(),
                body: JSON.stringify({ Password: password })
            });

            // If we get 401, session actually expired (user was locked 15+ minutes)
            if (response.status === 401) {
                handleRealSessionExpired();
                return;
            }

            const data = await response.json();

            if (response.ok && data.success) {

                debugLog('Unlock successful');
                hideLockScreen();
                showNotification('Welcome back!', 'success');

            } else {

                // Show error
                passwordInput.classList.add('error');

                if (errorDiv) {
                    errorDiv.textContent = data.message || 'Invalid password';
                    errorDiv.classList.add('visible');
                }

                if (passwordInput) {
                    passwordInput.value = '';
                    passwordInput.focus();
                }

                // Check if too many attempts
                if (data.locked) {
                    errorDiv.textContent = 'Account locked. Signing out...';
                    setTimeout(() => logout(), 2000);
                }
            }
        } catch (error) {

            debugLog('Unlock error: ' + error.message);

            if (errorDiv) {
                errorDiv.textContent = 'Connection error. Please try again.';
                errorDiv.classList.add('visible');
            }

        } finally {

            if (unlockBtn) {
                unlockBtn.disabled = false;
                unlockBtn.classList.remove('loading');
            }
        }
    }

    // ========================================
    // Real Session Expiration
    // ========================================
    function handleRealSessionExpired() {

        // This happens if user was locked for 30+ minutes (ESS_Auth cookie expired)
        debugLog('Real session expired - must re-login');

        // Remove lock screen
        const overlay = document.getElementById('session-lock-overlay');
        if (overlay) overlay.remove();

        // Show message and redirect
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: 'Session Expired',
                text: 'Your session has expired. Please sign in again. SM',
                icon: 'info',
                confirmButtonText: 'Sign In',
                allowOutsideClick: false
            }).then(() => {
                redirectToLogin();
            });
        } else {
            alert('Your session has expired. Please sign in again. SM');
            redirectToLogin();
        }
    }

    function redirectToLogin(reason) {

        debugLog('Redirecting to login: ' + reason);

        // Store current URL for return after login
        const currentUrl = window.location.pathname + window.location.search;

        if (currentUrl !== '/' && !currentUrl.toLowerCase().includes('/auth/')) {
            sessionStorage.setItem('returnUrl', currentUrl);
        }

        window.location.href = '/Auth/SignIn?sessionExpired=true';
    }

    function logout() {

        debugLog('Logging out');

        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '/Auth/SignOut';

        const token = getCSRFToken();
        if (token) {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = '__RequestVerificationToken';
            input.value = token;
            form.appendChild(input);
        }

        document.body.appendChild(form);
        form.submit();
    }

    // ========================================
    // Configuration Loading
    // ========================================

    async function loadConfig() {
        try {
            const response = await fetch('/Auth/SessionConfig', {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            if (response.ok) {

                const serverConfig = await response.json();

                // Map server config to our config
                if (serverConfig.idleTimeoutMinutes) {
                    config.idleTimeoutMinutes = serverConfig.idleTimeoutMinutes;
                } 

                if (serverConfig.warningBeforeLockSeconds) {
                    config.warningBeforeLockSeconds = serverConfig.warningBeforeLockSeconds;
                }

                if (typeof serverConfig.useLockScreen !== 'undefined') {
                    config.useLockScreen = serverConfig.useLockScreen;
                }

                if (typeof serverConfig.debugMode !== 'undefined') {
                    config.debugMode = serverConfig.debugMode;
                }

                debugLog('Config loaded: idleTimeout=' + config.idleTimeoutMinutes + 'min');
            }
        } catch (error) {
            debugLog('Failed to load config: ' + error.message);
        }
    }


    // ========================================
    // Initialization
    // ========================================

    async function initialize() {

        debugLog('Initializing session manager...');

        // Load configuration from server
        await loadConfig();

        if (!config.useLockScreen) {
            debugLog('Lock screen disabled');
            return;
        }

        // Track user activity
        const events = ['mousedown', 'keypress', 'keydown', 'scroll', 'touchstart', 'click'];

        events.forEach(event => {
            document.addEventListener(event, trackActivity, { passive: true });
        });

        // Track tab visibility (user returns to tab)
        document.addEventListener('visibilitychange', () => {

            if (!document.hidden && !isLocked) {
                trackActivity();
            }
        });

        // Check idle status every 10 seconds
        idleCheckInterval = setInterval(checkIdle, 10 * 1000);

        // Send keep-alive to keep session alive (even when locked!)
        keepAliveInterval = setInterval(sendKeepAlive, config.keepAliveIntervalSeconds * 1000);

        // Initial activity
        trackActivity();

        // Cleanup on page unload
        window.addEventListener('beforeunload', stop);

        debugLog('Initialized - idle timeout: ' + config.idleTimeoutMinutes + ' min');
    }

    function stop() {

        if (idleCheckInterval) clearInterval(idleCheckInterval);
        if (keepAliveInterval) clearInterval(keepAliveInterval);
        if (warningCountdownInterval) clearInterval(warningCountdownInterval);
        
        debugLog('Session manager stopped');
    }

    // Public API
    return {
        init: initialize,
        stop: stop,
        logout: logout,

        // State
        isLocked: () => isLocked,
        isWarningShown: () => isWarningShown,
        getIdleTime: () => Math.floor(getIdleTimeSeconds()),

        // Debug
        getConfig: () => ({ ...config }),

        sendKeepAlive: sendKeepAlive,

        // Manual controls (for testing)
        lock: showLockScreen,
        unlock: hideLockScreen,

    };

})();

// ========================================
// Auto-initialize - on authenticated pages
// ========================================
document.addEventListener('DOMContentLoaded', () => {

    const path = window.location.pathname.toLowerCase();
    const isAuthPage = path.includes('/auth/');

    if (!isAuthPage) {
        console.log('[SessionManager] Initializing...');
        SessionManager.init();
    } else {
        console.log('[SessionManager] Skipping on auth page');
    }
});

// Debug access in development
if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
    window.SessionManager = SessionManager;
}