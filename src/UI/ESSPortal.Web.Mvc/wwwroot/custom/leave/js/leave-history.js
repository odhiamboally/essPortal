/* ========================================
   UN Portal - Leave History JavaScript
   ======================================== */

class LeaveHistoryManager {
    constructor() {
        this.currentPage = 1;
        this.itemsPerPage = 10;
        this.allRows = [];
        this.filteredRows = [];

        this.init();
    }

    init() {
        console.log('Leave History Manager: Initializing...');

        // Get all table rows
        this.allRows = Array.from(document.querySelectorAll('.leave-row'));
        this.filteredRows = [...this.allRows];

        // Setup event listeners
        this.setupEventListeners();

        // Initial display and pagination setup
        this.updateDisplay();
        this.updatePagination();

        console.log(`Leave History Manager: Loaded ${this.allRows.length} applications`);
    }

    setupEventListeners() {
        // Search input
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.addEventListener('input', () => this.debounce(() => this.applyFilters(), 300));
        }

        // Filter inputs
        const applicationNoFilter = document.getElementById('applicationNoFilter');
        if (applicationNoFilter) {
            applicationNoFilter.addEventListener('input', () => this.debounce(() => this.applyFilters(), 300));
        }

        const dateFromFilter = document.getElementById('dateFromFilter');
        if (dateFromFilter) {
            dateFromFilter.addEventListener('change', () => this.applyFilters());
        }

        const dateToFilter = document.getElementById('dateToFilter');
        if (dateToFilter) {
            dateToFilter.addEventListener('change', () => this.applyFilters());
        }

