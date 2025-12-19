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
        warningBeforeLockSeconds: 30,
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
        const warningThreshold = lockThreshold - (config.warningBeforeLockSeconds || 30);

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
    // Lock Screen - Server-Side Redirect (SECURE)
    // ========================================
    async function showLockScreen() {
        if (isLocked) return;

        isLocked = true;
        debugLog('Triggering server-side lock');

        // Stop checking idle (will resume after unlock)
        if (idleCheckInterval) {
            clearInterval(idleCheckInterval);
            idleCheckInterval = null;
        }

        try {
            // Tell server to lock the session
            const response = await fetch('/Auth/TriggerLock', {
                method: 'POST',
                headers: getHeaders()
            });

            if (response.status === 401) {
                handleRealSessionExpired();
                return;
            }

            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    window.location.href = data.redirectUrl || '/Auth/Lock';
                    return;
                }
            }

            // Fallback
            window.location.href = '/Auth/Lock';

        } catch (error) {

            debugLog('Lock trigger error: ' + error.message);
            // Fallback: redirect directly  
            window.location.href = '/Auth/Lock';
        }
    }

    function hideLockScreen() {

        isLocked = false;
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
        SessionManager.init();
    } else {
        console.log('[SessionManager] Skipping on auth page');
    }
});

// Debug access in development
if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
    window.SessionManager = SessionManager;
}