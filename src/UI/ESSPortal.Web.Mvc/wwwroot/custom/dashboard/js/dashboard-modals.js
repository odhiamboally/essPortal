/* ========================================
   UN Portal - Dashboard Modals JavaScript
   ======================================== */

/**
 * Dashboard Modals Module
 * Handles modal loading, management, and interactions
 */
const DashboardModals = (function () {

    let isInitialized = false;

    /**
     * Initialize modal handling
     */
    function init() {
        if (isInitialized) {
            return;
        }

        checkInitialModalTriggers();
        isInitialized = true;
    }

    /**
     * Check for initial modal triggers from server (ViewBag)
     */
    function checkInitialModalTriggers() {
        const showPayslipModal = window.viewBag?.showPayslipModal || false;
        const showP9Modal = window.viewBag?.showP9Modal || false;
        const showLeaveDetailModal = window.viewBag?.showLeaveDetailModal || false;
        const showEditLeaveModal = window.viewBag?.showEditLeaveModal || false;

        if (showPayslipModal) loadPayslipModal();
        if (showP9Modal) loadP9Modal();
        if (showLeaveDetailModal) loadLeaveDetailModal();
        if (showEditLeaveModal) loadEditLeaveModal();
    }

    /**
      * Load payslip generator modal (custom month/year selection)
      */
    async function loadPayslipModal() {
        console.log('DashboardModals: Loading payslip modal...');

        try {
            showLoadingInContainer();

            const response = await fetch('/Payroll/PayslipModal', {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': getCSRFToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const html = await response.text();
            loadModalContent(html, 'payslipModal');

        } catch (error) {
            console.error('DashboardModals: Error loading payslip modal:', error);
            showModalError('Error loading payslip form. Please try again.');
        }
    }

    /**
     * Load P9 generator modal (custom year selection)
     */
    async function loadP9Modal() {
        console.log('DashboardModals: Loading P9 modal...');

        try {
            showLoadingInContainer();

            const response = await fetch('/Payroll/P9Modal', {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': getCSRFToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const html = await response.text();
            loadModalContent(html, 'p9Modal');

        } catch (error) {
            console.error('DashboardModals: Error loading P9 modal:', error);
            showModalError('Error loading P9 form. Please try again.');
        }
    }

    /**
     * Load leave detail modal
     */
    async function loadLeaveDetailModal(applicationNo) {
        console.log('DashboardModals: Loading leave detail modal for:', applicationNo);

        if (!applicationNo) {
            console.error('DashboardModals: No application number provided');
            showToast('error', 'Application number is required.');
            return;
        }

        try {
            showLoadingInContainer();

            const response = await fetch(`/Leave/LeaveDetailModal?applicationNo=${encodeURIComponent(applicationNo)}`, {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': getCSRFToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const html = await response.text();
            loadModalContent(html, 'leaveDetailModal');

        } catch (error) {
            console.error('DashboardModals: Error loading leave detail modal:', error);
            showModalError('Error loading leave details. Please try again.');
        }
    }

    /**
     * Load edit leave modal
     */
    async function loadEditLeaveModal(applicationNo) {
        console.log('DashboardModals: Loading edit leave modal for:', applicationNo);

        if (!applicationNo) {
            console.error('DashboardModals: No application number provided');
            showToast('error', 'Application number is required.');
            return;
        }

        try {
            showLoadingInContainer();

            const response = await fetch(`/Leave/EditLeaveModal?applicationNo=${encodeURIComponent(applicationNo)}`, {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': getCSRFToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const html = await response.text();
            loadModalContent(html, 'editLeaveModal');

        } catch (error) {
            console.error('DashboardModals: Error loading edit leave modal:', error);
            showModalError('Error loading leave editor. Please try again.');
        }
    }

    /**
     * Load modal content and show modal
     */
    function loadModalContent(html, modalId) {
        const container = document.getElementById('dynamicModalContainer');
        if (!container) {
            console.error('DashboardModals: Modal container not found');
            return;
        }

        container.innerHTML = html;

        const modalElement = document.getElementById(modalId);
        if (modalElement) {
            const modal = new bootstrap.Modal(modalElement, {
                backdrop: 'static',
                keyboard: true
            });

            modal.show();
            setupModalEventListeners(modalElement, modal);
        } else {
            console.error('DashboardModals: Modal element not found:', modalId);
            showModalError('Error initializing modal.');
        }
    }

    /**
     * Setup event listeners for modal
     */
    function setupModalEventListeners(modalElement, modalInstance) {
        // Clean up on modal close
        modalElement.addEventListener('hidden.bs.modal', function () {
            clearModalContainer();
        });

        // Handle form submissions within modal
        const forms = modalElement.querySelectorAll('form');
        forms.forEach(form => {
            form.addEventListener('submit', function (e) {
                handleModalFormSubmit(e, modalInstance);
            });
        });
    }

    /**
     * Handle modal form submission
     */
    async function handleModalFormSubmit(event, modalInstance) {
        event.preventDefault();
        const form = event.target;
        const formData = new FormData(form);

        showFormLoading(form);

        try {
            const response = await fetch(form.action, {
                method: form.method || 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': getCSRFToken()
                }
            });

            if (response.ok) {
                const result = await response.json();

                if (result.success && result.fileData) {
                    // Handle file download from modal form
                    downloadBase64File(result.fileData, result.fileName || 'document.pdf');
                    showToast('success', 'Document generated successfully!');
                    modalInstance.hide();
                } else if (result.success) {
                    showToast('success', result.message || 'Form submitted successfully!');
                    modalInstance.hide();
                } else {
                    showToast('error', result.message || 'An error occurred.');
                    hideFormLoading(form);
                }
            } else {
                throw new Error(`Form submission failed: ${response.status}`);
            }

        } catch (error) {
            console.error('DashboardModals: Form submission error:', error);
            hideFormLoading(form);
            showFormError(form, 'Error submitting form. Please try again.');
        }
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
     * Show loading state in modal container
     */
    function showLoadingInContainer() {
        const container = document.getElementById('dynamicModalContainer');
        if (container) {
            container.innerHTML = `
                <div class="modal-loading text-center p-5">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <p class="mt-2 text-muted">Loading...</p>
                </div>`;
        }
    }

    /**
     * Show error in modal container
     */
    function showModalError(message) {
        const container = document.getElementById('dynamicModalContainer');
        if (container) {
            container.innerHTML = `
                <div class="modal-error text-center p-5">
                    <div class="text-danger mb-3" style="font-size: 2rem;">⚠️</div>
                    <h6>Error</h6>
                    <p class="text-muted">${message}</p>
                    <button type="button" class="btn btn-primary btn-sm" onclick="location.reload()">
                        Refresh Page
                    </button>
                </div>`;
        }
        showToast('error', message);
    }

    /**
     * Clear modal container
     */
    function clearModalContainer() {
        const container = document.getElementById('dynamicModalContainer');
        if (container) {
            container.innerHTML = '';
        }
    }

    /**
     * Show loading state on form
     */
    function showFormLoading(form) {
        const submitButtons = form.querySelectorAll('button[type="submit"], input[type="submit"]');
        submitButtons.forEach(button => {
            button.disabled = true;
            button.setAttribute('data-original-text', button.innerHTML);
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Processing...';
        });
    }

    /**
     * Hide loading state on form
     */
    function hideFormLoading(form) {
        const submitButtons = form.querySelectorAll('button[type="submit"], input[type="submit"]');
        submitButtons.forEach(button => {
            button.disabled = false;
            button.innerHTML = button.getAttribute('data-original-text') || 'Submit';
        });
    }

    /**
     * Show form error
     */
    function showFormError(form, message) {
        let errorContainer = form.querySelector('.form-error-message');
        if (!errorContainer) {
            errorContainer = document.createElement('div');
            errorContainer.className = 'alert alert-danger form-error-message';
            form.insertBefore(errorContainer, form.firstChild);
        }
        errorContainer.innerHTML = `<i class="me-2">⚠️</i>${message}`;
    }

    /**
     * Show toast notification
     */
    function showToast(type, message) {
        if (typeof Swal !== 'undefined') {
            const config = {
                success: { icon: 'success', title: 'Success', confirmButtonColor: '#065f46' },
                error: { icon: 'error', title: 'Error', confirmButtonColor: '#dc2626' },
                info: { icon: 'info', title: 'Information', confirmButtonColor: '#0369a1' },
                warning: { icon: 'warning', title: 'Warning', confirmButtonColor: '#d97706' }
            };

            Swal.fire({
                ...config[type],
                text: message,
                timer: type === 'success' ? 3000 : undefined,
                timerProgressBar: type === 'success'
            });
        } else {
            alert(message);
        }
    }

    /**
     * Get CSRF token
     */
    function getCSRFToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    // Public API
    return {
        init: init,
        loadPayslipModal: loadPayslipModal,
        loadP9Modal: loadP9Modal,
        loadLeaveDetailModal: loadLeaveDetailModal,
        loadEditLeaveModal: loadEditLeaveModal
    };

})();

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    DashboardModals.init();
});

// Global functions for onclick handlers in HTML
function loadPayslipModal() {
    DashboardModals.loadPayslipModal();
}

function loadP9Modal() {
    DashboardModals.loadP9Modal();
}

function loadLeaveDetailModal(applicationNo) {
    DashboardModals.loadLeaveDetailModal(applicationNo);
}

function loadEditLeaveModal(applicationNo) {
    DashboardModals.loadEditLeaveModal(applicationNo);
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DashboardModals;
}