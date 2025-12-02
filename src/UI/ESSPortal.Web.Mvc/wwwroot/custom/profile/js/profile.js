/* ========================================
   UN Portal - Profile Management JavaScript
   Enhanced with better image resizing and error handling
   ======================================== */

document.addEventListener('DOMContentLoaded', function () {
    setupProfilePicturePreview();
    setupFormValidation();
    setupSuccessMessages();
    animateProgressBar();
});

// Profile Picture Preview and Resize Functionality
function setupProfilePicturePreview() {
    const profilePictureInput = document.getElementById('profilePictureInput');
    const imagePreview = document.getElementById('imagePreview');
    const imagePlaceholder = document.getElementById('imagePlaceholder');
    const uploadButton = document.getElementById('uploadButton');
    const form = document.getElementById('profilePictureForm');

    if (!profilePictureInput || !imagePreview || !imagePlaceholder || !uploadButton) return;

    profilePictureInput.addEventListener('change', async function (e) {
        const file = e.target.files[0];

        if (file) {
            try {
                console.log('Processing file:', file.name, 'Size:', file.size, 'Type:', file.type);

                // Validate file type first
                const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
                if (!allowedTypes.includes(file.type.toLowerCase())) {
                    showError('Only JPEG, PNG, GIF, and WebP images are allowed.');
                    resetFileInput();
                    return;
                }

                // Show loading state
                showImageProcessing(true);

                // Resize and compress the image
                const resizedFile = await resizeImage(file, {
                    maxWidth: 400,
                    maxHeight: 400,
                    quality: 0.8,
                    maxFileSize: 2 * 1024 * 1024 // 2MB after compression
                });

                if (!resizedFile) {
                    showError('Failed to process image. Please try a different image.');
                    resetFileInput();
                    showImageProcessing(false);
                    return;
                }

                console.log('Resized file size:', resizedFile.size);

                // Create preview URL
                const previewUrl = URL.createObjectURL(resizedFile);
                showImagePreview(previewUrl);

                // Replace the original file with resized version for form submission
                replaceFileInput(resizedFile, file.name);

                showImageProcessing(false);
                uploadButton.disabled = false;

            } catch (error) {
                console.error('Image processing error:', error);
                showError(`Failed to process image: ${error.message}. Please try again.`);
                resetFileInput();
                showImageProcessing(false);
            }
        } else {
            resetPreview();
        }
    });

    // Handle form submission to ensure we have the processed file
    if (form) {
        form.addEventListener('submit', function (e) {
            const fileInput = this.querySelector('input[type="file"]');
            if (!fileInput || !fileInput.files || fileInput.files.length === 0) {
                e.preventDefault();
                showError('Please select an image to upload.');
                return false;
            }
        });
    }

    function showImageProcessing(show) {
        if (show) {
            imagePlaceholder.innerHTML = `
                <div class="text-center">
                    <div class="spinner-border text-primary mb-2" role="status">
                        <span class="visually-hidden">Processing...</span>
                    </div>
                    <p class="placeholder-text">Processing image...</p>
                </div>
            `;
            uploadButton.disabled = true;
        }
    }

    function showImagePreview(src) {
        imagePreview.src = src;
        imagePreview.style.display = 'block';
        imagePlaceholder.style.display = 'none';
    }

    function resetPreview() {
        imagePreview.style.display = 'none';
        imagePlaceholder.style.display = 'flex';
        imagePlaceholder.innerHTML = `
            <i class="placeholder-icon">📷</i>
            <p class="placeholder-text">Select an image to preview</p>
        `;
        uploadButton.disabled = true;

        // Clean up any blob URLs
        if (imagePreview.src && imagePreview.src.startsWith('blob:')) {
            URL.revokeObjectURL(imagePreview.src);
        }
    }

    function resetFileInput() {
        profilePictureInput.value = '';
        resetPreview();
    }

    function replaceFileInput(processedFile, originalName) {
        try {
            // Create a new DataTransfer object to replace the file
            const dt = new DataTransfer();

            // Create a new file with the original name but processed content
            const newFile = new File([processedFile], originalName, {
                type: processedFile.type,
                lastModified: Date.now()
            });

            dt.items.add(newFile);
            profilePictureInput.files = dt.files;

            console.log('File replaced successfully:', newFile.name, newFile.size);
        } catch (error) {
            console.error('Error replacing file input:', error);
            throw new Error('Failed to prepare processed file for upload');
        }
    }
}

