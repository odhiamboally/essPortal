// Complete 2FA Modal JavaScript with debugging
console.log('2FA Modal JS loaded');



// This works even if the #twoFactorToggle element is added to the page later.
document.addEventListener('change', function (event) {
    // Check if the element that triggered the event is our toggle switch
    if (event.target.id === 'twoFactorToggle') {
        // If it is, call our handler function
        handleToggleChange(event);
    }
});

async function handleToggleChange(event) {
    console.log('🔄 Toggle changed!', event.target.checked);

    const toggle = event.target;
    const isCurrentlyEnabled = toggle.getAttribute('data-enabled') === 'true';
    const shouldEnable = toggle.checked;

    console.log('Current state:', isCurrentlyEnabled, 'Should enable:', shouldEnable);

    // If user is trying to enable 2FA
    if (!isCurrentlyEnabled && shouldEnable) {
        console.log('👆 Enabling 2FA...');
        try {
            showLoading('Preparing setup...');

            const token = getCSRFToken();
            console.log('CSRF Token found:', !!token);

            if (!token) {
                throw new Error('CSRF token not found');
            }

            const url = toggle.dataset.enableUrl;
            console.log('Making request to:', url);

            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: `__RequestVerificationToken=${encodeURIComponent(token)}`
            });

            console.log('Response status:', response.status);
            console.log('Response ok:', response.ok);

            if (!response.ok) {
                const errorText = await response.text();
                console.error('Response error:', errorText);
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            console.log('Response data:', data);

            if (data.success && data.setupRequired) {
                console.log('✅ Setup required, redirecting...');
                // Close modal and redirect to setup
                const modalInstance = bootstrap.Modal.getInstance(document.getElementById('twoFactorSettingsModal'));
                if (modalInstance) {
                    modalInstance.hide();
                }

                setTimeout(() => {
                    window.location.href = data.setupUrl || '/TwoFactor/Setup';
                }, 300);

                return; // Don't revert toggle - user is going to setup
            } else if (data.success) {
                console.log('✅ 2FA enabled successfully');
                showToast(data.message || '2FA enabled successfully', 'success');
                toggle.setAttribute('data-enabled', 'true');
            } else {
                console.error('❌ Enable failed:', data.message);
                throw new Error(data.message || 'Failed to enable 2FA');
            }

        } catch (error) {
            console.error('❌ Error enabling 2FA:', error);
            showToast(error.message || 'Failed to enable 2FA. Please try again.', 'error');
            toggle.checked = false; // Revert toggle
        } finally {
            hideLoading();
        }
    }

    // If user is trying to disable 2FA
    else if (isCurrentlyEnabled && !shouldEnable) {
        console.log('👇 Disabling 2FA...');
        try {
            showLoading('Disabling 2FA...');

            const token = getCSRFToken();
            console.log('CSRF Token found:', !!token);

            if (!token) {
                throw new Error('CSRF token not found');
            }

            const url = toggle.dataset.disableUrl;
            console.log('Making request to:', url);

            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: `__RequestVerificationToken=${encodeURIComponent(token)}`
            });

            console.log('Response status:', response.status);
            console.log('Response ok:', response.ok);

            if (!response.ok) {
                const errorText = await response.text();
                console.error('Response error:', errorText);
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            console.log('Response data:', data);

            if (data.success) {
                console.log('✅ 2FA disabled successfully');
                showToast(data.message || '2FA disabled successfully', 'success');
                toggle.setAttribute('data-enabled', 'false');

                // Hide backup codes section if visible
                const backupSection = document.querySelector('.border-top');
                if (backupSection) {
                    backupSection.style.display = 'none';
                }
            } else {
                console.error('❌ Disable failed:', data.message);
                throw new Error(data.message || 'Failed to disable 2FA');
            }

        } catch (error) {
            console.error('❌ Error disabling 2FA:', error);
            showToast(error.message || 'Failed to disable 2FA. Please try again.', 'error');
            toggle.checked = true; // Revert toggle
        } finally {
            hideLoading();
        }
    } else {
        console.log('ℹ️ No state change needed');
    }
}

// Utility functions
function getCSRFToken() {
    // Try using global function first
    if (window.getCSRFToken && typeof window.getCSRFToken === 'function') {
        return window.getCSRFToken();
    }

    console.log('Using local CSRF token detection');

    // Fallback to local implementation
    let token = null;

    // 1. Try from any form on the page
    const formToken = document.querySelector('input[name="__RequestVerificationToken"]');
    if (formToken) {
        token = formToken.value;
        console.log('Found CSRF token in form input');
    }

    // 2. Try from meta tag
    if (!token) {
        const metaToken = document.querySelector('meta[name="__RequestVerificationToken"]');
        if (metaToken) {
            token = metaToken.getAttribute('content');
            console.log('Found CSRF token in meta tag');
        }
    }

    if (!token) {
        console.error('CSRF token not found anywhere!');
    }

    return token;
}

function showLoading(message = 'Loading...') {
    // Try using global function first
    if (window.showLoading && typeof window.showLoading === 'function') {
        window.showLoading(message);
        return;
    }

    console.log('Using local showLoading');

    // Remove existing loader
    hideLoading();

    const loader = document.createElement('div');
    loader.id = 'twoFactorLoader';
    loader.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0,0,0,0.7);
        z-index: 10000;
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: center;
        color: white;
    `;
    loader.innerHTML = `
        <div class="spinner-border text-light mb-3" style="width: 3rem; height: 3rem;">
            <span class="visually-hidden">Loading...</span>
        </div>
        <div>${message}</div>
    `;
    document.body.appendChild(loader);
}

function hideLoading() {
    // Try using global function first
    if (window.hideLoading && typeof window.hideLoading === 'function') {
        window.hideLoading();
        return;
    }

    const loader = document.getElementById('twoFactorLoader');
    if (loader) loader.remove();
}

function showToast(message, type) {
    // Try using global function first
    if (window.showToast && typeof window.showToast === 'function') {
        window.showToast(message, type);
        return;
    }

    console.log('Using local showToast');

    // Fallback to local implementation
    const toast = document.createElement('div');
    toast.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible fade show position-fixed`;
    toast.style.cssText = `
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        max-width: 500px;
    `;
    toast.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;

    document.body.appendChild(toast);

    // Auto remove after 5 seconds
    setTimeout(() => {
        if (toast.parentNode) {
            toast.remove();
        }
    }, 5000);
}