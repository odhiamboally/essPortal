/**
 * UN Portal - Forgot Password Page JavaScript
 * Enhanced with loading states and better UX
 */

document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    // ========================================
    // Form Submission Enhancement with Loading State
    // ========================================
    function initFormSubmission() {
        const form = document.querySelector('.signin-form');
        const submitButton = document.querySelector('#forgotPasswordSubmitBtn');

        if (!form || !submitButton) return;

        form.addEventListener('submit', function (event) {
            console.log('Forgot password form submission started');

            // Only validate if jQuery validation is available
            if (typeof $ !== 'undefined' && $.fn.validate && $(form).data('validator')) {
                console.log('jQuery validation detected, checking validity...');
                if (!$(form).valid()) {
                    console.log('jQuery validation failed');
                    event.preventDefault();
                    showValidationError();
                    return false;
                }
                console.log('jQuery validation passed');
            } else {
                // Basic HTML5 validation check
                if (!form.checkValidity()) {
                    console.log('HTML5 validation failed');
                    event.preventDefault();
                    showValidationError();
                    return false;
                }
                console.log('No jQuery validation detected, proceeding...');
            }

            console.log('Form is valid, showing loading state');

            // Show loading state
            showLoadingState(submitButton);

            // Safety timeout to re-enable button if something goes wrong
            setTimeout(function () {
                if (submitButton.disabled) {
                    hideLoadingState(submitButton);
                    console.log('Submit button re-enabled after timeout');
                }
            }, 30000);

            // Allow the form to submit
            return true;
        });
    }

    function showLoadingState(button) {
        if (!button) return;

        button.disabled = true;
        button.innerHTML = `
            <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
            Sending Reset Link...
        `;
        button.style.opacity = '0.8';

        // Add CSS for spinner if Bootstrap isn't available
        if (!document.querySelector('.spinner-border')) {
            addSpinnerStyles();
        }
    }

    function hideLoadingState(button) {
        if (!button) return;

        button.disabled = false;
        button.innerHTML = 'Send Reset Link';
        button.style.opacity = '1';
    }

    function addSpinnerStyles() {
        const style = document.createElement('style');
        style.textContent = `
            .spinner-border {
                display: inline-block;
                width: 1rem;
                height: 1rem;
                vertical-align: -0.125em;
                border: 0.125em solid currentColor;
                border-right-color: transparent;
                border-radius: 50%;
                animation: spinner-border 0.75s linear infinite;
            }
            .spinner-border-sm {
                width: 0.875rem;
                height: 0.875rem;
                border-width: 0.125em;
            }
            .me-2 {
                margin-right: 0.5rem;
            }
            @keyframes spinner-border {
                to { transform: rotate(360deg); }
            }
        `;
        document.head.appendChild(style);
    }

    function showValidationError() {
        // Show SweetAlert for validation errors if available
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'error',
                title: 'Validation Error',
                text: 'Please enter a valid email address.',
                confirmButtonColor: '#dc2626'
            });
        } else {
            // Fallback to alert
            alert('Please enter a valid email address.');
        }
    }

    // ========================================
    // Email Validation Enhancement
    // ========================================
    function initEmailValidation() {
        const emailInput = document.querySelector('input[name="Email"]');
        if (!emailInput) return;

        emailInput.addEventListener('blur', function () {
            validateEmail(this);
        });

        emailInput.addEventListener('input', function () {
            // Clear error styling when user starts typing
            this.classList.remove('input-validation-error');
            const errorSpan = this.parentElement.querySelector('.field-validation-error');
            if (errorSpan) {
                errorSpan.style.display = 'none';
            }
        });
    }

    function validateEmail(emailField) {
        const value = emailField.value.trim();
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        let isValid = true;
        let errorMessage = '';

        if (!value) {
            isValid = false;
            errorMessage = 'Email address is required.';
        } else if (!emailRegex.test(value)) {
            isValid = false;
            errorMessage = 'Please enter a valid email address.';
        }

        // Update field styling
        const errorSpan = emailField.parentElement.querySelector('.field-validation-error');
        if (!isValid) {
            emailField.classList.add('input-validation-error');
            if (errorSpan) {
                errorSpan.textContent = errorMessage;
                errorSpan.style.display = 'block';
            }
        } else {
            emailField.classList.remove('input-validation-error');
            if (errorSpan) {
                errorSpan.style.display = 'none';
            }
        }

        return isValid;
    }

    // ========================================
    // Enhanced Validation Display
    // ========================================
    function initValidationEnhancements() {
        // Show validation summary if it has content
        const validationSummary = document.querySelector('.validation-summary');
        if (validationSummary && validationSummary.querySelector('ul li')) {
            validationSummary.classList.add('show');
        }

        // Remove error styling when user starts typing
        const errorFields = document.querySelectorAll('.input-validation-error');
        errorFields.forEach(function (field) {
            field.addEventListener('input', function () {
                this.classList.remove('input-validation-error');

                // Also hide the field validation error
                const errorSpan = this.parentElement.querySelector('.field-validation-error');
                if (errorSpan) {
                    errorSpan.style.display = 'none';
                }
            });
        });
    }

    // ========================================
    // Auto-focus Enhancement
    // ========================================
    function initAutoFocus() {
        const emailInput = document.querySelector('input[name="Email"]');
        if (emailInput) {
            // Small delay to ensure page is fully loaded
            setTimeout(() => {
                emailInput.focus();
                emailInput.select(); // Select any existing text
            }, 100);
        }
    }

    // ========================================
    // Alert Auto-Hide Functionality
    // ========================================
    function initAlertAutoHide() {
        const alerts = document.querySelectorAll('.alert');

        alerts.forEach(function (alert) {
            // Auto-hide after 7 seconds
            setTimeout(function () {
                alert.style.opacity = '0';
                setTimeout(function () {
                    if (alert.parentNode) {
                        alert.style.display = 'none';
                    }
                }, 300);
            }, 7000);
        });
    }

    // ========================================
    // Accessibility Enhancements
    // ========================================
    function initAccessibility() {
        // Add proper ARIA attributes
        const form = document.querySelector('.signin-form');
        if (form) {
            form.setAttribute('novalidate', 'novalidate');
        }

        // Add live region for screen reader announcements
        const liveRegion = document.createElement('div');
        liveRegion.setAttribute('aria-live', 'polite');
        liveRegion.setAttribute('aria-atomic', 'true');
        liveRegion.className = 'sr-only';
        liveRegion.style.cssText = 'position:absolute;width:1px;height:1px;padding:0;margin:-1px;overflow:hidden;clip:rect(0,0,0,0);white-space:nowrap;border:0;';
        document.body.appendChild(liveRegion);

        // Function to announce messages to screen readers
        window.announceToScreenReader = function (message) {
            liveRegion.textContent = message;
            setTimeout(function () {
                liveRegion.textContent = '';
            }, 1000);
        };
    }

    // ========================================
    // Help Section Enhancement
    // ========================================
    function initHelpSection() {
        const helpSection = document.querySelector('.help-section');
        if (!helpSection) return;

        // Add click handler for help text (could expand to show more info)
        helpSection.addEventListener('click', function () {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'info',
                    title: 'Need Help?',
                    html: `
                        <p>If you're having trouble accessing your account:</p>
                        <ul style="text-align: left; margin: 16px 0;">
                            <li>Check your spam/junk folder for the reset email</li>
                            <li>Ensure you're using the email associated with your account</li>
                            <li>Wait a few minutes before requesting another reset</li>
                            <li>Contact IT Support if the problem persists</li>
                        </ul>
                    `,
                    confirmButtonColor: '#0369a1',
                    confirmButtonText: 'Got it'
                });
            }
        });

        // Add visual indication that help section is clickable
        helpSection.style.cursor = 'pointer';
        helpSection.style.transition = 'background-color 0.2s ease';

        helpSection.addEventListener('mouseenter', function () {
            this.style.backgroundColor = '#f1f3f4';
        });

        helpSection.addEventListener('mouseleave', function () {
            this.style.backgroundColor = '#f8f9fa';
        });
    }

    // ========================================
    // Rate Limiting Feedback
    // ========================================
    function initRateLimitFeedback() {
        const form = document.querySelector('.signin-form');
        const submitButton = document.querySelector('#forgotPasswordSubmitBtn');

        if (!form || !submitButton) return;

        // Track submission attempts
        let submissionCount = parseInt(sessionStorage.getItem('forgotPasswordAttempts') || '0');
        const maxAttempts = 3;
        const cooldownMinutes = 5;

        // Check if user is in cooldown period
        const lastSubmission = parseInt(sessionStorage.getItem('lastForgotPasswordSubmission') || '0');
        const now = Date.now();
        const timeSinceLastSubmission = now - lastSubmission;
        const cooldownPeriod = cooldownMinutes * 60 * 1000; // Convert to milliseconds

        if (submissionCount >= maxAttempts && timeSinceLastSubmission < cooldownPeriod) {
            const remainingTime = Math.ceil((cooldownPeriod - timeSinceLastSubmission) / 60000);
            submitButton.disabled = true;
            submitButton.innerHTML = `Please wait ${remainingTime} minute(s)`;

            // Start countdown
            const countdown = setInterval(() => {
                const currentTime = Date.now();
                const timeLeft = Math.ceil((cooldownPeriod - (currentTime - lastSubmission)) / 60000);

                if (timeLeft <= 0) {
                    clearInterval(countdown);
                    submitButton.disabled = false;
                    submitButton.innerHTML = 'Send Reset Link';
                    sessionStorage.removeItem('forgotPasswordAttempts');
                    sessionStorage.removeItem('lastForgotPasswordSubmission');
                } else {
                    submitButton.innerHTML = `Please wait ${timeLeft} minute(s)`;
                }
            }, 1000);
        }

        // Track new submissions
        form.addEventListener('submit', function () {
            submissionCount++;
            sessionStorage.setItem('forgotPasswordAttempts', submissionCount.toString());
            sessionStorage.setItem('lastForgotPasswordSubmission', Date.now().toString());

            if (submissionCount >= maxAttempts) {
                setTimeout(() => {
                    if (typeof Swal !== 'undefined') {
                        Swal.fire({
                            icon: 'warning',
                            title: 'Rate Limit',
                            text: `You've reached the maximum number of reset attempts. Please wait ${cooldownMinutes} minutes before trying again.`,
                            confirmButtonColor: '#d97706'
                        });
                    }
                }, 1000);
            }
        });
    }

    // ========================================
    // Initialization
    // ========================================
    function init() {
        try {
            initFormSubmission();
            initEmailValidation();
            initValidationEnhancements();
            initAutoFocus();
            initAlertAutoHide();
            initAccessibility();
            initHelpSection();
            initRateLimitFeedback();
        } catch (error) {
            console.error('Error initializing forgot password page:', error);
        }
    }

    // Start initialization
    init();
});