// Improved Image Resizing Function
async function resizeImage(file, options = {}) {
    const {
        maxWidth = 400,
        maxHeight = 400,
        quality = 0.8,
        maxFileSize = 2 * 1024 * 1024, // 2MB
        outputFormat = null // Let it auto-detect or preserve original format
    } = options;

    return new Promise((resolve, reject) => {
        try {
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            const img = new Image();

            // Set timeout to prevent hanging
            const timeout = setTimeout(() => {
                reject(new Error('Image processing timeout'));
            }, 30000); // 30 seconds timeout

            img.onload = function () {
                try {
                    clearTimeout(timeout);
                    console.log('Original image dimensions:', img.width, 'x', img.height);

                    // Calculate new dimensions maintaining aspect ratio
                    const { width, height } = calculateDimensions(img.width, img.height, maxWidth, maxHeight);
                    console.log('Target dimensions:', width, 'x', height);

                    // Set canvas dimensions
                    canvas.width = width;
                    canvas.height = height;

                    // Enable image smoothing for better quality
                    ctx.imageSmoothingEnabled = true;
                    ctx.imageSmoothingQuality = 'high';

                    // Fill with white background for JPEG (prevents transparency issues)
                    if (file.type === 'image/jpeg') {
                        ctx.fillStyle = '#FFFFFF';
                        ctx.fillRect(0, 0, width, height);
                    }

                    // Draw resized image
                    ctx.drawImage(img, 0, 0, width, height);

                    // Determine output format - preserve original format when possible
                    let finalFormat = outputFormat;
                    if (!finalFormat) {
                        // Use original format, but convert GIF to PNG to avoid animation issues
                        finalFormat = file.type === 'image/gif' ? 'image/png' : file.type;
                    }

                    // Try different quality levels if file is too large
                    compressImageWithQuality(canvas, finalFormat, quality, maxFileSize, resolve, reject);

                    // Clean up
                    URL.revokeObjectURL(img.src);
                } catch (error) {
                    clearTimeout(timeout);
                    console.error('Error in image processing:', error);
                    reject(error);
                }
            };

            img.onerror = function (error) {
                clearTimeout(timeout);
                console.error('Error loading image:', error);
                reject(new Error('Failed to load image file'));
            };

            // Create object URL for the image
            img.src = URL.createObjectURL(file);

        } catch (error) {
            console.error('Error setting up image processing:', error);
            reject(error);
        }
    });
}

// Helper function to compress image with different quality levels
function compressImageWithQuality(canvas, format, initialQuality, maxFileSize, resolve, reject) {
    let currentQuality = initialQuality;
    const minQuality = 0.1;
    const qualityStep = 0.1;

    function attemptCompression() {
        canvas.toBlob((blob) => {
            if (!blob) {
                reject(new Error('Failed to create image blob'));
                return;
            }

            console.log(`Compressed with quality ${currentQuality}: ${blob.size} bytes`);

            // If file size is acceptable or we've reached minimum quality, resolve
            if (blob.size <= maxFileSize || currentQuality <= minQuality) {
                resolve(blob);
            } else {
                // Try with lower quality
                currentQuality = Math.max(minQuality, currentQuality - qualityStep);
                attemptCompression();
            }
        }, format, currentQuality);
    }

    attemptCompression();
}

// Calculate new dimensions maintaining aspect ratio
function calculateDimensions(originalWidth, originalHeight, maxWidth, maxHeight) {
    let width = originalWidth;
    let height = originalHeight;

    // Calculate scaling factor
    const widthRatio = maxWidth / originalWidth;
    const heightRatio = maxHeight / originalHeight;
    const ratio = Math.min(widthRatio, heightRatio);

    // Only resize if image is larger than max dimensions
    if (ratio < 1) {
        width = Math.round(originalWidth * ratio);
        height = Math.round(originalHeight * ratio);
    }

    return { width, height };
}

// Enhanced Form Validation
function setupFormValidation() {
    const forms = document.querySelectorAll('form[id$="Form"]');

    forms.forEach(form => {
        // Remove browser default validation styling
        form.setAttribute('novalidate', 'true');

        form.addEventListener('submit', function (e) {
            if (!validateForm(this)) {
                e.preventDefault();
                e.stopPropagation();
            } else {
                // Show loading state for submit button
                const submitButton = this.querySelector('button[type="submit"]');
                if (submitButton) {
                    showLoadingState(submitButton);
                }
            }
        });

        // Real-time validation
        const inputs = form.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            input.addEventListener('blur', () => validateField(input));
            input.addEventListener('input', () => clearFieldError(input));
        });
    });
}

function validateForm(form) {
    const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
    let isValid = true;

    inputs.forEach(input => {
        if (!validateField(input)) {
            isValid = false;
        }
    });

    return isValid;
}

function validateField(input) {
    const value = input.value.trim();
    let isValid = true;
    let errorMessage = '';

    // Required field validation
    if (input.hasAttribute('required') && !value) {
        isValid = false;
        errorMessage = 'This field is required.';
    }
    // Email validation
    else if (input.type === 'email' && value) {
        const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailPattern.test(value)) {
            isValid = false;
            errorMessage = 'Please enter a valid email address.';
        }
    }
    // Phone validation
    else if (input.type === 'tel' && value) {
        const phonePattern = /^[\+]?[\d\s\-\(\)]{7,15}$/;
        if (!phonePattern.test(value)) {
            isValid = false;
            errorMessage = 'Please enter a valid phone number.';
        }
    }

    // Update field styling and show/hide error
    updateFieldValidation(input, isValid, errorMessage);

    return isValid;
}

