/* ========================================
   UN Portal - Layout Core JavaScript
   ======================================== */

/**
 * Layout Core Module
 * Handles main layout functionality, CSRF tokens, and modal management
 */
const LayoutCore = {

    /**
     * Initialize layout functionality
     */
    init() {
        console.log('Layout Core: Initializing...');
        this.setupGlobalErrorHandling();
        this.initializeCSRFToken();
        this.setupModalFunctions();
        this.setupGlobalModalCleanup();
        console.log('Layout Core: Initialization complete');
    },

    /**
     * Initialize CSRF token
     */
    initializeCSRFToken() {
        window.csrfToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    },

    /**
     * Get CSRF token from various sources
     */
    getCSRFToken() {
        // Check meta tag first
        const meta = document.querySelector('meta[name="__RequestVerificationToken"]');
        if (meta) return meta.content;

        // Check input
        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        if (input) return input.value;

        // Check global variable
        if (window.csrfToken) return window.csrfToken;

        console.error('CSRF token not found!');
        return '';
    },

    /**
     * Setup global AJAX error handling
     */
    setupGlobalErrorHandling() {
        // Global AJAX error handler for session expiry
        if (typeof $ !== 'undefined') {
            $(document).ajaxError((event, xhr, settings) => {
                if (xhr.status === 401) {
                    const response = xhr.responseJSON;
                    if (response && (response.error === 'session_expired' || response.error === 'unauthorized')) {
                        // Show user-friendly message
                        alert('Your session has expired. You will be redirected to the login page.');

                        // Redirect to login with session expired flag
                        const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
                        window.location.href = '/Auth/SignIn?returnUrl=' + returnUrl + '&sessionExpired=true';
                    }
                }
            });
        }
    },

    /**
     * Setup global modal cleanup to prevent backdrop issues
     */
    setupGlobalModalCleanup() {
        // Global cleanup for all modals
        document.addEventListener('hidden.bs.modal', function (event) {
            console.log('Global modal cleanup triggered');

            // Small delay to ensure Bootstrap has finished its cleanup
            setTimeout(() => {
                // Remove any lingering backdrop elements
                const backdrops = document.querySelectorAll('.modal-backdrop');
                if (backdrops.length > 0) {
                    console.log(`Removing ${backdrops.length} lingering backdrop(s)`);
                    backdrops.forEach(backdrop => backdrop.remove());
                }

                // Reset body state if no modals are open
                const openModals = document.querySelectorAll('.modal.show');
                if (openModals.length === 0) {
                    document.body.classList.remove('modal-open');
                    document.body.style.paddingRight = '';
                    document.body.style.overflow = '';
                    console.log('Body state reset - no open modals');
                }
            }, 150);
        });

        console.log('Global modal cleanup handlers installed');
    },

    /**
     * Handle AJAX failures
     */
    onAjaxFailure(xhr, status, error) {
        console.error('AJAX Error:', status, error);
        this.showError('Request failed: ' + error);
    },

    /**
     * Execute scripts found in HTML content
     */
    executeScripts(container) {
        const scripts = container.querySelectorAll('script');
        scripts.forEach(script => {
            if (script.textContent.trim()) {
                try {
                    console.log('Executing modal script...');
                    // Create a new script element and execute it
                    const newScript = document.createElement('script');
                    newScript.textContent = script.textContent;
                    document.head.appendChild(newScript);

                    // Clean up after a short delay
                    setTimeout(() => {
                        if (newScript.parentNode) {
                            newScript.parentNode.removeChild(newScript);
                        }
                    }, 1000);
                } catch (scriptError) {
                    console.error('Error executing modal script:', scriptError);
                }
            }
        });
    },

    /**
     * Load modal content from URL with script execution
     */
    async loadModalContent(url, modalId) {
        console.log(`Loading modal content from ${url} for modal ${modalId}`);
        this.showLoading();

        try {
            const response = await fetch(url, {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': this.getCSRFToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const html = await response.text();
            console.log(`Modal content loaded for ${modalId}`);

            // Clear any existing modal content and backdrop issues
            const container = document.getElementById('dynamicModalContainer');
            container.innerHTML = '';

            // Remove any lingering backdrop elements
            const existingBackdrops = document.querySelectorAll('.modal-backdrop');
            existingBackdrops.forEach(backdrop => backdrop.remove());

            // Insert new modal HTML
            container.innerHTML = html;

            // CRITICAL: Execute any scripts in the loaded content
            this.executeScripts(container);

            // Wait a moment for DOM to update and scripts to run, then show modal
            setTimeout(() => {
                const modalElement = document.getElementById(modalId);
                if (modalElement) {
                    // Create modal instance
                    const modal = new bootstrap.Modal(modalElement, {
                        backdrop: true,
                        keyboard: true,
                        focus: true
                    });

                    // Add event listeners to handle cleanup
                    modalElement.addEventListener('hidden.bs.modal', function () {
                        console.log(`Modal ${modalId} hidden, cleaning up...`);

                        // Clean up any remaining backdrop elements
                        const backdrops = document.querySelectorAll('.modal-backdrop');
                        backdrops.forEach(backdrop => {
                            backdrop.remove();
                        });

                        // Reset body classes
                        document.body.classList.remove('modal-open');
                        document.body.style.paddingRight = '';
                        document.body.style.overflow = '';

                        // Clear the modal container
                        container.innerHTML = '';
                    });

                    modal.show();
                    console.log(`Modal ${modalId} displayed`);
                } else {
                    console.error(`Modal element ${modalId} not found after loading`);
                }
            }, 100); // Increased delay to allow scripts to execute

        } catch (error) {
            console.error(`Error loading ${modalId} modal:`, error);
            const container = document.getElementById('dynamicModalContainer');
            container.innerHTML = '';

            // Clean up any backdrop from failed load
            const backdrops = document.querySelectorAll('.modal-backdrop');
            backdrops.forEach(backdrop => backdrop.remove());
            document.body.classList.remove('modal-open');

            this.showToast('Error loading content. Please try again.', 'error');
        }
    },

    /**
     * Show loading indicator
     */
    showLoading() {
        const container = document.getElementById('dynamicModalContainer');
        if (container) {
            container.innerHTML = `
                <div class="text-center p-5">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <p class="mt-2">Loading...</p>
                </div>`;
        }
    },

    /**
     * Show toast notification
     */
    showToast(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        toast.style.cssText = `
            top: 20px;
            right: 20px;
            z-index: 9999;
            min-width: 300px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        `;
        toast.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(toast);

        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (toast.parentNode) {
                toast.remove();
            }
        }, 5000);
    },

    /**
     * Show error message
     */
    showError(message) {
        this.showToast(message, 'danger');
    },

    /**
     * Show success message
     */
    showSuccess(message) {
        this.showToast(message, 'success');
    },

    /**
     * Setup modal loading functions
     */
    setupModalFunctions() {
        // Make functions globally available
        window.loadModalContent = this.loadModalContent.bind(this);
        window.showLoading = this.showLoading.bind(this);
        window.showToast = this.showToast.bind(this);
        window.getCSRFToken = this.getCSRFToken.bind(this);
        window.onAjaxFailure = this.onAjaxFailure.bind(this);

        // Specific modal loaders
        window.loadPayslipModal = () => {
            this.loadModalContent('/Payroll/PayslipModal', 'payslipModal');
        };

        window.loadP9Modal = () => {
            this.loadModalContent('/Payroll/P9Modal', 'p9Modal');
        };

        window.load2FAModal = () => {
            this.loadModalContent('/TwoFactor/Settings', 'twoFactorSettingsModal');
        };
    }
};

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    LayoutCore.init();
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = LayoutCore;
}