/* ========================================
   UN Portal - Leave Application JavaScript 
   ======================================== */

document.addEventListener('DOMContentLoaded', function () {
    initializeLeaveApplication();
});

function initializeLeaveApplication() {

    function validateDaysAppliedField(field) {
        if (!field || !field.value) return true;

        const value = parseFloat(field.value);

        // Basic number validation
        if (isNaN(value) || value <= 0) {
            showFieldError(field, 'Please enter a valid number of days');
            return false;
        }

        // Check decimal places (should be .5 or whole numbers)
        if (value % 0.5 !== 0) {
            showFieldError(field, 'Days must be in increments of 0.5 (half days)');
            return false;
        }

        // Get current form elements for validation context
        const leaveTypeSelect = document.querySelector('select[name="LeaveType"]');
        const halfDayToggle = document.querySelector('input[name="HalfDay"]');

        // Half-day specific validation
        const isHalfDay = halfDayToggle && halfDayToggle.checked;
        if (isHalfDay && value !== 0.5) {
            showFieldError(field, 'Half-day leave must be exactly 0.5 days');
            return false;
        }

        // Check against leave type maximum limits
        const maxAllowed = getMaxAllowedDays();
        if (maxAllowed && value > maxAllowed) {
            const leaveTypeName = getLeaveTypeName();
            showFieldError(field, `${leaveTypeName} allows maximum ${maxAllowed} days`);
            return false;
        }

        // Check against earned days (for annual leave only)
        if (isAnnualLeave()) {
            const earnedDaysInput = document.querySelector('input[name="LeaveEarnedToDate"]');
            const earnedDays = earnedDaysInput ? parseFloat(earnedDaysInput.value) || 0 : 0;

            if (value > earnedDays) {
                showFieldError(field, `Cannot exceed ${earnedDays} days (earned to date) for annual leave`);
                return false;
            }
        }

        // If we get here, validation passed
        clearFieldError(field);
        return true;
    }

    // Check if current leave type is annual leave
    function isAnnualLeave() {
        if (!leaveTypeSelect || !leaveTypeSelect.value) return false;
        const selectedOption = leaveTypeSelect.querySelector(`option[value="${leaveTypeSelect.value}"]`);
        return selectedOption ? selectedOption.getAttribute('data-is-annual') === 'true' : false;
    }


    // ===========================================
    // FORM VALIDATION
    // ===========================================

    function setupFormValidation() {
        const form = document.getElementById('leaveApplicationForm');
        if (!form) return;

        // Real-time validation feedback
        const requiredFields = form.querySelectorAll('input[required], select[required]');

        requiredFields.forEach(field => {
            field.addEventListener('blur', function () {
                validateInputField(this);
            });

            field.addEventListener('input', function () {
                // Clear error when user starts typing/selecting
                if (this.classList.contains('is-invalid')) {
                    clearFieldError(this);
                }
            });

            field.addEventListener('change', function () {
                // Clear error when user makes a selection
                if (this.classList.contains('is-invalid')) {
                    clearFieldError(this);
                }
            });
        });
    }

    function validateInputField(field) {
        if (!field) return true;

        // Clear previous error first
        clearFieldError(field);

        // Required field validation
        if (field.hasAttribute('required') && !field.value.trim()) {
            showFieldError(field, 'This field is required');
            return false;
        }

        // Email validation
        if (field.type === 'email' && field.value) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(field.value)) {
                showFieldError(field, 'Please enter a valid email address');
                return false;
            }
        }

        // Date validation
        if (field.type === 'date' && field.value) {
            const selectedDate = new Date(field.value);
            const today = new Date();
            today.setHours(0, 0, 0, 0);

            if (field.name === 'FromDate' || field.name === 'ToDate') {
                if (selectedDate < today) {
                    showFieldError(field, 'Please select a future date');
                    return false;
                }
            }
        }

        // Enhanced number validation for days applied
        if (field.name === 'DaysApplied' && field.value) {
            return validateDaysAppliedField(field);
        }

        return true;
    }

    // ===========================================
    // FORM SUBMISSION (Enhanced)
    // ===========================================

    function setupFormSubmission() {
        const form = document.getElementById('leaveApplicationForm');
        if (!form) return;

        form.addEventListener('submit', async function (e) {
            e.preventDefault();

            // Validate all fields
            let isValid = true;
            const requiredFields = form.querySelectorAll('input[required], select[required]');

            requiredFields.forEach(field => {
                if (!validateInputField(field)) {
                    isValid = false;
                }
            });

            // Custom validation for reliever selection
            const selectedReliever = form.querySelector('input[name="SelectedRelieverEmployeeNos"]:checked');
            if (!selectedReliever) {
                showAlert('Please select a reliever for your leave.', 'warning');
                isValid = false;
            }

            // Additional business validation
            const fromDate = form.querySelector('input[name="FromDate"]');
            const toDate = form.querySelector('input[name="ToDate"]');
            const daysApplied = form.querySelector('input[name="DaysApplied"]');
            const leaveType = form.querySelector('select[name="LeaveType"]');
            const halfDayToggle = form.querySelector('input[name="HalfDay"]');

            const isHalfDay = halfDayToggle && halfDayToggle.checked;

            if (isHalfDay) {
                // Ensure same date for half-day
                if (fromDate.value !== toDate.value) {
                    showFieldError(toDate, 'Half-day leave requires same start and end date');
                    isValid = false;
                }

                // Ensure exactly 0.5 days
                const daysValue = parseFloat(daysApplied.value);
                if (daysValue !== 0.5) {
                    showFieldError(daysApplied, 'Half-day leave must be exactly 0.5 days');
                    isValid = false;
                }
            }

            // Validate date range (for non-half-day or as general check)
            if (fromDate && toDate && fromDate.value && toDate.value) {
                const startDate = new Date(fromDate.value);
                const endDate = new Date(toDate.value);

                if (endDate < startDate) {
                    showFieldError(toDate, 'End date must be after or same as start date');
                    isValid = false;
                }
            }

            // Validate days applied
            if (daysApplied && !validateDaysAppliedField(daysApplied)) {
                isValid = false;
            }

            // Validate leave type selection
            if (leaveType && !leaveType.value) {
                showFieldError(leaveType, 'Please select a leave type');
                isValid = false;
            }

            // Leave allowance validation (only for annual leave)
            const leaveAllowanceToggle = form.querySelector('input[name="LeaveAllowancePayable"]');
            const isLeaveAllowanceRequested = leaveAllowanceToggle && leaveAllowanceToggle.checked;

            if (isLeaveAllowanceRequested) {
                if (!leaveType.value || leaveType.value.toUpperCase() !== 'ANNUAL') {
                    showFieldError(leaveType, 'Leave allowance is only available for annual leave');
                    isValid = false;
                }
            }

            // Additional business validation: check for weekend-only applications
            if (fromDate.value && toDate.value && !isHalfDay) {
                const startDate = new Date(fromDate.value);
                const endDate = new Date(toDate.value);

                // Check if the application spans only weekends (might be unintentional)
                let hasWeekdays = false;
                const current = new Date(startDate);

                while (current <= endDate) {
                    const dayOfWeek = current.getDay();
                    if (dayOfWeek !== 0 && dayOfWeek !== 6) { // Not Sunday(0) or Saturday(6)
                        hasWeekdays = true;
                        break;
                    }
                    current.setDate(current.getDate() + 1);
                }

                // For business day leave types, warn if no weekdays selected
                const leaveTypeSelect = form.querySelector('select[name="LeaveType"]');
                if (leaveTypeSelect && leaveTypeSelect.value) {
                    const selectedOption = leaveTypeSelect.querySelector(`option[value="${leaveTypeSelect.value}"]`);
                    const usesCalendarDays = selectedOption ? selectedOption.getAttribute('data-uses-calendar') === 'true' : false;

                    if (!usesCalendarDays && !hasWeekdays) {
                        const result = await Swal.fire({
                            title: 'Weekend Only Application',
                            text: 'Your selected dates include only weekends. For this leave type, only business days are counted. Do you want to continue?',
                            icon: 'warning',
                            showCancelButton: true,
                            confirmButtonText: 'Yes, Continue',
                            cancelButtonText: 'Let me change dates',
                            confirmButtonColor: '#ffc107',
                            cancelButtonColor: '#6c757d'
                        });

                        if (!result.isConfirmed) {
                            return; // Stop submission
                        }
                    }
                }
            }

            if (!isValid) {
                // Focus on first invalid field
                const firstInvalid = form.querySelector('.is-invalid');
                if (firstInvalid) {
                    firstInvalid.focus();
                    firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
                }
                return;
            }

            // Show confirmation dialog with leave details
            const leaveTypeName = leaveType.options[leaveType.selectedIndex].text.split('(')[0].trim();
            const relieverName = selectedReliever.closest('.reliever-item').querySelector('.reliever-name').textContent;

            // Format dates nicely for confirmation
            const formatDate = (dateStr) => new Date(dateStr).toLocaleDateString('en-US', {
                weekday: 'short',
                year: 'numeric',
                month: 'short',
                day: 'numeric'
            });

            let confirmationHTML = `
            <div class="text-start">
                <strong>Leave Type:</strong> ${leaveTypeName}<br>
                <strong>Duration:</strong> ${daysApplied.value} days${isHalfDay ? ' (Half Day)' : ''}<br>
                <strong>From:</strong> ${formatDate(fromDate.value)}<br>
                <strong>To:</strong> ${formatDate(toDate.value)}<br>
                <strong>Reliever:</strong> ${relieverName}
        `;

            if (isLeaveAllowanceRequested) {
                confirmationHTML += `<br><strong>Leave Allowance:</strong> Yes`;
            }

            confirmationHTML += `</div>`;

            const result = await Swal.fire({
                title: 'Confirm Leave Application',
                html: confirmationHTML,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, Submit Application',
                cancelButtonText: 'Review Again',
                confirmButtonColor: '#198754',
                cancelButtonColor: '#6c757d'
            });

            if (!result.isConfirmed) return;

            // Continue with form submission
            await submitForm(form);
        });
    }


    // ===========================================
    // RELIEVER SELECTOR (Single Selection Only)
    // ===========================================

    function setupRelieverSelector() {
        const searchInput = document.getElementById('relieverSearch');
        const relieverList = document.getElementById('relieverList');

        if (!searchInput || !relieverList) return;

        // Enhanced search filter - only name and employee number
        searchInput.addEventListener('input', function () {
            const query = this.value.toLowerCase().trim();
            const items = relieverList.querySelectorAll('.reliever-item');

            items.forEach(item => {
                const name = (item.dataset.name || '').toLowerCase();
                const employeeNo = (item.dataset.employeeNo || '').toLowerCase();

                // Only search by name and employee number
                const searchText = name + ' ' + employeeNo;

                item.style.display = searchText.includes(query) ? '' : 'none';
            });

            // Update available count
            updateAvailableCount();
        });

        // Handle single selection (radio-like behavior)
        relieverList.addEventListener('change', function (e) {
            if (e.target.type === 'checkbox' && e.target.name === 'SelectedRelieverEmployeeNos') {
                handleSingleRelieverSelection(e.target);
            }
        });

        // Click on item to select (single selection)
        relieverList.addEventListener('click', function (e) {
            const relieverItem = e.target.closest('.reliever-item');
            if (!relieverItem) return;

            const checkbox = relieverItem.querySelector('input[type="checkbox"]');
            if (checkbox && e.target !== checkbox) {
                // Clear all other selections first
                clearAllRelieverSelections();
                // Select this one
                checkbox.checked = true;
                handleSingleRelieverSelection(checkbox);
            }
        });
    }

    function handleSingleRelieverSelection(selectedCheckbox) {
        // Clear all other selections first
        const allCheckboxes = document.querySelectorAll('input[name="SelectedRelieverEmployeeNos"]');
        allCheckboxes.forEach(checkbox => {
            if (checkbox !== selectedCheckbox) {
                checkbox.checked = false;
                checkbox.closest('.reliever-item').classList.remove('selected');
            }
        });

        // Handle the selected one
        const relieverItem = selectedCheckbox.closest('.reliever-item');
        if (selectedCheckbox.checked) {
            relieverItem.classList.add('selected');
            showSelectedRelieverInfo(relieverItem);
        } else {
            relieverItem.classList.remove('selected');
            clearSelectedRelieverInfo();
        }
    }

    function clearAllRelieverSelections() {
        const allCheckboxes = document.querySelectorAll('input[name="SelectedRelieverEmployeeNos"]');
        allCheckboxes.forEach(checkbox => {
            checkbox.checked = false;
            checkbox.closest('.reliever-item').classList.remove('selected');
        });
        clearSelectedRelieverInfo();
    }

    function showSelectedRelieverInfo(relieverItem) {
        const name = relieverItem.querySelector('.reliever-name').textContent;
        const employeeNo = relieverItem.querySelector('.reliever-employee-no').textContent;
        const department = relieverItem.querySelector('.reliever-department').textContent;
        const jobTitle = relieverItem.querySelector('.reliever-job-title').textContent;

        // Create or update selected reliever display
        let selectedDisplay = document.getElementById('selectedRelieverDisplay');
        if (!selectedDisplay) {
            selectedDisplay = document.createElement('div');
            selectedDisplay.id = 'selectedRelieverDisplay';
            selectedDisplay.className = 'selected-reliever-display mt-3 p-3 bg-light border rounded';

            // Insert after the reliever list
            const relieverContainer = document.querySelector('.reliever-card-body');
            if (relieverContainer) {
                relieverContainer.appendChild(selectedDisplay);
            }
        }

        selectedDisplay.innerHTML = `
        <div class="d-flex justify-content-between align-items-start">
            <div>
                <h6 class="mb-1 text-primary">Selected Reliever:</h6>
                <strong>${name}</strong> (${employeeNo})
                <br>
                <small class="text-muted">${department} • ${jobTitle}</small>
            </div>
            <button type="button" class="btn btn-sm btn-outline-danger" onclick="clearAllRelieverSelections()">
                <i>❌</i> Remove
            </button>
        </div>
    `;
    }

    function clearSelectedRelieverInfo() {
        const selectedDisplay = document.getElementById('selectedRelieverDisplay');
        if (selectedDisplay) {
            selectedDisplay.remove();
        }
    }

    function updateAvailableCount() {
        const visibleItems = document.querySelectorAll('.reliever-item:not([style*="display: none"])');
        const countElement = document.querySelector('.reliever-count');
        if (countElement) {
            countElement.textContent = `(${visibleItems.length} available)`;
        }
    }



    setupDateCalculations();
    setupRelieverSelector();
    setupAttachmentHandler();
    setupFormValidation();
    setupFormSubmission();
}