        const statusFilter = document.getElementById('statusFilter');
        if (statusFilter) {
            statusFilter.addEventListener('change', () => this.applyFilters());
        }
    }

    applyFilters() {
        console.log('Applying filters...');

        const searchTerm = document.getElementById('searchInput').value.toLowerCase().trim();
        const applicationNo = document.getElementById('applicationNoFilter').value.toLowerCase().trim();
        const dateFrom = document.getElementById('dateFromFilter').value;
        const dateTo = document.getElementById('dateToFilter').value;
        const status = document.getElementById('statusFilter').value.toLowerCase();

        this.filteredRows = this.allRows.filter(row => {
            // Search across multiple fields
            if (searchTerm) {
                const searchableText = [
                    row.dataset.applicationNo,
                    row.dataset.leaveType,
                    row.dataset.status,
                    row.querySelector('.application-date').textContent,
                    row.querySelector('.start-date').textContent,
                    row.querySelector('.end-date').textContent
                ].join(' ').toLowerCase();

                if (!searchableText.includes(searchTerm)) {
                    return false;
                }
            }

            // Application number filter
            if (applicationNo && !row.dataset.applicationNo.toLowerCase().includes(applicationNo)) {
                return false;
            }

            // Date range filter
            if (dateFrom) {
                const applicationDate = new Date(row.dataset.applicationDate);
                const fromDate = new Date(dateFrom);
                if (applicationDate < fromDate) {
                    return false;
                }
            }

            if (dateTo) {
                const applicationDate = new Date(row.dataset.applicationDate);
                const toDate = new Date(dateTo);
                if (applicationDate > toDate) {
                    return false;
                }
            }

            // Status filter
            if (status && row.dataset.status !== status) {
                return false;
            }

            return true;
        });

        // Reset to first page
        this.currentPage = 1;

        // Update display
        this.updateDisplay();
        this.updatePagination();

        console.log(`Filtered to ${this.filteredRows.length} applications`);
    }

    clearFilters() {
        console.log('Clearing all filters...');

        // Clear all input values
        document.getElementById('searchInput').value = '';
        document.getElementById('applicationNoFilter').value = '';
        document.getElementById('dateFromFilter').value = '';
        document.getElementById('dateToFilter').value = '';
        document.getElementById('statusFilter').value = '';

        // Reset filtered rows
        this.filteredRows = [...this.allRows];
        this.currentPage = 1;

        // Update display
        this.updateDisplay();
        this.updatePagination();
    }

    updateDisplay() {
        const startIndex = (this.currentPage - 1) * this.itemsPerPage;
        const endIndex = startIndex + this.itemsPerPage;
        const currentPageRows = this.filteredRows.slice(startIndex, endIndex);

        // Hide all rows first
        this.allRows.forEach(row => {
            row.classList.add('hidden');
        });

        // Show current page rows
        currentPageRows.forEach(row => {
            row.classList.remove('hidden');
        });

        // Update results count
        const resultsCount = document.getElementById('resultsCount');
        if (resultsCount) {
            resultsCount.textContent = `(${this.filteredRows.length} applications)`;
        }

        // Show/hide no results message
        const noResults = document.getElementById('noResults');
        const tableContainer = document.querySelector('.table-container table');

        if (this.filteredRows.length === 0) {
            if (noResults) noResults.style.display = 'block';
            if (tableContainer) tableContainer.style.display = 'none';
        } else {
            if (noResults) noResults.style.display = 'none';
            if (tableContainer) tableContainer.style.display = 'table';
        }
    }

    updatePagination() {
        const totalPages = Math.ceil(this.filteredRows.length / this.itemsPerPage);
        const startItem = this.filteredRows.length === 0 ? 0 : (this.currentPage - 1) * this.itemsPerPage + 1;
        const endItem = Math.min(this.currentPage * this.itemsPerPage, this.filteredRows.length);

        // Update pagination info
        const paginationInfo = document.getElementById('paginationInfo');
        if (paginationInfo) {
            if (this.filteredRows.length === 0) {
                paginationInfo.textContent = 'No applications found';
            } else {
                paginationInfo.textContent = `Showing ${startItem}-${endItem} of ${this.filteredRows.length} applications`;
            }
        }

        // Update pagination controls
        const prevBtn = document.getElementById('prevBtn');
        const nextBtn = document.getElementById('nextBtn');
        const pageNumbers = document.getElementById('pageNumbers');

        if (prevBtn) {
            prevBtn.disabled = this.currentPage <= 1;
        }

        if (nextBtn) {
            nextBtn.disabled = this.currentPage >= totalPages;
        }

        // Generate page numbers
        if (pageNumbers) {
            pageNumbers.innerHTML = this.generatePageNumbers(totalPages);
        }

        // Hide pagination if only one page or no results
        const paginationContainer = document.getElementById('paginationContainer');
        if (paginationContainer) {
            paginationContainer.style.display = (totalPages <= 1) ? 'none' : 'flex';
        }
    }

    generatePageNumbers(totalPages) {
        if (totalPages <= 1) return '';

        let html = '';
        const maxVisiblePages = 5;
        let startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
        let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

        // Adjust start page if we're near the end
        if (endPage - startPage < maxVisiblePages - 1) {
            startPage = Math.max(1, endPage - maxVisiblePages + 1);
        }

        // First page and ellipsis
        if (startPage > 1) {
            html += `<span class="page-number" onclick="leaveHistory.goToPage(1)">1</span>`;
            if (startPage > 2) {
                html += `<span class="page-ellipsis">...</span>`;
            }
        }

        // Page numbers
        for (let i = startPage; i <= endPage; i++) {
            const activeClass = i === this.currentPage ? ' active' : '';
            html += `<span class="page-number${activeClass}" onclick="leaveHistory.goToPage(${i})">${i}</span>`;
        }

        // Last page and ellipsis
        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                html += `<span class="page-ellipsis">...</span>`;
            }
            html += `<span class="page-number" onclick="leaveHistory.goToPage(${totalPages})">${totalPages}</span>`;
        }

        return html;
    }

    goToPage(page) {
        const totalPages = Math.ceil(this.filteredRows.length / this.itemsPerPage);

        if (page >= 1 && page <= totalPages) {
            this.currentPage = page;
            this.updateDisplay();
            this.updatePagination();

            // Scroll to top of table content, not the table container
            const tableWrapper = document.querySelector('.table-wrapper');
            if (tableWrapper) {
                tableWrapper.scrollTop = 0;
            }
        }
    }

    previousPage() {
        if (this.currentPage > 1) {
            this.goToPage(this.currentPage - 1);
        }
    }

    nextPage() {
        const totalPages = Math.ceil(this.filteredRows.length / this.itemsPerPage);
        if (this.currentPage < totalPages) {
            this.goToPage(this.currentPage + 1);
        }
    }

    // Utility function for debouncing
    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
}

// Global functions for pagination controls
let leaveHistory;

function applyFilters() {
    if (leaveHistory) {
        leaveHistory.applyFilters();
    }
}

function clearFilters() {
    if (leaveHistory) {
        leaveHistory.clearFilters();
    }
}

function previousPage() {
    if (leaveHistory) {
        leaveHistory.previousPage();
    }
}

function nextPage() {
    if (leaveHistory) {
        leaveHistory.nextPage();
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    leaveHistory = new LeaveHistoryManager();

    // Setup keyboard shortcuts
    document.addEventListener('keydown', function (e) {
        // Ctrl/Cmd + F to focus search
        if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
            e.preventDefault();
            const searchInput = document.getElementById('searchInput');
            if (searchInput) {
                searchInput.focus();
                searchInput.select();
            }
        }

        // Escape to clear search
        if (e.key === 'Escape') {
            const searchInput = document.getElementById('searchInput');
            if (searchInput && searchInput === document.activeElement) {
                clearFilters();
                searchInput.blur();
            }
        }
    });

    // Auto-focus search input
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        // Small delay to ensure page is fully loaded
        setTimeout(() => searchInput.focus(), 100);
    }
});

// Export for testing/debugging
if (typeof module !== 'undefined' && module.exports) {
    module.exports = LeaveHistoryManager;
}