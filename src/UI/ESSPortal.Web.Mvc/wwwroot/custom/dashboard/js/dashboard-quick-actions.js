const DashboardQuickActions = (function () {
    'use strict';

    let isInitialized = false;

    /**
     * Initialize the module
     */
    function init() {
        if (isInitialized) {
            console.warn('DashboardQuickActions: Already initialized');
            return;
        }

        console.log('DashboardQuickActions: Initializing...');
        createLoadingOverlay();
        setupEventListeners();
        isInitialized = true;
        console.log('DashboardQuickActions: Ready');
    }

    /**
     * Setup click event listeners for download actions
     */
    function setupEventListeners() {
        document.addEventListener('click', function (event) {
            const actionItem = event.target.closest('a.action-item');
            if (!actionItem) return;

            const href = actionItem.getAttribute('href');
            if (!href) return;

            // Check if this is a direct download action
            const isDownloadAction =
                href.includes('/GetRecentPayslip') ||
                href.includes('/GetCurrentPayslip') ||
                href.includes('/GetRecentP9') ||
                href.includes('/GetCurrentP9');

            if (isDownloadAction) {
                event.preventDefault();
                event.stopPropagation(); // Prevent other handlers from firing
                handleDownload(href);
            }
        }, true); // Use capture phase to handle before other listeners
    }

    /**
     * Handle the download request
     */
    function handleDownload(url) {
        console.log('DashboardQuickActions: Processing download:', url);

        // Determine action type for messaging
        const actionType = url.includes('Payslip') ? 'payslip' : 'P9 certificate';
        const actionVerb = 'Generating';

        // Show loading overlay
        showLoadingOverlay(
            `${actionVerb} your ${actionType}...`,
            'Please wait while we prepare your document'
        );

        // Make the request
        fetch(url, {
            method: 'GET',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(result => {
                if (result.success && result.fileData) {
                    // Update overlay to show success
                    updateLoadingOverlay('Download ready!', 'Starting download...', 'success');

                    // Download the file
                    downloadBase64File(result.fileData, result.fileName || `${actionType}.pdf`);

                    // Hide overlay after brief delay
                    setTimeout(() => {
                        hideLoadingOverlay();
                        showSuccessToast(`${capitalize(actionType)} downloaded successfully`);
                    }, 800);
                } else {
                    hideLoadingOverlay();
                    showErrorAlert('Download Failed', result.message || 'Unable to generate document.');
                }
            })
            .catch(error => {
                console.error('DashboardQuickActions: Request failed:', error);
                hideLoadingOverlay();
                showErrorAlert('Request Failed', 'Unable to process request. Please try again.');
            });
    }

    /**
     * Download a base64-encoded file
     */
    function downloadBase64File(base64Data, fileName) {
        const byteArray = new Uint8Array(
            atob(base64Data).split('').map(c => c.charCodeAt(0))
        );
        const blob = new Blob([byteArray], { type: 'application/pdf' });
        const downloadUrl = URL.createObjectURL(blob);

        const link = document.createElement('a');
        link.href = downloadUrl;
        link.download = fileName;
        link.style.display = 'none';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(downloadUrl);
    }

    /**
     * Create the loading overlay (if not exists)
     */
    function createLoadingOverlay() {
        if (document.getElementById('dashboardLoadingOverlay')) return;

        const overlayHTML = `
            <div id="dashboardLoadingOverlay" class="dashboard-loading-overlay" style="display: none;">
                <div class="loading-backdrop"></div>
                <div class="loading-content">
                    <div class="loading-spinner">
                        <div class="spinner-ring"></div>
                        <div class="spinner-ring"></div>
                        <div class="spinner-ring"></div>
                    </div>
                    <h4 class="loading-title">Processing...</h4>
                    <p class="loading-message">Please wait</p>
                </div>
            </div>
        `;

        const overlayCSS = `
            .dashboard-loading-overlay {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                z-index: 9999;
                display: flex;
                align-items: center;
                justify-content: center;
            }
            .loading-backdrop {
                position: absolute;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(255, 255, 255, 0.95);
                backdrop-filter: blur(4px);
            }
            .loading-content {
                position: relative;
                text-align: center;
                background: white;
                padding: 40px;
                border-radius: 12px;
                box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
                border: 1px solid rgba(0, 0, 0, 0.1);
                max-width: 400px;
                margin: 20px;
            }
            .loading-spinner {
                position: relative;
                width: 80px;
                height: 80px;
                margin: 0 auto 20px;
            }
            .spinner-ring {
                position: absolute;
                width: 64px;
                height: 64px;
                margin: 8px;
                border: 8px solid transparent;
                border-top: 8px solid #007bff;
                border-radius: 50%;
                animation: quickActionSpin 1.2s cubic-bezier(0.5, 0, 0.5, 1) infinite;
            }
            .spinner-ring:nth-child(1) { animation-delay: -0.45s; }
            .spinner-ring:nth-child(2) { animation-delay: -0.3s; }
            .spinner-ring:nth-child(3) { animation-delay: -0.15s; }
            @keyframes quickActionSpin {
                0% { transform: rotate(0deg); }
                100% { transform: rotate(360deg); }
            }
            .loading-title {
                color: #333;
                margin: 0 0 10px 0;
                font-size: 1.25rem;
                font-weight: 600;
            }
            .loading-message {
                color: #666;
                margin: 0;
                font-size: 0.95rem;
            }
            .loading-content.success .spinner-ring {
                border-top-color: #28a745;
            }
            .loading-content.success .loading-title {
                color: #28a745;
            }
            .dashboard-loading-overlay.show {
                animation: overlayFadeIn 0.3s ease-out;
            }
            .dashboard-loading-overlay.hide {
                animation: overlayFadeOut 0.3s ease-out;
            }
            @keyframes overlayFadeIn {
                from { opacity: 0; }
                to { opacity: 1; }
            }
            @keyframes overlayFadeOut {
                from { opacity: 1; }
                to { opacity: 0; }
            }
        `;

        // Inject CSS
        const style = document.createElement('style');
        style.id = 'dashboardLoadingCSS';
        style.textContent = overlayCSS;
        document.head.appendChild(style);

        // Inject HTML
        document.body.insertAdjacentHTML('beforeend', overlayHTML);
    }

    /**
     * Show loading overlay
     */
    function showLoadingOverlay(title, message) {
        const overlay = document.getElementById('dashboardLoadingOverlay');
        if (!overlay) return;

        overlay.querySelector('.loading-title').textContent = title;
        overlay.querySelector('.loading-message').textContent = message;
        overlay.querySelector('.loading-content').className = 'loading-content';

        overlay.style.display = 'flex';
        overlay.classList.add('show');
        overlay.classList.remove('hide');
        document.body.style.overflow = 'hidden';
    }

    /**
     * Update loading overlay content
     */
    function updateLoadingOverlay(title, message, type) {
        const overlay = document.getElementById('dashboardLoadingOverlay');
        if (!overlay) return;

        overlay.querySelector('.loading-title').textContent = title;
        overlay.querySelector('.loading-message').textContent = message;
        overlay.querySelector('.loading-content').className = `loading-content ${type || ''}`;
    }

    /**
     * Hide loading overlay
     */
    function hideLoadingOverlay() {
        const overlay = document.getElementById('dashboardLoadingOverlay');
        if (!overlay) return;

        overlay.classList.add('hide');
        overlay.classList.remove('show');

        setTimeout(() => {
            overlay.style.display = 'none';
            document.body.style.overflow = '';
        }, 300);
    }

    /**
     * Show success toast notification
     */
    function showSuccessToast(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'success',
                title: 'Downloaded!',
                text: message,
                timer: 3000,
                showConfirmButton: false,
                toast: true,
                position: 'top-end'
            });
        }
    }

    /**
     * Show error alert
     */
    function showErrorAlert(title, message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'error',
                title: title,
                text: message,
                confirmButtonColor: '#dc3545'
            });
        } else {
            alert(`${title}: ${message}`);
        }
    }

    /**
     * Capitalize first letter
     */
    function capitalize(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    }

    // Public API
    return {
        init: init
    };

})();

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    DashboardQuickActions.init();
});