// ===========================================
// DATE CALCULATIONS 
// ===========================================
function setupDateCalculations() {

    const fromDate = document.querySelector('input[name="FromDate"]');
    const toDate = document.querySelector('input[name="ToDate"]');
    const daysApplied = document.querySelector('input[name="DaysApplied"]');
    const resumptionDate = document.querySelector('input[name="ResumptionDate"]');
    const leaveTypeSelect = document.querySelector('select[name="LeaveType"]');
    const halfDayToggle = document.querySelector('input[name="HalfDay"]'); // New half-day 

    if (!fromDate || !toDate || !daysApplied) return;

    // Half-day toggle event listener
    if (halfDayToggle) {
        halfDayToggle.addEventListener('change', function () {
            handleHalfDayToggle();
            updateCalculations();
        });
    }

    function handleHalfDayToggle() {

        const isHalfDay = halfDayToggle && halfDayToggle.checked;

        if (isHalfDay) {
            // When half-day is selected, enforce single day selection
            if (fromDate.value) {
                toDate.value = fromDate.value; // Force same date
                toDate.min = fromDate.value;
                toDate.max = fromDate.value; // Restrict to same date

            }

            // Show helper text
            showHalfDayHelper();

        } else {
            // When half-day is deselected, remove date restrictions
            if (fromDate.value) {
                toDate.min = fromDate.value; // Restore normal minimum
                toDate.removeAttribute('max'); // Remove maximum restriction
            }

            // Hide helper text
            hideHalfDayHelper();
        }
    }

    function showHalfDayHelper() {
        let helper = document.getElementById('halfDayHelper');
        if (!helper) {
            helper = document.createElement('div');
            helper.id = 'halfDayHelper';
            helper.className = 'alert alert-info mt-2';
            helper.innerHTML = `
                <i class="fas fa-info-circle"></i>
                <strong>Half Day Leave:</strong> Please select the same date for both start and end date. 
                This will automatically calculate as 0.5 days.
            `;

            // Insert after the half-day toggle
            const toggleContainer = halfDayToggle.closest('.form-group') || halfDayToggle.parentNode;
            toggleContainer.appendChild(helper);
        }
        helper.style.display = 'block';

    }

    function hideHalfDayHelper() {
        const helper = document.getElementById('halfDayHelper');
        if (helper) {
            helper.style.display = 'none';
        }
    }

    function isHalfDaySelected() {
        return halfDayToggle && halfDayToggle.checked;

    }

    function isSingleDayRange(startDate, endDate) {
        return startDate.getTime() === endDate.getTime();

    }

    function updateLeaveBalanceDisplay(selectedLeaveType) {

        const leaveBalanceInfo = document.getElementById('leaveBalanceInfo');
        if (!leaveBalanceInfo) return;

        const alertElement = document.querySelector('.alert.alert-info');
        if (!alertElement) return;

        if (!selectedLeaveType) {
            leaveBalanceInfo.innerHTML = 'Select a leave type to view your available balance';
            return;
        }

        // Map leave types to their data attributes and display names
        const leaveTypeMap = {
            'ANNUAL': {
                name: 'Annual Leave',
                balanceKey: 'annualBalance',
                earnedKey: 'annualEarned'
            },
            'ADOPTION': {
                name: 'Adoption Leave',
                balanceKey: 'adoptionBalance'
            },
            'COMPASSIONATE': {
                name: 'Compassion Leave',
                balanceKey: 'compassionBalance'
            },
            'MATERNITY': {
                name: 'Maternity Leave',
                balanceKey: 'maternityBalance'
            },
            'PATERNITY': {
                name: 'Paternity Leave',
                balanceKey: 'paternityBalance'
            },
            'SICK': {
                name: 'Sick Leave',
                balanceKey: 'sickBalance'
            },
            'STUDY': {
                name: 'Study Leave',
                balanceKey: 'studyBalance'
            },
            'UNPAID': {
                name: 'Unpaid Leave',
                balanceKey: 'unpaidBalance'
            }
        };

        const leaveInfo = leaveTypeMap[selectedLeaveType];

        if (!leaveInfo) {
            leaveBalanceInfo.innerHTML = 'Leave type information not available';
            return;
        }

        // Get balance from data attributes
        const balance = alertElement.dataset[leaveInfo.balanceKey] || '0';

        if (selectedLeaveType === 'ANNUAL') {
            const earned = alertElement.dataset[leaveInfo.earnedKey] || '0';
            leaveBalanceInfo.innerHTML = `
            <strong>${leaveInfo.name}:</strong> ${earned} days earned, 
            <strong>${balance} days available</strong>
        `;
        } else {
            leaveBalanceInfo.innerHTML = `
            <strong>${leaveInfo.name}:</strong> 
            <strong>${balance} days available</strong>
        `;
        }
    }

    // Calculate business days (excluding weekends)
    function calculateBusinessDays(start, end) {

        if (!start || !end) return 0;

        const startDate = new Date(start);
        const endDate = new Date(end);
        let count = 0;

        const current = new Date(startDate);

        while (current <= endDate) {
            const dayOfWeek = current.getDay();
            if (dayOfWeek !== 0 && dayOfWeek !== 6) { // Not Sunday(0) or Saturday(6)
                count++;
            }

            current.setDate(current.getDate() + 1);
        }

        return count;
    }

    // Calculate calendar days (including weekends and holidays)
    function calculateCalendarDays(start, end) {

        if (!start || !end) return 0;

        const startDate = new Date(start);
        const endDate = new Date(end);
        const diffTime = Math.abs(endDate - startDate);
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1; // +1 to include both start and end dates

        return diffDays;
    }

    // Calculate next business day for resumption
    function getNextBusinessDay(date) {

        const nextDay = new Date(date);
        nextDay.setDate(nextDay.getDate() + 1);

        while (nextDay.getDay() === 0 || nextDay.getDay() === 6) {
            nextDay.setDate(nextDay.getDate() + 1);
        }

        return nextDay;
    }

    // Calculate next calendar day for resumption
    function getNextCalendarDay(date) {
        const nextDay = new Date(date);
        nextDay.setDate(nextDay.getDate() + 1);
        return nextDay;
    }

    // Format date for display
    function formatDisplayDate(date) {
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

    // Check if current leave type uses calendar days
    function usesCalendarDays() {
        if (!leaveTypeSelect || !leaveTypeSelect.value) return false;
        const selectedOption = leaveTypeSelect.querySelector(`option[value="${leaveTypeSelect.value}"]`);
        return selectedOption ? selectedOption.getAttribute('data-uses-calendar') === 'true' : false;
    }

    // Get maximum allowed days for selected leave type
    function getMaxAllowedDays() {
        if (!leaveTypeSelect || !leaveTypeSelect.value) return null;

        const selectedOption = leaveTypeSelect.querySelector(`option[value="${leaveTypeSelect.value}"]`);
        const maxDays = selectedOption ? selectedOption.getAttribute('data-max-days') : null;

        return maxDays ? parseInt(maxDays) : null;
    }

    

    // Get leave type name for display
    function getLeaveTypeName() {
        if (!leaveTypeSelect || !leaveTypeSelect.value) return 'Selected leave type';
        const selectedOption = leaveTypeSelect.options[leaveTypeSelect.selectedIndex];
        return selectedOption.text.split('(')[0].trim();
    }

    // Auto-calculate when dates change
    // ToDo: We need to handle half day toggle checked.
    function updateCalculations() {

        if (fromDate.value && toDate.value) {
            const startDate = new Date(fromDate.value);
            const endDate = new Date(toDate.value);

            // Validate date range first
            if (endDate < startDate) {
                daysApplied.value = '';
                if (resumptionDate) resumptionDate.value = '';
                showFieldError(toDate, 'End date must be after or same as start date');
                return;
            }

            // Half-day validation
            if (isHalfDaySelected()) {
                if (!isSingleDayRange(startDate, endDate)) {
                    showFieldError(toDate, 'Half-day leave must be for the same date (start and end date must be identical)');
                    return;
                }
            }

            // Enable the days applied field for editing
            enableDaysAppliedField();

            // Calculate days based on half-day toggle first, then leave type
            let calculatedDays;

            if (isHalfDaySelected()) {

                // Half-day leave is always 0.5 regardless of leave type
                calculatedDays = 0.5;
            } else {

                // Regular calculation based on leave type
                if (usesCalendarDays()) {
                    calculatedDays = calculateCalendarDays(fromDate.value, toDate.value);
                } else {
                    calculatedDays = calculateBusinessDays(fromDate.value, toDate.value);
                }
            }

            daysApplied.value = calculatedDays;

            // Validate the calculated days
            if (!validateCalculatedDays(calculatedDays)) {
                return; // Don't proceed if validation fails
            }

            // Calculate resumption date
            if (resumptionDate) {

                let resumption;

                if (isHalfDaySelected()) {

                    // For half-day, resumption is the next business/calendar day based on leave type
                    if (usesCalendarDays()) {
                        resumption = getNextCalendarDay(endDate);
                    } else {
                        resumption = getNextBusinessDay(endDate);
                    }
                } else {

                    // Regular resumption calculation
                    if (usesCalendarDays()) {
                        resumption = getNextCalendarDay(endDate);
                    } else {
                        resumption = getNextBusinessDay(endDate);
                    }
                }

                resumptionDate.removeAttribute('readonly');
                resumptionDate.value = formatShortDisplayDate(resumption);
                resumptionDate.setAttribute('readonly', 'readonly');
            }

            // Clear validation errors when calculations are valid
            clearFieldError(fromDate);
            clearFieldError(toDate);
            clearFieldError(daysApplied);

            // Update the max attribute for days applied based on leave type
            updateDaysAppliedConstraints();

            disableDaysAppliedField();
            
        }

    }

    function enableDaysAppliedField() {
        daysApplied.removeAttribute('readonly');
        daysApplied.classList.remove('calculated-field');
    }
    
    function disableDaysAppliedField() {

        daysApplied.setAttribute('readonly', 'readonly');
        daysApplied.classList.add('calculated-field');
    }

    // Validate calculated days against constraints
    function validateCalculatedDays(calculatedDays) {

        // Check against leave type limits
        const maxAllowed = getMaxAllowedDays();

        // For half-day, we don't need to check maximums since 0.5 is always valid
        if (!isHalfDaySelected() && maxAllowed && calculatedDays > maxAllowed) {
            const leaveTypeName = getLeaveTypeName();
            showFieldError(daysApplied, `${leaveTypeName} allows maximum ${maxAllowed} days`);
            return false;
        }

        // Check against earned days (for annual leave only)
        if (isAnnualLeave()) {
            const earnedDaysInput = document.querySelector('input[name="LeaveEarnedToDate"]');
            const earnedDays = earnedDaysInput ? parseFloat(earnedDaysInput.value) || 0 : 0;

            if (calculatedDays > earnedDays) {
                showFieldError(daysApplied, `Cannot exceed ${earnedDays} days (earned to date) for annual leave`);
                return false;
            }
        }

        return true;
    }

    // Update days applied field constraints based on leave type
    function updateDaysAppliedConstraints() {
        const maxAllowed = getMaxAllowedDays();

        if (maxAllowed) {
            daysApplied.setAttribute('max', maxAllowed);
            daysApplied.setAttribute('title', `Maximum ${maxAllowed} days allowed for this leave type`);

        } else if (isAnnualLeave()) {
            // For annual leave, use earned days as max
            const earnedDaysInput = document.querySelector('input[name="LeaveEarnedToDate"]');
            const earnedDays = earnedDaysInput ? parseFloat(earnedDaysInput.value) || 0 : 30;
            daysApplied.setAttribute('max', earnedDays);
            daysApplied.setAttribute('title', `Cannot exceed ${earnedDays} days (earned to date)`);

        } else {
            // Remove max limit for unlimited leave types
            daysApplied.removeAttribute('max');
            daysApplied.setAttribute('title', 'Enter number of days');
        }

        // Update placeholder based on calculation method
        if (usesCalendarDays()) {
            daysApplied.setAttribute('placeholder', 'Calculated as calendar days (includes weekends)');
        } else {
            daysApplied.setAttribute('placeholder', 'Calculated as business days (excludes weekends)');
        }
    }

    fromDate.addEventListener('change', function () {
        if (this.value) {
            if (isHalfDaySelected()) {
                // For half-day, automatically set toDate to same as fromDate
                toDate.value = this.value;
                toDate.min = this.value;
                toDate.max = this.value;
            } else {
                // Regular behavior
                toDate.min = this.value;
                toDate.removeAttribute('max');
                if (toDate.value && toDate.value < this.value) {
                    toDate.value = '';
                    daysApplied.value = '';
                    if (resumptionDate) resumptionDate.value = '';
                }
            }
            clearFieldError(this);
        }
        updateCalculations();
    });

    toDate.addEventListener('change', function () {
        // Validate half-day constraint
        if (isHalfDaySelected() && fromDate.value) {
            if (this.value !== fromDate.value) {
                showFieldError(this, 'For half-day leave, end date must be same as start date');
                return;
            }
        }
        clearFieldError(this);
        updateCalculations();
    });

    // Leave type change affects constraints and calculation method
    if (leaveTypeSelect) {
        leaveTypeSelect.addEventListener('change', function () {
            updateDaysAppliedConstraints();
            updateLeaveBalanceDisplay(this.value); // Call the balance update function

            // Recalculate if dates are already selected
            if (fromDate.value && toDate.value) {
                updateCalculations();
            }

            // Revalidate current days if any
            if (daysApplied.value) {
                validateDaysAppliedField(daysApplied);
            }
        });
    }

    // Manual days input change with validation
    daysApplied.addEventListener('input', function () {
        clearFieldError(this);
        validateDaysAppliedField(this);
    });

    // Initialize constraints on page load
    updateDaysAppliedConstraints();
    updateLeaveBalanceDisplay('ANNUAL');


}



// ===========================================
// ATTACHMENT HANDLER
// ===========================================

function setupAttachmentHandler() {
    const fileInput = document.querySelector('input[name="Attachments"]');
    if (!fileInput) return;

    fileInput.addEventListener('change', function (e) {
        handleFileSelection(e.target.files);
    });

    // Enhanced drag and drop
    const uploadArea = document.querySelector('.file-upload-area');
    if (uploadArea) {
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            uploadArea.addEventListener(eventName, preventDefaults, false);
        });

        ['dragenter', 'dragover'].forEach(eventName => {
            uploadArea.addEventListener(eventName, highlight, false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            uploadArea.addEventListener(eventName, unhighlight, false);
        });

        uploadArea.addEventListener('drop', handleDrop, false);
    }

    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }

    function highlight(e) {
        uploadArea.classList.add('drag-over');
    }

    function unhighlight(e) {
        uploadArea.classList.remove('drag-over');
    }

    function handleDrop(e) {
        const dt = e.dataTransfer;
        const files = dt.files;
        fileInput.files = files;
        handleFileSelection(files);
    }
}

