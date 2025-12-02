/**
 * UN Portal - Register Page JavaScript
 * Employee Number + Password Registration System
 */

document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    // ========================================
    // FORM SUBMISSION MODULE
    // ========================================
    const FormSubmission = {
        init() {
            const form = document.querySelector('.signin-form');
            const submitButton = document.querySelector('#registerSubmitBtn');

            if (!form || !submitButton) return;

            form.addEventListener('submit', (event) => {
                this.handleFormSubmit(event, form, submitButton);
            });
        },

        handleFormSubmit(event, form, submitButton) {
            const employeeNumber = form.querySelector('input[name="EmployeeNumber"]')?.value?.trim();
            const password = form.querySelector('input[name="Password"]')?.value;
            const confirmPassword = form.querySelector('input[name="ConfirmPassword"]')?.value;

            // Validate required fields
            if (!this.validateRequiredFields(event, form, employeeNumber, password, confirmPassword)) {
                return false;
            }

            // jQuery validation check if available
            if (typeof $ !== 'undefined' && $.fn.validate && $(form).data('validator')) {
                if (!$(form).valid()) {
                    event.preventDefault();
                    ValidationHelper.showValidationError('Please correct the highlighted fields');
                    return false;
                }
            }

            // Show loading state
            LoadingHelper.show(submitButton, 'Creating Account...');

            // Safety timeout to re-enable button
            setTimeout(() => {
                if (submitButton.disabled) {
                    LoadingHelper.hide(submitButton, 'Create Account');
                }
            }, 30000);

            return true;
        },

        validateRequiredFields(event, form, employeeNumber, password, confirmPassword) {
            if (!employeeNumber) {
                event.preventDefault();
                ValidationHelper.showValidationError('Please enter your employee number');
                const employeeNumberInput = form.querySelector('input[name="EmployeeNumber"]');
                if (employeeNumberInput) {
                    ValidationHelper.showFieldError(employeeNumberInput, 'Employee number is required');
                    employeeNumberInput.focus();
                }
                return false;
            }

            if (!password) {
                event.preventDefault();
                ValidationHelper.showValidationError('Please enter a password');
                const passwordInput = form.querySelector('input[name="Password"]');
                if (passwordInput) {
                    ValidationHelper.showFieldError(passwordInput, 'Password is required');
                    passwordInput.focus();
                }
                return false;
            }

            if (!PasswordStrength.validate(password)) {
                event.preventDefault();
                ValidationHelper.showValidationError('Password does not meet the requirements');
                const passwordInput = form.querySelector('input[name="Password"]');
                if (passwordInput) {
                    ValidationHelper.showFieldError(passwordInput, 'Password does not meet requirements');
                    passwordInput.focus();
                }
                return false;
            }

            if (password !== confirmPassword) {
                event.preventDefault();
                ValidationHelper.showValidationError('Passwords do not match');
                const confirmPasswordInput = form.querySelector('input[name="ConfirmPassword"]');
                if (confirmPasswordInput) {
                    ValidationHelper.showFieldError(confirmPasswordInput, 'Passwords do not match');
                    confirmPasswordInput.focus();
                }
                return false;
            }

            return true;
        }
    };

    // ========================================
    // PASSWORD HANDLING MODULE
    // ========================================
    const PasswordHandling = {
        init() {
            this.initPasswordToggle();
            this.initPasswordStrengthFeedback();
            this.initPasswordValidation();
        },

        initPasswordToggle() {
            const passwordToggles = document.querySelectorAll('.password-toggle');

            passwordToggles.forEach(toggle => {
                toggle.addEventListener('click', (e) => {
                    e.preventDefault();
                    this.togglePasswordVisibility(toggle);
                });

                toggle.setAttribute('aria-label', 'Show password');
                toggle.setAttribute('tabindex', '0');

                // Keyboard support
                toggle.addEventListener('keydown', (e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        toggle.click();
                    }
                });
            });
        },

        togglePasswordVisibility(toggle) {
            const passwordInput = toggle.parentElement.querySelector('input');
            const eyeIcon = toggle.querySelector('.eye-icon');

            if (passwordInput && eyeIcon) {
                if (passwordInput.type === 'password') {
                    passwordInput.type = 'text';
                    eyeIcon.textContent = '🙈';
                    toggle.setAttribute('aria-label', 'Hide password');
                } else {
                    passwordInput.type = 'password';
                    eyeIcon.textContent = '👁️';
                    toggle.setAttribute('aria-label', 'Show password');
                }
            }
        },

        initPasswordStrengthFeedback() {
            const passwordInput = document.querySelector('input[name="Password"]');
            const requirementsList = document.querySelector('.requirements-list');

            if (!passwordInput || !requirementsList) return;

            const requirements = requirementsList.querySelectorAll('li');

            passwordInput.addEventListener('input', () => {
                PasswordStrength.validate(passwordInput.value, requirements);
            });
        },

        initPasswordValidation() {
            const passwordInput = document.querySelector('input[name="Password"]');
            const confirmPasswordInput = document.querySelector('input[name="ConfirmPassword"]');

            if (!passwordInput || !confirmPasswordInput) return;

            confirmPasswordInput.addEventListener('blur', () => {
                if (confirmPasswordInput.value && confirmPasswordInput.value !== passwordInput.value) {
                    ValidationHelper.showFieldError(confirmPasswordInput, 'Passwords do not match');
                }
            });

            confirmPasswordInput.addEventListener('input', () => {
                if (confirmPasswordInput.value === passwordInput.value) {
                    ValidationHelper.clearFieldError(confirmPasswordInput);
                }
            });

            passwordInput.addEventListener('input', () => {
                if (confirmPasswordInput.value && confirmPasswordInput.value !== passwordInput.value) {
                    ValidationHelper.showFieldError(confirmPasswordInput, 'Passwords do not match');
                } else if (confirmPasswordInput.value === passwordInput.value) {
                    ValidationHelper.clearFieldError(confirmPasswordInput);
                }
            });
        }
    };

    // ========================================
    // PASSWORD STRENGTH MODULE
    // ========================================
    const PasswordStrength = {
        validate(password, requirementElements = null) {
            const checks = [
                password.length >= 8,                    // At least 8 characters
                /[A-Z]/.test(password),                 // Uppercase letter
                /[a-z]/.test(password),                 // Lowercase letter
                /\d/.test(password),                    // Number
                /[@$!%*?&]/.test(password)              // Special character
            ];

            this.updateVisualFeedback(checks, requirementElements);
            return checks.every(Boolean);
        },

        updateVisualFeedback(checks, requirementElements) {
            // Update visual feedback if requirement elements exist
            if (requirementElements) {
                requirementElements.forEach((req, index) => {
                    this.updateRequirementElement(req, checks[index]);
                });
            }

            // Also check by data-requirement attribute
            const requirements = {
                'length': checks[0],
                'uppercase': checks[1],
                'lowercase': checks[2],
                'number': checks[3],
                'special': checks[4]
            };

            Object.keys(requirements).forEach(req => {
                const element = document.querySelector(`[data-requirement="${req}"]`);
                if (element) {
                    element.setAttribute('data-valid', requirements[req]);
                    this.updateRequirementElement(element, requirements[req]);
                }
            });
        },

        updateRequirementElement(element, isValid) {
            if (isValid) {
                element.style.color = '#065f46';
                element.style.fontWeight = '600';
                element.setAttribute('data-valid', 'true');
            } else {
                element.style.color = '#6b7280';
                element.style.fontWeight = '400';
                element.setAttribute('data-valid', 'false');
            }
        }
    };

    // ========================================
    // VALIDATION HELPER MODULE
    // ========================================
    const ValidationHelper = {
        init() {
            this.initValidationSummary();
            this.initFieldValidation();
            this.initRealTimeValidation();
        },

        initValidationSummary() {
            const validationSummary = document.querySelector('.validation-summary');
            if (validationSummary && validationSummary.querySelector('ul li')) {
                validationSummary.classList.add('show');
            }
        },

        initFieldValidation() {
            const errorFields = document.querySelectorAll('.input-validation-error');
            errorFields.forEach(field => {
                field.addEventListener('input', () => {
                    this.clearFieldError(field);
                });
            });
        },

        initRealTimeValidation() {
            const employeeNumberInput = document.querySelector('input[name="EmployeeNumber"]');

            if (employeeNumberInput) {
                employeeNumberInput.addEventListener('blur', () => {
                    const value = employeeNumberInput.value.trim();

                    if (!value) {
                        this.showFieldError(employeeNumberInput, 'Employee number is required');
                    }
                    else if (!value.startsWith('SN')) {
                        this.showFieldError(employeeNumberInput, 'Employee number must start with "SN"');
                    }
                    else if (value.length < 4) {
                        this.showFieldError(employeeNumberInput, 'Employee number must be at least 4 characters');
                    }
                    else if (value.length > 10) {
                        this.showFieldError(employeeNumberInput, 'Employee number must not exceed 10 characters');
                    }
                    else if (!/^SN[a-zA-Z0-9]+$/.test(value)) {
                        this.showFieldError(employeeNumberInput, 'Employee number can only contain alphanumeric characters');
                    }
                });

                employeeNumberInput.addEventListener('input', () => {
                    const value = employeeNumberInput.value.trim();

                    if (value && value.startsWith('SN') && value.length >= 4 && value.length <= 10 && /^SN[a-zA-Z0-9]+$/.test(value)) {
                        this.clearFieldError(employeeNumberInput);
                    }
                });
            }
        },

        showFieldError(input, message) {
            if (!input) return;

            input.classList.add('input-validation-error');

            let errorSpan = input.parentElement.querySelector('.field-validation-error');
            if (!errorSpan) {
                errorSpan = input.parentElement.parentElement?.querySelector('.field-validation-error');
            }

            if (errorSpan) {
                errorSpan.textContent = message;
                errorSpan.style.display = 'block';
            }
        },

        clearFieldError(input) {
            if (!input) return;

            input.classList.remove('input-validation-error');

            let errorSpan = input.parentElement.querySelector('.field-validation-error');
            if (!errorSpan) {
                errorSpan = input.parentElement.parentElement?.querySelector('.field-validation-error');
            }

            if (errorSpan) {
                errorSpan.style.display = 'none';
            }
        },

        showValidationError(message) {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'error',
                    title: 'Validation Error',
                    text: message,
                    confirmButtonColor: '#dc2626'
                });
            } else if (window.showToast) {
                window.showToast(message, 'error');
            } else {
                alert(message);
            }
        }
    };

    // ========================================
    // UI HELPER MODULES
    // ========================================
    const LoadingHelper = {
        show(button, text = 'Loading...') {
            if (!button) return;

            button.disabled = true;
            button.classList.add('loading');

            const btnText = button.querySelector('.btn-text');
            if (btnText) {
                btnText.textContent = text;
            } else {
                button.innerHTML = `<span class="spinner-border spinner-border-sm me-2"></span>${text}`;
            }
        },

        hide(button, text = 'Submit') {
            if (!button) return;

            button.disabled = false;
            button.classList.remove('loading');

            const btnText = button.querySelector('.btn-text');
            if (btnText) {
                btnText.textContent = text;
            } else {
                button.innerHTML = text;
            }
        }
    };

    const ToastHelper = {
        show(message, type = 'info') {
            if (window.showToast) {
                window.showToast(message, type);
            }
        }
    };

    const AlertAutoHide = {
        init() {
            const alerts = document.querySelectorAll('.alert');

            alerts.forEach(alert => {
                setTimeout(() => {
                    alert.style.opacity = '0';
                    setTimeout(() => {
                        if (alert.parentNode) {
                            alert.style.display = 'none';
                        }
                    }, 300);
                }, 7000);
            });
        }
    };

    // ========================================
    // ACCESSIBILITY MODULE
    // ========================================
    const Accessibility = {
        init() {
            this.setupForm();
            this.setupLiveRegion();
        },

        setupForm() {
            const form = document.querySelector('.signin-form');
            if (form) {
                form.setAttribute('novalidate', 'novalidate');
            }
        },

        setupLiveRegion() {
            if (!document.querySelector('[aria-live="polite"]')) {
                const liveRegion = document.createElement('div');
                liveRegion.setAttribute('aria-live', 'polite');
                liveRegion.setAttribute('aria-atomic', 'true');
                liveRegion.className = 'sr-only';
                liveRegion.style.cssText = 'position:absolute;width:1px;height:1px;padding:0;margin:-1px;overflow:hidden;clip:rect(0,0,0,0);white-space:nowrap;border:0;';
                document.body.appendChild(liveRegion);

                window.announceToScreenReader = function (message) {
                    liveRegion.textContent = message;
                    setTimeout(() => {
                        liveRegion.textContent = '';
                    }, 1000);
                };
            }
        }
    };

    // ========================================
    // MAIN INITIALIZATION
    // ========================================
    function init() {
        try {
            FormSubmission.init();
            PasswordHandling.init();
            ValidationHelper.init();
            AlertAutoHide.init();
            Accessibility.init();

            console.log('Register page initialized successfully');
        } catch (error) {
            console.error('Error initializing register page:', error);
        }
    }

    // Start the application
    init();
});