function updateFieldValidation(input, isValid, errorMessage) {
    const formGroup = input.closest('.mb-3') || input.parentElement;

    // Remove existing validation classes and error messages
    input.classList.remove('is-valid', 'is-invalid');
    const existingError = formGroup.querySelector('.invalid-feedback');
    if (existingError) {
        existingError.remove();
    }

    if (!isValid && errorMessage) {
        input.classList.add('is-invalid');

        // Add error message
        const errorDiv = document.createElement('div');
        errorDiv.className = 'invalid-feedback';
        errorDiv.textContent = errorMessage;
        formGroup.appendChild(errorDiv);

        // Focus on first invalid field
        if (input === document.querySelector('.is-invalid')) {
            input.focus();
        }
    } else if (input.value.trim()) {
        input.classList.add('is-valid');
    }
}

function clearFieldError(input) {
    input.classList.remove('is-invalid');
    const formGroup = input.closest('.mb-3') || input.parentElement;
    const errorMsg = formGroup.querySelector('.invalid-feedback');
    if (errorMsg) {
        errorMsg.remove();
    }
}

// Success Messages and Modal Management
function setupSuccessMessages() {
    // Auto-hide alerts
    const alerts = document.querySelectorAll('.alert-success, .alert-danger, .alert-info');
    alerts.forEach(alert => {
        setTimeout(() => {
            fadeOutElement(alert);
        }, 5000);
    });

    // Modal reset handling
    const modals = document.querySelectorAll('.modal');
    modals.forEach(modal => {
        modal.addEventListener('hidden.bs.modal', function () {
            resetModal(this);
        });
    });
}

function resetModal(modal) {
    const form = modal.querySelector('form');
    if (form) {
        form.reset();

        // Clear validation states
        const fields = form.querySelectorAll('.is-valid, .is-invalid');
        fields.forEach(field => {
            field.classList.remove('is-valid', 'is-invalid');
        });

        // Remove error messages
        const errors = form.querySelectorAll('.invalid-feedback');
        errors.forEach(error => error.remove());
    }

    // Reset profile picture specific elements
    if (modal.id === 'profilePictureModal') {
        const imagePreview = document.getElementById('imagePreview');
        const imagePlaceholder = document.getElementById('imagePlaceholder');
        const uploadButton = document.getElementById('uploadButton');

        if (imagePreview && imagePlaceholder && uploadButton) {
            if (imagePreview.src && imagePreview.src.startsWith('blob:')) {
                URL.revokeObjectURL(imagePreview.src);
            }
            imagePreview.style.display = 'none';
            imagePlaceholder.style.display = 'flex';
            imagePlaceholder.innerHTML = `
                <i class="placeholder-icon">📷</i>
                <p class="placeholder-text">Select an image to preview</p>
            `;
            uploadButton.disabled = true;
        }
    }
}

// Utility Functions
function showLoadingState(button) {
    if (!button) return;

    const originalContent = button.innerHTML;
    button.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status"></span>Processing...';
    button.disabled = true;

    // Store original content for potential restoration
    button.dataset.originalContent = originalContent;
}

function showError(message) {
    console.error('Error:', message);

    // Create or update error alert
    let alertContainer = document.querySelector('.alert-container');
    if (!alertContainer) {
        alertContainer = document.createElement('div');
        alertContainer.className = 'alert-container position-fixed top-0 start-50 translate-middle-x';
        alertContainer.style.zIndex = '9999';
        alertContainer.style.marginTop = '20px';
        document.body.appendChild(alertContainer);
    }

    const alert = document.createElement('div');
    alert.className = 'alert alert-danger alert-dismissible fade show';
    alert.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    alertContainer.appendChild(alert);

    // Auto-hide after 7 seconds (longer for error messages)
    setTimeout(() => fadeOutElement(alert), 7000);
}

function fadeOutElement(element) {
    if (!element) return;

    element.style.transition = 'opacity 0.3s ease';
    element.style.opacity = '0';

    setTimeout(() => {
        if (element.parentNode) {
            element.parentNode.removeChild(element);
        }
    }, 300);
}

function animateProgressBar() {
    const progressBar = document.querySelector('.progress-bar');
    if (!progressBar) return;

    const targetWidth = progressBar.style.width || progressBar.getAttribute('style')?.match(/width:\s*(\d+%)/)?.[1];
    if (!targetWidth) return;

    progressBar.style.width = '0%';
    progressBar.style.transition = 'width 1s ease-in-out';

    setTimeout(() => {
        progressBar.style.width = targetWidth;
    }, 200);
}