function handleFileSelection(files) {
    const uploadArea = document.querySelector('.file-upload-area');
    if (!uploadArea || !files.length) return;

    // Validate files
    const validFiles = [];
    const maxSize = 10 * 1024 * 1024; // 10MB
    const allowedTypes = [
        'application/pdf',
        'application/msword',
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
        'application/vnd.ms-excel',
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        'application/vnd.ms-powerpoint',
        'application/vnd.openxmlformats-officedocument.presentationml.presentation',
        'text/plain',
        'image/jpeg',
        'image/jpg',
        'image/png',
        'image/gif',
        'application/zip',
        'application/x-rar-compressed'
    ];

    for (let file of files) {
        if (file.size > maxSize) {
            showAlert(`File "${file.name}" is too large. Maximum size is 10MB.`, 'warning');
            continue;
        }

        if (!allowedTypes.includes(file.type) && !isValidFileExtension(file.name)) {
            showAlert(`File "${file.name}" is not a supported file type.`, 'warning');
            continue;
        }

        validFiles.push(file);
    }

    if (validFiles.length === 0) return;

    // Update display
    updateFileDisplay(validFiles);
}

function isValidFileExtension(filename) {
    const validExtensions = ['.pdf', '.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx',
        '.txt', '.jpg', '.jpeg', '.png', '.gif', '.zip', '.rar'];
    const extension = filename.toLowerCase().substring(filename.lastIndexOf('.'));
    return validExtensions.includes(extension);
}

