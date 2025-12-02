/**
 * UN Portal - SignIn Page JavaScript
 * Simplified and improved version
 */

document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    // ========================================
    // Password Toggle Functionality
    // ========================================
    function initPasswordToggle() {
        const toggleBtn = document.getElementById('toggleBtn');
        const passwordInput = document.querySelector('input[name="Password"]');

        if (!toggleBtn || !passwordInput) return;

        const eyeIcon = toggleBtn.querySelector('.eye-icon');

        toggleBtn.addEventListener('click', function () {
            const isPassword = passwordInput.type === 'password';
            passwordInput.type = isPassword ? 'text' : 'password';
            eyeIcon.textContent = isPassword ? '🙈' : '👁️';
            this.setAttribute('aria-label', isPassword ? 'Hide password' : 'Show password');
        });

        // Initial setup
        toggleBtn.setAttribute('aria-label', 'Show password');
        toggleBtn.setAttribute('tabindex', '0');

        // Keyboard support
        toggleBtn.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            }
        });
    }

    // ========================================
    // Form Validation & Submission
    // ========================================
    function initFormSubmission() {
        const form = document.querySelector('.signin-form');
        const submitButton = document.querySelector('.btn-signin');

        if (!form || !submitButton) return;

        let isSubmitting = false;

        form.addEventListener('submit', function (event) {
            // Prevent double submission
            if (isSubmitting) {
                event.preventDefault();
                return false;
            }

            // Basic validation
            const employeeNumber = form.querySelector('input[name="EmployeeNumber"]')?.value.trim();
            const password = form.querySelector('input[name="Password"]')?.value.trim();

            if (!employeeNumber || !password) {
                event.preventDefault();
                showMessage('error', 'Please enter both employee number and password.');
                return false;
            }

            // Set submitting state
            isSubmitting = true;
            setFormState(false); // Disable form

            // If form validation passes, it will submit naturally
            // The server will handle the response and page redirect/reload
        });

        // Re-enable form if there's a page error (like validation errors)
        // This handles cases where the page reloads with errors
        window.addEventListener('load', function () {
            setFormState(true);
            isSubmitting = false;
        });
    }

    // ========================================
    // Form State Management
    // ========================================
    function setFormState(enabled) {
        const form = document.querySelector('.signin-form');
        const submitButton = document.querySelector('.btn-signin');

        if (!form || !submitButton) return;

        // Disable/enable all form inputs
        const inputs = form.querySelectorAll('input, button');
        inputs.forEach(input => {
            //input.disabled = !enabled;
        });

        // Update submit button text and state
        if (enabled) {
            submitButton.innerHTML = 'Sign In';
            submitButton.disabled = false;
        } else {
            submitButton.innerHTML = `
                <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                Signing In...
            `;
            submitButton.disabled = true;
        }
    }

    // ========================================
    // Simple Message Display
    // ========================================
    function showMessage(type, text) {
        if (text.toLowerCase().includes('license')) {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'error',
                    title: 'Access Issue',
                    text: text,
                    confirmButtonColor: '#dc2626',
                    footer: '<a href="mailto:support@yourcompany.com">Contact Support</a>'
                });
            } else {
                alert(text + '\n\nPlease contact support');
            }
        }
        else {
            if (typeof Swal !== 'undefined') {
                const config = {
                    error: { icon: 'error', title: 'Error', confirmButtonColor: '#dc2626' },
                    success: { icon: 'success', title: 'Success', confirmButtonColor: '#065f46' },
                    info: { icon: 'info', title: 'Information', confirmButtonColor: '#0369a1' },
                    warning: { icon: 'warning', title: 'Warning', confirmButtonColor: '#d97706' }
                };

                Swal.fire({
                    ...config[type],
                    text: text
                });
            } else {
                alert(text);
            }
        }
    }

    // ========================================
    // Real-time Field Validation
    // ========================================
    function initFieldValidation() {
        const inputs = document.querySelectorAll('.form-control');

        inputs.forEach(input => {
            // Clear errors on input
            input.addEventListener('input', function () {
                this.classList.remove('is-invalid', 'input-validation-error');
            });

            // Validate on blur
            input.addEventListener('blur', function () {
                if (this.value.trim() === '' && this.hasAttribute('required')) {
                    this.classList.add('is-invalid');
                } else if (this.value.trim() !== '') {
                    this.classList.remove('is-invalid');
                    this.classList.add('is-valid');
                }
            });
        });
    }

    // ========================================
    // Initialize Everything
    // ========================================
    function init() {
        try {
            initPasswordToggle();
            initFormSubmission();
            initFieldValidation();
            console.log('SignIn page initialized successfully');
        } catch (error) {
            console.error('Error initializing SignIn page:', error);
        }
    }

    init();
});