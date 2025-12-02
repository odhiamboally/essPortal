/**
 * Universal Toast Notification System
 * A clean, reusable notification system using SweetAlert2
 */

class ToastManager {
    constructor() {
        this.initializeOnLoad();
    }

    /**
     * Initialize toast system when page loads
     */
    initializeOnLoad() {
        document.addEventListener('DOMContentLoaded', () => {
            this.showPageMessages();
        });
    }

    /**
     * Show messages that were set server-side via TempData/ViewBag
     */
    showPageMessages() {
        // Get messages from data attributes or meta tags
        const messages = this.extractServerMessages();

        if (messages.length === 0) return;

        // Show the most important message (error > warning > success > info)
        const priorityOrder = ['error', 'warning', 'success', 'info'];
        const messageToShow = priorityOrder
            .map(type => messages.find(msg => msg.type === type))
            .find(msg => msg !== undefined);

        if (messageToShow) {
            this.showToast(messageToShow.type, messageToShow.title, messageToShow.message);
        }
    }

    /**
     * Extract server messages from various sources
     */
    extractServerMessages() {
        const messages = [];

        // Method 1: From data attributes on body
        const body = document.body;
        if (body.dataset.successMessage) {
            messages.push({
                type: 'success',
                title: 'Success',
                message: body.dataset.successMessage
            });
        }
        if (body.dataset.errorMessage) {
            messages.push({
                type: 'error',
                title: 'Error',
                message: body.dataset.errorMessage
            });
        }
        if (body.dataset.warningMessage) {
            messages.push({
                type: 'warning',
                title: 'Warning',
                message: body.dataset.warningMessage
            });
        }
        if (body.dataset.infoMessage) {
            messages.push({
                type: 'info',
                title: 'Information',
                message: body.dataset.infoMessage
            });
        }

        // Method 2: From meta tags
        const metaMessages = [
            { selector: 'meta[name="toast-success"]', type: 'success', title: 'Success' },
            { selector: 'meta[name="toast-error"]', type: 'error', title: 'Error' },
            { selector: 'meta[name="toast-warning"]', type: 'warning', title: 'Warning' },
            { selector: 'meta[name="toast-info"]', type: 'info', title: 'Information' }
        ];

        metaMessages.forEach(({ selector, type, title }) => {
            const meta = document.querySelector(selector);
            if (meta && meta.content) {
                messages.push({
                    type,
                    title,
                    message: meta.content
                });
            }
        });

        return messages;
    }

    /**
     * Show a toast notification
     */
    showToast(type, title, message, options = {}) {
        if (!this.isSwalAvailable()) {
            this.showFallbackMessage(type, title, message);
            return;
        }

        const config = this.getToastConfig(type, title, message, options);
        Swal.fire(config);
    }

    /**
     * Show success toast
     */
    success(message, title = 'Success', options = {}) {
        this.showToast('success', title, message, options);
    }

    /**
     * Show error toast
     */
    error(message, title = 'Error', options = {}) {
        this.showToast('error', title, message, options);
    }

    /**
     * Show warning toast
     */
    warning(message, title = 'Warning', options = {}) {
        this.showToast('warning', title, message, options);
    }

    /**
     * Show info toast
     */
    info(message, title = 'Information', options = {}) {
        this.showToast('info', title, message, options);
    }

    /**
     * Show confirmation dialog
     */
    async confirm(message, title = 'Confirm Action', options = {}) {
        if (!this.isSwalAvailable()) {
            return confirm(`${title}\n\n${message}`);
        }

        const result = await Swal.fire({
            title,
            text: message,
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: options.confirmText || 'Yes',
            cancelButtonText: options.cancelText || 'Cancel',
            ...options
        });

        return result.isConfirmed;
    }

    /**
     * Get SweetAlert configuration for different toast types
     */
    getToastConfig(type, title, message, options = {}) {
        const baseConfig = {
            title,
            text: message,
            showConfirmButton: true,
            timer: options.timer || (type === 'success' ? 3000 : undefined),
            timerProgressBar: options.timer ? true : false,
            ...options
        };

        const typeConfigs = {
            success: {
                icon: 'success',
                confirmButtonColor: '#059669',
                toast: options.toast !== false,
                position: options.position || 'top-end'
            },
            error: {
                icon: 'error',
                confirmButtonColor: '#dc2626'
            },
            warning: {
                icon: 'warning',
                confirmButtonColor: '#d97706'
            },
            info: {
                icon: 'info',
                confirmButtonColor: '#0369a1'
            }
        };

        return { ...baseConfig, ...typeConfigs[type] };
    }

    /**
     * Check if SweetAlert is available
     */
    isSwalAvailable() {
        return typeof Swal !== 'undefined';
    }

    /**
     * Fallback for when SweetAlert is not available
     */
    showFallbackMessage(type, title, message) {
        const prefix = type.charAt(0).toUpperCase() + type.slice(1);
        alert(`${prefix}: ${message}`);
    }

    /**
     * Clear server messages to prevent them from showing again
     */
    clearServerMessages() {
        // Clear data attributes
        const body = document.body;
        delete body.dataset.successMessage;
        delete body.dataset.errorMessage;
        delete body.dataset.warningMessage;
        delete body.dataset.infoMessage;

        // Clear meta tags
        const metaSelectors = [
            'meta[name="toast-success"]',
            'meta[name="toast-error"]',
            'meta[name="toast-warning"]',
            'meta[name="toast-info"]'
        ];

        metaSelectors.forEach(selector => {
            const meta = document.querySelector(selector);
            if (meta) {
                meta.remove();
            }
        });
    }

    /**
     * Show activity-specific success message (for profile updates, etc.)
     */
    showActivitySuccess(activity, message) {
        const activityTitles = {
            'profile_update': 'Profile Updated',
            'password_change': 'Password Changed',
            'settings_update': 'Settings Updated',
            'file_upload': 'File Uploaded',
            'data_save': 'Data Saved'
        };

        const title = activityTitles[activity] || 'Success';
        this.success(message, title, { timer: 4000 });
    }
}

// Create global instance
window.Toast = new ToastManager();

// Backward compatibility - expose individual methods globally
window.showToast = (type, title, message, options) => window.Toast.showToast(type, title, message, options);
window.showSuccess = (message, title) => window.Toast.success(message, title);
window.showError = (message, title) => window.Toast.error(message, title);
window.showWarning = (message, title) => window.Toast.warning(message, title);
window.showInfo = (message, title) => window.Toast.info(message, title);