function updateFileDisplay(files) {
    const uploadArea = document.querySelector('.file-upload-area');
    if (!uploadArea) return;

    // Create file list display
    let fileListDisplay = uploadArea.querySelector('.file-list-display');
    if (!fileListDisplay) {
        fileListDisplay = document.createElement('div');
        fileListDisplay.className = 'file-list-display mt-3';
        uploadArea.appendChild(fileListDisplay);
    }

    // Update content
    if (files.length === 0) {
        fileListDisplay.innerHTML = '';
        return;
    }

    const fileListHTML = Array.from(files).map((file, index) => {
        const sizeText = formatFileSize(file.size);
        return `
            <div class="file-item d-flex justify-content-between align-items-center p-2 mb-2 bg-white border rounded">
                <div class="file-info">
                    <div class="file-name fw-bold text-truncate" style="max-width: 200px;" title="${file.name}">
                        📎 ${file.name}
                    </div>
                    <small class="text-muted">${sizeText}</small>
                </div>
                <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeFile(${index})">
                    <i>🗑️</i>
                </button>
            </div>
        `;
    }).join('');

    fileListDisplay.innerHTML = `
        <div class="selected-files-header">
            <strong>Selected Files (${files.length}):</strong>
        </div>
        ${fileListHTML}
    `;
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

function removeFile(index) {
    const fileInput = document.querySelector('input[name="Attachments"]');
    if (!fileInput) return;

    const dt = new DataTransfer();
    const files = fileInput.files;

    for (let i = 0; i < files.length; i++) {
        if (i !== index) {
            dt.items.add(files[i]);
        }
    }

    fileInput.files = dt.files;
    handleFileSelection(fileInput.files);
}




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



// Helper function for form submission
async function submitForm(form) {
    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Submitting...';

    try {
        const formData = new FormData(form);

        // Ensure boolean values are properly set
        const applyOnBehalf = form.querySelector('input[name="ApplyOnBehalf"]:checked');
        const halfDay = form.querySelector('input[name="HalfDay"]:checked');
        const leaveAllowance = form.querySelector('input[name="LeaveAllowancePayable"]:checked');

        formData.set('ApplyOnBehalf', applyOnBehalf ? 'true' : 'false');
        formData.set('HalfDay', halfDay ? 'true' : 'false');
        formData.set('LeaveAllowancePayable', leaveAllowance ? 'true' : 'false');

        const response = await fetch(form.action || window.location.pathname, {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            const data = await response.json();

            if (data.success) {
                await Swal.fire({
                    title: 'Success!',
                    text: `Leave application submitted successfully. ${data.applicationNo ? 'Reference: ' + data.applicationNo : ''}`,
                    icon: 'success',
                    confirmButtonText: 'Go to Dashboard',
                    confirmButtonColor: '#198754'
                });

                window.location.href = data.redirectUrl || '/Home/Index';

            } else {
                let errorMessage = data.message || 'Failed to submit application';

                if (data.errors && data.errors.length > 0) {
                    errorMessage += '\n\nDetails:\n' + data.errors.join('\n');
                }

                Swal.fire({
                    title: 'Error!',
                    text: errorMessage,
                    icon: 'error',
                    confirmButtonColor: '#dc3545'
                });
            }
        } else {

            throw new Error(`Server error: ${response.status}`);
        }

    } catch (error) {
        console.error('Submission error:', error);
        Swal.fire({
            title: 'Error!',
            text: error,
            icon: 'error',
            confirmButtonColor: '#dc3545'
        });
    } finally {
        // Restore button
        submitBtn.disabled = false;
        submitBtn.innerHTML = originalText;
    }
}

// ===========================================
// UTILITIES
// ===========================================

// Show alert function (fallback if SweetAlert2 not available)
function showAlert(message, type = 'info') {
    if (typeof Swal !== 'undefined') {
        const config = {
            title: getAlertTitle(type),
            text: message,
            icon: type,
            confirmButtonText: 'OK',
            confirmButtonColor: '#0d6efd'
        };
        Swal.fire(config);
    } else {
        alert(message);
    }
}

function getAlertTitle(type) {
    switch (type) {
        case 'success': return 'Success!';
        case 'error': return 'Error!';
        case 'warning': return 'Warning!';
        case 'info': default: return 'Information';
    }
}

// Global functions for buttons
window.clearAllRelieverSelections = clearAllRelieverSelections;
window.removeFile = removeFile;