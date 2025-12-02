/**
 * MINIMAL 2FA JavaScript - No interference with form submission
 * Keeps SweetAlert functionality for error handling
 */
(function () {
    'use strict';

    // DOM Elements
    let codeInput, verifyBtn, loadingOverlay, form;

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        console.log('🔍 2FA: DOM loaded, initializing...');

        // Get elements
        form = document.getElementById('twoFactorForm');
        codeInput = document.getElementById('code');
        verifyBtn = document.getElementById('verifyBtn');
        loadingOverlay = document.getElementById('loadingOverlay');

        console.log('🔍 2FA: Elements found:', {
            form: !!form,
            codeInput: !!codeInput,
            verifyBtn: !!verifyBtn
        });

        if (!form || !codeInput || !verifyBtn) {
            console.error('❌ 2FA: Required elements not found!');
            showAlert('error', 'Page initialization failed. Please refresh the page.');
            return;
        }

        // Set up event listeners
        setupEventListeners();

        if (window.DeviceFingerprint) {
            DeviceFingerprint.setOnForm('twoFactorForm', 'DeviceFingerprint');
        }

        // Focus on input
        codeInput.focus();

        console.log('✅ 2FA: Initialization complete');
    });

    function setupEventListeners() {
        // Code input handling
        codeInput.addEventListener('input', function (e) {
            console.log('🔍 2FA: Input changed:', e.target.value);

            // Remove non-numeric characters
            let value = e.target.value.replace(/\D/g, '');

            // Limit to 6 digits
            if (value.length > 6) {
                value = value.slice(0, 6);
            }

            e.target.value = value;

            // Enable/disable button
            verifyBtn.disabled = value.length !== 6;

            console.log('🔍 2FA: Button enabled:', !verifyBtn.disabled);
        });

        // Handle paste
        codeInput.addEventListener('paste', function (e) {
            e.preventDefault();
            const paste = (e.clipboardData || window.clipboardData).getData('text');
            const numbers = paste.replace(/\D/g, '').slice(0, 6);

            if (numbers.length > 0) {
                e.target.value = numbers;
                verifyBtn.disabled = numbers.length !== 6;
            }
        });

        // 🔥 MINIMAL Form validation - NO SUBMIT INTERFERENCE
        form.addEventListener('submit', function (e) {
            console.log('🔍 2FA: Form submit triggered');

            const code = codeInput.value.trim();

            // Only prevent if validation fails
            if (!code || code.length !== 6) {
                console.log('❌ 2FA: Invalid code, preventing submit');
                e.preventDefault();
                showAlert('error', 'Please enter a valid 6-digit verification code.');
                codeInput.focus();
                return false;
            }

            console.log('✅ 2FA: Validation passed, allowing form submission');
            // NO e.preventDefault() - let the browser handle the submission naturally
            // NO showLoading() - this might disable the form before submission
        });

        // Enter key handling
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && codeInput.value.length === 6 && !verifyBtn.disabled) {
                console.log('🔍 2FA: Enter key pressed, submitting form');
                verifyBtn.click();
            }
        });
    }

    function showAlert(type, message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: type,
                title: type === 'error' ? 'Error' : 'Success',
                text: message,
                confirmButtonText: 'OK'
            });
        } else {
            // Fallback to regular alert
            alert(message);
        }
    }

    // Handle page errors (if antiforgery fails and redirects)
    window.addEventListener('DOMContentLoaded', function () {
        // Check if there's an error message in URL params or TempData
        const urlParams = new URLSearchParams(window.location.search);
        const errorMessage = urlParams.get('error');

        if (errorMessage) {
            showAlert('error', decodeURIComponent(errorMessage));
        }
    });

    // Export for external use
    window.TwoFactorAuth = {
        showAlert: showAlert
    };

})();