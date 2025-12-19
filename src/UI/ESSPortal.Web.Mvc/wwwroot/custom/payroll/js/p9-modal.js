// P9 Modal Handler (Modal form only)
document.addEventListener('DOMContentLoaded', function () {

    // Handle P9 form submission (modal form)
    document.addEventListener('submit', function (event) {
        if (event.target.id === 'p9GenerationForm') {

            // Prevent normal form submission
            event.preventDefault();
            event.stopPropagation();
            event.stopImmediatePropagation();

            const form = event.target;
            const yearSelect = form.querySelector('select[name="Year"]');
            const submitBtn = form.querySelector('button[type="submit"]');

            // Find close and cancel buttons
            const modal = form.closest('.modal');
            const closeBtn = modal ? modal.querySelector('.btn-close') : null;
            const cancelBtn = modal ? modal.querySelector('#p9CancelBtn') : null;

            // Validation
            if (!yearSelect?.value) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'error',
                        title: 'Validation Error',
                        text: 'Please select a year.',
                        confirmButtonColor: '#dc3545'
                    });
                } else {
                    alert('Please select a year.');
                }
                return false;
            }


            // Disable modal controls during generation
            disableModalControls(submitBtn, closeBtn, cancelBtn, modal);

            // Create form data
            const formData = new FormData(form);

            // Submit via AJAX
            fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
                .then(response => {
                    console.log('P9 response received:', response.status);
                    if (!response.ok) {
                        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                    }
                    return response.json();
                })
                .then(result => {
                    console.log('P9 response data:', result);

                    if (result.success) {
                        console.log('P9 success! Starting download...');

                        // Handle file download from base64 data
                        downloadBase64File(result.fileData, result.fileName);

                        // Close modal after successful download
                        const modalElement = document.getElementById('p9Modal');
                        if (modalElement) {
                            const modalInstance = bootstrap.Modal.getInstance(modalElement);
                            if (modalInstance) modalInstance.hide();
                        }

                        // Success message
                        showSuccessMessage('P9 certificate downloaded successfully');

                    } else {
                        throw new Error(result.message || 'Failed to generate P9 certificate');
                    }
                })
                .catch(error => {
                    console.error('P9 error:', error);
                    showErrorMessage(error.message);
                })
                .finally(() => {
                    // Re-enable modal controls
                    enableModalControls(submitBtn, closeBtn, cancelBtn, modal);
                });

            return false;
        }
    }, true);

    // Helper function to disable modal controls
    function disableModalControls(submitBtn, closeBtn, cancelBtn, modal) {
        console.log('Disabling modal controls during generation...');

        // Disable submit button
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Generating...';
        }

        // Disable close button (X)
        if (closeBtn) {
            closeBtn.disabled = true;
            closeBtn.style.opacity = '0.5';
            closeBtn.style.cursor = 'not-allowed';
            closeBtn.style.pointerEvents = 'none';
            // Store original dismiss attribute
            closeBtn.setAttribute('data-original-dismiss', closeBtn.getAttribute('data-bs-dismiss') || '');
            closeBtn.removeAttribute('data-bs-dismiss');
        }

        // Disable cancel button
        if (cancelBtn) {
            cancelBtn.disabled = true;
            cancelBtn.style.opacity = '0.5';
            cancelBtn.style.cursor = 'not-allowed';
            cancelBtn.style.pointerEvents = 'none';
            cancelBtn.setAttribute('data-original-html', cancelBtn.innerHTML);
            cancelBtn.innerHTML = '<i class="me-2">⏳</i><span>Please wait...</span>';
            cancelBtn.setAttribute('data-original-dismiss', cancelBtn.getAttribute('data-bs-dismiss') || '');
            cancelBtn.removeAttribute('data-bs-dismiss');
        }

        // Store modal instance for later restoration
        if (modal) {
            const modalInstance = bootstrap.Modal.getInstance(modal);
            if (modalInstance) {
                // Store original settings
                modal.setAttribute('data-original-backdrop', modalInstance._config.backdrop.toString());
                modal.setAttribute('data-original-keyboard', modalInstance._config.keyboard.toString());

                // Disable backdrop and keyboard ONLY during processing
                modalInstance._config.backdrop = 'static';
                modalInstance._config.keyboard = false;
            }

            // Create a hide blocker function
            const blockModalHide = function (e) {
                console.log('Blocking modal hide during generation');
                e.preventDefault();
                e.stopPropagation();
                e.stopImmediatePropagation();
                return false;
            };

            // Store the blocker function reference on the modal for later removal
            modal._hideBlocker = blockModalHide;
            modal.addEventListener('hide.bs.modal', blockModalHide);
        }
    }


    // Helper function to enable modal controls
    function enableModalControls(submitBtn, closeBtn, cancelBtn, modal) {
        console.log('Re-enabling modal controls...');

        // Remove hide blocker FIRST
        if (modal && modal._hideBlocker) {
            modal.removeEventListener('hide.bs.modal', modal._hideBlocker);
            delete modal._hideBlocker;
        }

        // Re-enable submit button
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = '<i class="me-2">📄</i>Generate & Download';
        }

        // Re-enable close button
        if (closeBtn) {
            closeBtn.disabled = false;
            closeBtn.style.opacity = '';
            closeBtn.style.cursor = '';
            closeBtn.style.pointerEvents = '';

            // Restore original dismiss attribute
            const originalDismiss = closeBtn.getAttribute('data-original-dismiss');
            if (originalDismiss) {
                closeBtn.setAttribute('data-bs-dismiss', originalDismiss);
                closeBtn.removeAttribute('data-original-dismiss');
            }
        }

        // Re-enable cancel button
        if (cancelBtn) {
            cancelBtn.disabled = false;
            cancelBtn.style.opacity = '';
            cancelBtn.style.cursor = '';
            cancelBtn.style.pointerEvents = '';

            // Restore original HTML
            const originalHtml = cancelBtn.getAttribute('data-original-html');
            if (originalHtml) {
                cancelBtn.innerHTML = originalHtml;
                cancelBtn.removeAttribute('data-original-html');
            } else {
                cancelBtn.innerHTML = '<i class="me-2">❌</i>Cancel';
            }

            // Restore original dismiss attribute
            const originalDismiss = cancelBtn.getAttribute('data-original-dismiss');
            if (originalDismiss) {
                cancelBtn.setAttribute('data-bs-dismiss', originalDismiss);
                cancelBtn.removeAttribute('data-original-dismiss');
            }
        }

        // Restore modal backdrop and keyboard behavior
        if (modal) {
            const modalInstance = bootstrap.Modal.getInstance(modal);
            if (modalInstance) {
                // Restore original settings
                const originalBackdrop = modal.getAttribute('data-original-backdrop');
                const originalKeyboard = modal.getAttribute('data-original-keyboard');

                modalInstance._config.backdrop = originalBackdrop === 'static' ? 'static' : true;
                modalInstance._config.keyboard = originalKeyboard === 'false' ? false : true;

                // Clean up stored attributes
                modal.removeAttribute('data-original-backdrop');
                modal.removeAttribute('data-original-keyboard');
            }
        }

        console.log('Modal controls restored to normal behavior');
    }

    // Helper function to download base64 file
    function downloadBase64File(base64Data, fileName) {
        const byteArray = new Uint8Array(
            atob(base64Data).split('').map(c => c.charCodeAt(0))
        );
        const blob = new Blob([byteArray], { type: 'application/pdf' });
        const downloadUrl = URL.createObjectURL(blob);

        const downloadLink = document.createElement('a');
        downloadLink.href = downloadUrl;
        downloadLink.download = fileName;
        document.body.appendChild(downloadLink);
        downloadLink.click();
        document.body.removeChild(downloadLink);
        URL.revokeObjectURL(downloadUrl);
    }

    // Helper function to show success message
    function showSuccessMessage(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'success',
                title: 'Success!',
                text: message,
                timer: 3000,
                showConfirmButton: false
            });
        } else {
            alert(message);
        }
    }

    // Helper function to show error message
    function showErrorMessage(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: message,
                confirmButtonColor: '#dc3545'
            });
        } else {
            alert('Error: ' + message);
        }
    }

});