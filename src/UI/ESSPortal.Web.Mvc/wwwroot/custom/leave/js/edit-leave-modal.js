/* ========================================
   UN Portal - Edit Leave Modal JavaScript
   ========================================
   
   This file handles the Edit Leave Modal functionality.
   It initializes when the modal content is dynamically loaded via AJAX.
   
   Dependencies:
   - Bootstrap 5 (for modal)
   - SweetAlert2 (optional, for alerts)
   
   Usage:
   - Include this file in _Layout.cshtml or LeaveHistory.cshtml
   - The modal will auto-initialize when loaded via DashboardModals.loadEditLeaveModal()
*/

const EditLeaveModal = (function () {
    'use strict';

    let isInitialized = false;
    let modalElement = null;

    // Form elements (set during initialization)
    let form = null;
    let leaveTypeSelect = null;
    let fromDate = null;
    let toDate = null;
    let daysApplied = null;
    let resumptionDate = null;
    let halfDayToggle = null;
    let leaveAllowanceToggle = null;
    let relieverSearch = null;
    let relieverList = null;
    let leaveBalanceInfo = null;
    let leaveBalanceAlert = null;

    /**
     * Initialize the Edit Leave Modal
     * Called after modal content is loaded via AJAX
     */
    function init() {
        console.log('EditLeaveModal: Initializing...');

        // Find the modal element
        modalElement = document.getElementById('editLeaveModal');
        if (!modalElement) {
            console.log('EditLeaveModal: Modal element not found, waiting...');
            return false;
        }

        // Cache form elements
        cacheFormElements();

        if (!form) {
            console.error('EditLeaveModal: Form not found');
            return false;
        }

        // Setup all event listeners
        setupLeaveTypeHandler();
        setupDateHandlers();
        setupHalfDayHandler();
        setupRelieverSelector();
        setupFormSubmission();

        // Initial state setup
        initializeExistingData();

        isInitialized = true;
        console.log('EditLeaveModal: Initialized successfully');
        return true;
    }

    /**
     * Cache all form elements for quick access
     */
    function cacheFormElements() {
        form = document.getElementById('editLeaveApplicationForm');
        leaveTypeSelect = document.querySelector('#editLeaveModal select[name="LeaveType"]');
        fromDate = document.querySelector('#editLeaveModal input[name="FromDate"]');
        toDate = document.querySelector('#editLeaveModal input[name="ToDate"]');
        daysApplied = document.querySelector('#editLeaveModal input[name="DaysApplied"]');
        resumptionDate = document.querySelector('#editLeaveModal input[name="ResumptionDate"]');

        //halfDayToggle = document.querySelector('#editLeaveModal input[name="HalfDay"]');
        halfDayToggle = document.querySelector('#editLeaveModal input[name="HalfDay"][type="checkbox"]');

        leaveAllowanceToggle = document.querySelector('#editLeaveModal input[name="LeaveAllowancePayable"][type="checkbox"]');
        relieverSearch = document.getElementById('relieverSearch');
        relieverList = document.getElementById('relieverList');
        leaveBalanceInfo = document.getElementById('leaveBalanceInfo');
        leaveBalanceAlert = document.querySelector('#editLeaveModal .alert.alert-info[data-annual-earned]');
    }

    /**
     * Initialize with existing data from the loaded form
     */
    function initializeExistingData() {
        // Update balance display for pre-selected leave type
        if (leaveTypeSelect && leaveTypeSelect.value) {
            updateLeaveBalanceDisplay();
        }

        // Calculate resumption date if dates are pre-filled
        if (fromDate && fromDate.value && toDate && toDate.value) {
            updateCalculations();
        }

        // Show half-day helper if already checked
        if (halfDayToggle && halfDayToggle.checked) {
            showHalfDayHelper();
        }

        // Highlight selected reliever
        highlightSelectedReliever();
    }

    // ===========================================
    // LEAVE TYPE & BALANCE
    // ===========================================

    function setupLeaveTypeHandler() {
        if (!leaveTypeSelect) return;

        leaveTypeSelect.addEventListener('change', function () {
            updateLeaveBalanceDisplay();
            updateCalculations();
        });
    }

    function updateLeaveBalanceDisplay() {
        if (!leaveTypeSelect || !leaveBalanceInfo || !leaveBalanceAlert) return;

        const selectedValue = leaveTypeSelect.value;
        if (!selectedValue) {
            leaveBalanceInfo.innerHTML = 'Select a leave type to view your available balance';
            return;
        }

        const leaveTypeMap = {
            'ANNUAL': { name: 'Annual Leave', balanceKey: 'annualBalance', earnedKey: 'annualEarned' },
            'ADOPTION': { name: 'Adoption Leave', balanceKey: 'adoptionBalance' },
            'COMPASSIONATE': { name: 'Compassion Leave', balanceKey: 'compassionBalance' },
            'MATERNITY': { name: 'Maternity Leave', balanceKey: 'maternityBalance' },
            'PATERNITY': { name: 'Paternity Leave', balanceKey: 'paternityBalance' },
            'SICK': { name: 'Sick Leave', balanceKey: 'sickBalance' },
            'STUDY': { name: 'Study Leave', balanceKey: 'studyBalance' },
            'UNPAID': { name: 'Unpaid Leave', balanceKey: 'unpaidBalance' }
        };

        const leaveInfo = leaveTypeMap[selectedValue.toUpperCase()];
        if (!leaveInfo) {
            leaveBalanceInfo.innerHTML = 'Leave type information not available';
            return;
        }

        const balance = leaveBalanceAlert.dataset[leaveInfo.balanceKey] || '0';

        if (selectedValue.toUpperCase() === 'ANNUAL') {
            const earned = leaveBalanceAlert.dataset[leaveInfo.earnedKey] || '0';
            leaveBalanceInfo.innerHTML = `<strong>${leaveInfo.name}:</strong> ${earned} days earned, <strong>${balance} days available</strong>`;
        } else {
            leaveBalanceInfo.innerHTML = `<strong>${leaveInfo.name}:</strong> <strong>${balance} days available</strong>`;
        }
    }

    // ===========================================
    // DATE CALCULATIONS
    // ===========================================

    function setupDateHandlers() {
        if (fromDate) {
            fromDate.addEventListener('change', function () {
                if (halfDayToggle && halfDayToggle.checked && this.value) {
                    toDate.value = this.value;
                    toDate.min = this.value;
                    toDate.max = this.value;
                } else if (this.value) {
                    toDate.min = this.value;
                    toDate.removeAttribute('max');
                }
                updateCalculations();
            });
        }

        if (toDate) {
            toDate.addEventListener('change', updateCalculations);
        }
    }

    function updateCalculations() {
        if (!fromDate || !toDate || !daysApplied) return;
        if (!fromDate.value || !toDate.value) return;

        const startDate = new Date(fromDate.value);
        const endDate = new Date(toDate.value);

        // Validate date range
        if (endDate < startDate) {
            daysApplied.value = '';
            if (resumptionDate) resumptionDate.value = '';
            showFieldError(toDate, 'End date must be after or same as start date');
            return;
        }

        clearFieldError(toDate);

        // Calculate days
        let calculatedDays;
        if (halfDayToggle && halfDayToggle.checked) {
            calculatedDays = 0.5;
        } else if (usesCalendarDays()) {
            calculatedDays = calculateCalendarDays(startDate, endDate);
        } else {
            calculatedDays = calculateBusinessDays(startDate, endDate);
        }

        daysApplied.value = calculatedDays;

        // Calculate resumption date
        if (resumptionDate) {

            let resumption;

            if (usesCalendarDays()) {

                resumption = new Date(endDate);

                resumption = getNextBusinessDay(endDate);
                //resumption.setDate(resumption.getDate() + 1);

            } else {

                resumption = getNextBusinessDay(endDate);
            }

            resumptionDate.value = formatShortDisplayDate(resumption);
            //resumptionDate.value = formatDateForInput(resumption);
        }
    }

    function usesCalendarDays() {
        if (!leaveTypeSelect || !leaveTypeSelect.value) return false;
        const selectedOption = leaveTypeSelect.options[leaveTypeSelect.selectedIndex];
        return selectedOption ? selectedOption.getAttribute('data-uses-calendar') === 'true' : false;
    }

    function calculateBusinessDays(start, end) {
        let count = 0;
        const current = new Date(start);

        while (current <= end) {
            const dayOfWeek = current.getDay();
            if (dayOfWeek !== 0 && dayOfWeek !== 6) {
                count++;
            }
            current.setDate(current.getDate() + 1);
        }

        return count;
    }

    function calculateCalendarDays(start, end) {
        const diffTime = Math.abs(end - start);
        return Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
    }

    function getNextBusinessDay(date) {
        const nextDay = new Date(date);
        nextDay.setDate(nextDay.getDate() + 1);

        while (nextDay.getDay() === 0 || nextDay.getDay() === 6) {
            nextDay.setDate(nextDay.getDate() + 1);
        }

        return nextDay;
    }

    function formatDateForInput(date) {
        return date.toISOString().split('T')[0];
    }

    function formatLongDisplayDate(date) {
        return date.toLocaleDateString('en-US', {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    }

    function formatShortDisplayDate(date) {
        return date.toLocaleDateString('en-US', {
            weekday: 'short',
            month: 'short',
            day: 'numeric',
            year: 'numeric'
        });
    }

    // ===========================================
    // HALF-DAY HANDLING
    // ===========================================

    function setupHalfDayHandler() {
        if (!halfDayToggle) return;

        halfDayToggle.addEventListener('change', function () {
            if (this.checked) {
                if (fromDate && fromDate.value) {
                    toDate.value = fromDate.value;
                    toDate.min = fromDate.value;
                    toDate.max = fromDate.value;
                }
                showHalfDayHelper();
            } else {
                if (fromDate && fromDate.value) {
                    toDate.min = fromDate.value;
                    toDate.removeAttribute('max');
                }
                hideHalfDayHelper();
            }
            updateCalculations();
        });
    }

    function showHalfDayHelper() {
        let helper = document.getElementById('halfDayHelper');
        if (helper) {
            helper.style.display = 'block';
        }
    }

    function hideHalfDayHelper() {
        let helper = document.getElementById('halfDayHelper');
        if (helper) {
            helper.style.display = 'none';
        }
    }

    // ===========================================
    // RELIEVER SELECTION
    // ===========================================

    function setupRelieverSelector() {
        if (!relieverList) return;

        // Search functionality
        if (relieverSearch) {
            relieverSearch.addEventListener('input', function () {
                const query = this.value.toLowerCase().trim();
                const items = relieverList.querySelectorAll('.reliever-item');

                items.forEach(item => {
                    const name = (item.dataset.name || '').toLowerCase();
                    const employeeNo = (item.dataset.employeeNo || '').toLowerCase();
                    const searchText = name + ' ' + employeeNo;

                    item.style.display = searchText.includes(query) ? '' : 'none';
                });

                updateRelieverCount();
            });
        }

        // Click to select (single selection)
        relieverList.addEventListener('click', function (e) {
            const relieverItem = e.target.closest('.reliever-item');
            if (!relieverItem) return;

            const checkbox = relieverItem.querySelector('input[type="checkbox"]');
            if (checkbox && e.target !== checkbox) {
                // Clear all other selections
                clearAllRelieverSelections();
                // Select this one
                checkbox.checked = true;
                relieverItem.classList.add('selected');
            }
        });

        // Checkbox change handler
        relieverList.addEventListener('change', function (e) {
            if (e.target.type === 'checkbox' && e.target.name === 'SelectedRelieverEmployeeNos') {
                handleSingleRelieverSelection(e.target);
            }
        });
    }

    function handleSingleRelieverSelection(selectedCheckbox) {
        const allCheckboxes = relieverList.querySelectorAll('input[type="checkbox"]');
        allCheckboxes.forEach(checkbox => {
            if (checkbox !== selectedCheckbox) {
                checkbox.checked = false;
                const item = checkbox.closest('.reliever-item');
                if (item) item.classList.remove('selected');
            }
        });

        const relieverItem = selectedCheckbox.closest('.reliever-item');
        if (selectedCheckbox.checked) {
            relieverItem.classList.add('selected');
        } else {
            relieverItem.classList.remove('selected');
        }
    }

    function clearAllRelieverSelections() {
        if (!relieverList) return;
        const allCheckboxes = relieverList.querySelectorAll('input[type="checkbox"]');
        allCheckboxes.forEach(checkbox => {
            checkbox.checked = false;
            const item = checkbox.closest('.reliever-item');
            if (item) item.classList.remove('selected');
        });
    }

    function highlightSelectedReliever() {
        if (!relieverList) return;
        const selectedCheckbox = relieverList.querySelector('input[type="checkbox"]:checked');
        if (selectedCheckbox) {
            const relieverItem = selectedCheckbox.closest('.reliever-item');
            if (relieverItem) {
                relieverItem.classList.add('selected');
            }
        }
    }

    function updateRelieverCount() {
        const visibleItems = relieverList.querySelectorAll('.reliever-item:not([style*="display: none"])');
        const countElement = document.querySelector('#editLeaveModal .reliever-count');
        if (countElement) {
            countElement.textContent = `(${visibleItems.length} available)`;
        }
    }

    // ===========================================
    // FORM SUBMISSION
    // ===========================================

    function setupFormSubmission() {
        if (!form) {
            console.error('EditLeaveModal: Form not found for submission setup');
            return;
        }

        // Find the submit button (now type="button")
        const submitBtn = document.getElementById('editLeaveSubmitBtn');

        if (submitBtn) {

            console.log('EditLeaveModal: Setting up button click handler');

            submitBtn.addEventListener('click', async function (e) {
                e.preventDefault();
                e.stopPropagation();

                // Validate
                if (!validateForm()) {
                    console.log('EditLeaveModal: Validation failed');
                    return;
                }

                // Confirm submission - WAIT for user response
                const confirmed = await confirmSubmission();
                if (!confirmed) {
                    console.log('EditLeaveModal: User cancelled submission');
                    return;
                }

                // Only submit after confirmation
                console.log('EditLeaveModal: User confirmed, submitting...');
                await submitForm();
            });
        }

        // Also prevent default form submission (in case Enter is pressed)
        form.addEventListener('submit', function (e) {

            e.preventDefault();
            e.stopPropagation();
            console.log('EditLeaveModal: Form submit event intercepted');

            // Trigger the button click to go through proper flow
            if (submitBtn) {
                submitBtn.click();
            }
        });

        //form.addEventListener('submit', async function (e) {
        //    e.preventDefault();

        //    // Validate
        //    if (!validateForm()) {
        //        return;
        //    }

        //    // Confirm submission
        //    const confirmed = await confirmSubmission();
        //    if (!confirmed) return;

        //    // Submit
        //    await submitForm();
        //});
    }

    function validateForm() {
        let isValid = true;

        // Leave type
        if (!leaveTypeSelect || !leaveTypeSelect.value) {
            showFieldError(leaveTypeSelect, 'Please select a leave type');
            isValid = false;
        }

        // Dates
        if (!fromDate || !fromDate.value) {
            showFieldError(fromDate, 'Please select a start date');
            isValid = false;
        }

        if (!toDate || !toDate.value) {
            showFieldError(toDate, 'Please select an end date');
            isValid = false;
        }

        // Date range validation
        if (fromDate && toDate && fromDate.value && toDate.value) {
            const startDate = new Date(fromDate.value);
            const endDate = new Date(toDate.value);
            if (endDate < startDate) {
                showFieldError(toDate, 'End date must be after or same as start date');
                isValid = false;
            }
        }

        // Half-day validation
        if (halfDayToggle && halfDayToggle.checked) {
            if (fromDate.value !== toDate.value) {
                showFieldError(toDate, 'Half-day leave requires same start and end date');
                isValid = false;
            }
        }

        // Reliever
        const selectedReliever = relieverList?.querySelector('input[name="SelectedRelieverEmployeeNos"]:checked');
        if (!selectedReliever) {
            showToast('warning', 'Please select a reliever for your leave.');
            isValid = false;
        }

        return isValid;
    }

    async function confirmSubmission() {
        if (typeof Swal === 'undefined') return true;

        const leaveTypeName = leaveTypeSelect.options[leaveTypeSelect.selectedIndex].text.split('(')[0].trim();
        const selectedReliever = relieverList.querySelector('input[name="SelectedRelieverEmployeeNos"]:checked');
        const relieverName = selectedReliever ?
            selectedReliever.closest('.reliever-item').querySelector('.reliever-name').textContent : 'N/A';

        console.log('EditLeaveModal: Showing confirmation dialog...');

        const result = await Swal.fire({
            title: 'Confirm Update',
            html: `
                <div class="text-start">
                    <strong>Leave Type:</strong> ${leaveTypeName}<br>
                    <strong>Duration:</strong> ${daysApplied.value} days<br>
                    <strong>From:</strong> ${formatDisplayDate(fromDate.value)}<br>
                    <strong>To:</strong> ${formatDisplayDate(toDate.value)}<br>
                    <strong>Reliever:</strong> ${relieverName}
                </div>
            `,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Yes, Update',
            cancelButtonText: 'Review Again',
            confirmButtonColor: '#198754',
            cancelButtonColor: '#6c757d',
            allowOutsideClick: false,
            allowEscapeKey: true
        });

        console.log('EditLeaveModal: Confirmation result:', result.isConfirmed);
        return result.isConfirmed;
    }

    async function submitForm() {

        const submitBtn = document.getElementById('editLeaveSubmitBtn');

        //const submitBtn = form.querySelector('button[type="submit"]');

        const originalText = submitBtn ? submitBtn.innerHTML : '';

        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Updating...';
        }

        try {

            const formData = new FormData(form);

            // Ensure boolean values
            formData.set('HalfDay', halfDayToggle && halfDayToggle.checked ? 'true' : 'false');
            formData.set('LeaveAllowancePayable', leaveAllowanceToggle && leaveAllowanceToggle.checked ? 'true' : 'false');

            const response = await fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {

                // Check content type BEFORE parsing
                const contentType = response.headers.get('content-type');

                if (!contentType || !contentType.includes('application/json')) {

                    console.error('EditLeaveModal: Server returned non-JSON response:', contentType);
                    throw new Error('Session may have expired. Please refresh the page and try again.');
                }

                const data = await response.json();

                if (data.success) {

                    showToast('success', data.message || 'Leave application updated successfully!');

                    // Close modal
                    const modal = bootstrap.Modal.getInstance(modalElement);
                    if (modal) modal.hide();

                    // Refresh page or redirect
                    setTimeout(() => {

                        if (data.redirectUrl) {
                            window.location.href = data.redirectUrl;
                        } else {
                            window.location.reload();
                        }
                    }, 1500);

                } else {

                    showToast('error', data.message || 'Failed to update application');
                }
            } else {

                throw new Error(`Server error: ${response.status}`);
            }

        } catch (error) {

            console.error('EditLeaveModal: Submission error:', error);
            showToast('error', 'Error updating application. Please try again.');

        } finally {

            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }
        }
    }

    // ===========================================
    // UTILITY FUNCTIONS
    // ===========================================

    function showFieldError(field, message) {
        if (!field) return;
        field.classList.add('is-invalid');

        let feedback = field.parentNode.querySelector('.invalid-feedback');
        if (feedback) {
            feedback.textContent = message;
            feedback.style.display = 'block';
        }
    }

    function clearFieldError(field) {
        if (!field) return;
        field.classList.remove('is-invalid');

        const feedback = field.parentNode.querySelector('.invalid-feedback');
        if (feedback) {
            feedback.style.display = 'none';
        }
    }

    function showToast(type, message) {
        if (typeof Swal !== 'undefined') {
            const config = {
                success: { icon: 'success', title: 'Success', confirmButtonColor: '#065f46' },
                error: { icon: 'error', title: 'Error', confirmButtonColor: '#dc2626' },
                warning: { icon: 'warning', title: 'Warning', confirmButtonColor: '#d97706' },
                info: { icon: 'info', title: 'Information', confirmButtonColor: '#0369a1' }
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

    function formatDisplayDate(dateStr) {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleDateString('en-US', {
            weekday: 'short',
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }

    // ===========================================
    // PUBLIC API
    // ===========================================

    return {
        init: init,
        isInitialized: function () { return isInitialized; }
    };

})();


/**
 * Auto-initialize when modal content is loaded
 * This uses MutationObserver to detect when the modal is added to the DOM
 */
(function () {
    'use strict';

    // Watch for modal being added to dynamicModalContainer
    const container = document.getElementById('dynamicModalContainer');
    if (!container) {
        // Container doesn't exist yet, wait for DOMContentLoaded
        document.addEventListener('DOMContentLoaded', function () {
            setupModalObserver();
        });
    } else {
        setupModalObserver();
    }

    function setupModalObserver() {

        const container = document.getElementById('dynamicModalContainer');

        if (!container) return;

        const observer = new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                if (mutation.addedNodes.length > 0) {
                    // Check if editLeaveModal was added
                    const modal = document.getElementById('editLeaveModal');
                    if (modal && !EditLeaveModal.isInitialized()) {
                        // Small delay to ensure all content is rendered
                        setTimeout(function () {
                            EditLeaveModal.init();
                        }, 100);
                    }
                }
            });
        });

        observer.observe(container, { childList: true, subtree: true });
    }
})();


// Also expose globally for manual initialization if needed
window.EditLeaveModal = EditLeaveModal;