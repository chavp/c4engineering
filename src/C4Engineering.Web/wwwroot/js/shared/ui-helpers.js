/**
 * UI Helper functions for Bootstrap components and common interactions
 */

/**
 * Show success toast notification
 */
export function showToast(message, type = 'success') {
    const toastId = type === 'success' ? 'successToast' : 'errorToast';
    const toast = document.getElementById(toastId);
    
    if (toast) {
        if (type === 'error') {
            const errorMessage = toast.querySelector('#errorMessage');
            if (errorMessage) {
                errorMessage.textContent = message;
            }
        } else {
            const toastBody = toast.querySelector('.toast-body');
            if (toastBody) {
                toastBody.textContent = message;
            }
        }
        
        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();
    }
}

/**
 * Show error toast notification
 */
export function showErrorToast(message) {
    showToast(message, 'error');
}

/**
 * Show success toast notification
 */
export function showSuccessToast(message) {
    showToast(message, 'success');
}

/**
 * Show error modal with details
 */
export function showErrorModal(title, message) {
    // Create modal if it doesn't exist
    let errorModal = document.getElementById('errorModal');
    if (!errorModal) {
        errorModal = createErrorModal();
        document.body.appendChild(errorModal);
    }
    
    const modalTitle = errorModal.querySelector('.modal-title');
    const modalBody = errorModal.querySelector('.modal-body');
    
    if (modalTitle) modalTitle.textContent = title;
    if (modalBody) modalBody.textContent = message;
    
    const bsModal = new bootstrap.Modal(errorModal);
    bsModal.show();
}

/**
 * Create error modal dynamically
 */
function createErrorModal() {
    const modal = document.createElement('div');
    modal.className = 'modal fade';
    modal.id = 'errorModal';
    modal.tabIndex = -1;
    modal.innerHTML = `
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Error</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    An error occurred.
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                </div>
            </div>
        </div>
    `;
    return modal;
}

/**
 * Show loading spinner
 */
export function showLoading(containerId = 'loadingSpinner') {
    const container = document.getElementById(containerId);
    if (container) {
        container.style.display = 'block';
    }
}

/**
 * Hide loading spinner
 */
export function hideLoading(containerId = 'loadingSpinner') {
    const container = document.getElementById(containerId);
    if (container) {
        container.style.display = 'none';
    }
}

/**
 * Show empty state
 */
export function showEmptyState(containerId = 'emptyState') {
    const container = document.getElementById(containerId);
    if (container) {
        container.style.display = 'block';
    }
}

/**
 * Hide empty state
 */
export function hideEmptyState(containerId = 'emptyState') {
    const container = document.getElementById(containerId);
    if (container) {
        container.style.display = 'none';
    }
}

/**
 * Format date for display
 */
export function formatDate(date, options = {}) {
    if (!date) return '-';
    
    const defaultOptions = {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    };
    
    const formatOptions = { ...defaultOptions, ...options };
    return new Date(date).toLocaleDateString('en-US', formatOptions);
}

/**
 * Format duration in milliseconds to human readable
 */
export function formatDuration(milliseconds) {
    if (!milliseconds) return '-';
    
    const seconds = Math.floor(milliseconds / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    
    if (hours > 0) {
        return `${hours}h ${minutes % 60}m ${seconds % 60}s`;
    } else if (minutes > 0) {
        return `${minutes}m ${seconds % 60}s`;
    } else {
        return `${seconds}s`;
    }
}

/**
 * Get Bootstrap badge class for status
 */
export function getStatusBadgeClass(status) {
    const statusClasses = {
        'Active': 'bg-success',
        'Running': 'bg-primary',
        'Success': 'bg-success',
        'Failed': 'bg-danger',
        'Error': 'bg-danger',
        'Stopped': 'bg-secondary',
        'Queued': 'bg-warning',
        'Pending': 'bg-warning',
        'Cancelled': 'bg-secondary',
        'Deprecated': 'bg-warning',
        'Development': 'bg-info'
    };
    
    return statusClasses[status] || 'bg-secondary';
}

/**
 * Debounce function for search inputs
 */
export function debounce(func, wait, immediate) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            timeout = null;
            if (!immediate) func(...args);
        };
        const callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func(...args);
    };
}

/**
 * Copy text to clipboard
 */
export async function copyToClipboard(text) {
    try {
        await navigator.clipboard.writeText(text);
        showSuccessToast('Copied to clipboard');
    } catch (err) {
        console.error('Failed to copy text: ', err);
        showErrorToast('Failed to copy to clipboard');
    }
}