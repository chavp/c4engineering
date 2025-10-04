// Service Details Page JavaScript Module
import { ApiClient } from './shared/api-client.js';
import { showToast, showErrorToast } from './shared/ui-helpers.js';

class ServiceDetailsManager {
    constructor() {
        this.apiClient = new ApiClient();
        this.serviceId = this.getServiceIdFromUrl();
        this.initializeEventListeners();
        this.loadRelatedDiagrams();
        this.checkHealthStatus();
    }

    getServiceIdFromUrl() {
        const urlParts = window.location.pathname.split('/');
        return urlParts[urlParts.length - 1];
    }

    initializeEventListeners() {
        // Health check refresh button
        const refreshHealthButton = document.getElementById('refreshHealthCheck');
        if (refreshHealthButton) {
            refreshHealthButton.addEventListener('click', () => this.checkHealthStatus());
        }

        // Edit service form
        const editServiceForm = document.getElementById('editServiceForm');
        if (editServiceForm) {
            editServiceForm.addEventListener('submit', (e) => this.handleEditService(e));
        }

        // Edit service modal show event
        const editServiceModal = document.getElementById('editServiceModal');
        if (editServiceModal) {
            editServiceModal.addEventListener('show.bs.modal', () => this.populateEditForm());
        }
    }

    async checkHealthStatus() {
        const healthStatusContainer = document.getElementById('healthStatus');
        if (!healthStatusContainer) return;

        const healthUrl = healthStatusContainer.dataset.healthUrl;
        if (!healthUrl) return;

        try {
            healthStatusContainer.innerHTML = `
                <div class="d-flex align-items-center">
                    <div class="spinner-border spinner-border-sm me-2" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    Checking health status...
                </div>
            `;

            const response = await fetch(healthUrl, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                },
                timeout: 10000
            });

            if (response.ok) {
                const healthData = await response.json();
                this.renderHealthStatus(healthData, true);
            } else {
                this.renderHealthStatus({ status: 'Unhealthy', details: 'HTTP ' + response.status }, false);
            }
        } catch (error) {
            console.error('Health check failed:', error);
            this.renderHealthStatus({ 
                status: 'Unknown', 
                details: error.message || 'Health check failed'
            }, false);
        }
    }

    renderHealthStatus(healthData, isHealthy) {
        const healthStatusContainer = document.getElementById('healthStatus');
        if (!healthStatusContainer) return;

        const statusClass = isHealthy ? 'health-status-healthy' : 'health-status-unhealthy';
        const iconClass = isHealthy ? 'bi-check-circle-fill' : 'bi-x-circle-fill';
        const status = healthData.status || (isHealthy ? 'Healthy' : 'Unhealthy');

        let detailsHtml = '';
        if (healthData.details) {
            detailsHtml = `<div class="mt-2 small text-muted">${healthData.details}</div>`;
        }

        healthStatusContainer.innerHTML = `
            <div class="d-flex align-items-center ${statusClass}">
                <i class="bi ${iconClass} me-2"></i>
                <strong>${status}</strong>
                <span class="ms-auto small text-muted">
                    Last checked: ${new Date().toLocaleTimeString()}
                </span>
            </div>
            ${detailsHtml}
        `;
    }

    async loadRelatedDiagrams() {
        const relatedDiagramsContainer = document.getElementById('relatedDiagrams');
        if (!relatedDiagramsContainer) return;

        try {
            const diagrams = await this.apiClient.get(`/api/diagrams/by-service/${this.serviceId}`);
            this.renderRelatedDiagrams(diagrams);
        } catch (error) {
            console.error('Failed to load related diagrams:', error);
            relatedDiagramsContainer.innerHTML = `
                <div class="text-muted small">
                    <i class="bi bi-exclamation-circle me-1"></i>
                    Unable to load related diagrams
                </div>
            `;
        }
    }

    renderRelatedDiagrams(diagrams) {
        const relatedDiagramsContainer = document.getElementById('relatedDiagrams');
        if (!relatedDiagramsContainer) return;

        if (!diagrams || diagrams.length === 0) {
            relatedDiagramsContainer.innerHTML = `
                <div class="text-muted small">
                    <i class="bi bi-diagram-3 me-1"></i>
                    No related diagrams found
                </div>
            `;
            return;
        }

        const diagramsHtml = diagrams.map(diagram => `
            <div class="mb-2">
                <a href="/Diagrams/Details/${diagram.id}" class="text-decoration-none">
                    <div class="d-flex align-items-center">
                        <i class="bi bi-diagram-2 me-2 text-primary"></i>
                        <div>
                            <div class="fw-medium">${diagram.name}</div>
                            <div class="small text-muted">${diagram.type}</div>
                        </div>
                    </div>
                </a>
            </div>
        `).join('');

        relatedDiagramsContainer.innerHTML = diagramsHtml;
    }

    async populateEditForm() {
        const form = document.getElementById('editServiceForm');
        if (!form) return;

        try {
            const service = await this.apiClient.get(`/api/services/${this.serviceId}`);
            
            const formFields = `
                <div class="row">
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label for="editServiceName" class="form-label">Service Name *</label>
                            <input type="text" class="form-control" id="editServiceName" name="name" value="${service.name || ''}" required>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label for="editServiceType" class="form-label">Type *</label>
                            <select class="form-select" id="editServiceType" name="type" required>
                                <option value="Service" ${service.type === 'Service' ? 'selected' : ''}>Service</option>
                                <option value="Library" ${service.type === 'Library' ? 'selected' : ''}>Library</option>
                                <option value="Website" ${service.type === 'Website' ? 'selected' : ''}>Website</option>
                                <option value="Database" ${service.type === 'Database' ? 'selected' : ''}>Database</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div class="mb-3">
                    <label for="editServiceDescription" class="form-label">Description</label>
                    <textarea class="form-control" id="editServiceDescription" name="description" rows="3">${service.description || ''}</textarea>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label for="editServiceOwner" class="form-label">Owner Team *</label>
                            <input type="text" class="form-control" id="editServiceOwner" name="owner" value="${service.owner || ''}" required>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label for="editServiceLifecycle" class="form-label">Lifecycle</label>
                            <select class="form-select" id="editServiceLifecycle" name="lifecycle">
                                <option value="Experimental" ${service.lifecycle === 'Experimental' ? 'selected' : ''}>Experimental</option>
                                <option value="Development" ${service.lifecycle === 'Development' ? 'selected' : ''}>Development</option>
                                <option value="Production" ${service.lifecycle === 'Production' ? 'selected' : ''}>Production</option>
                                <option value="Deprecated" ${service.lifecycle === 'Deprecated' ? 'selected' : ''}>Deprecated</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label for="editServiceRepository" class="form-label">Repository URL</label>
                            <input type="url" class="form-control" id="editServiceRepository" name="repository" value="${service.repository || ''}">
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label for="editServiceDocumentation" class="form-label">Documentation URL</label>
                            <input type="url" class="form-control" id="editServiceDocumentation" name="documentation" value="${service.documentation || ''}">
                        </div>
                    </div>
                </div>
                <div class="mb-3">
                    <label for="editServiceTags" class="form-label">Tags</label>
                    <input type="text" class="form-control" id="editServiceTags" name="tags" value="${(service.tags || []).join(', ')}" placeholder="Comma-separated tags">
                    <div class="form-text">Enter tags separated by commas</div>
                </div>
                <div class="mb-3">
                    <label for="editServiceSystem" class="form-label">System</label>
                    <input type="text" class="form-control" id="editServiceSystem" name="system" value="${service.system || ''}" placeholder="System or domain this service belongs to">
                </div>
            `;

            const modalBody = form.querySelector('.modal-body');
            if (modalBody) {
                modalBody.innerHTML = formFields;
            }
        } catch (error) {
            console.error('Failed to load service for editing:', error);
            showErrorToast('Failed to load service details for editing');
        }
    }

    async handleEditService(event) {
        event.preventDefault();
        
        const form = event.target;
        const formData = new FormData(form);
        
        const updateData = {
            name: formData.get('name'),
            description: formData.get('description'),
            owner: formData.get('owner'),
            repository: formData.get('repository'),
            documentation: formData.get('documentation'),
            tags: formData.get('tags') ? formData.get('tags').split(',').map(tag => tag.trim()).filter(tag => tag) : [],
            lifecycle: formData.get('lifecycle'),
            system: formData.get('system')
        };

        try {
            await this.apiClient.put(`/api/services/${this.serviceId}`, updateData);
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('editServiceModal'));
            if (modal) {
                modal.hide();
            }
            
            showToast('Service updated successfully! Refreshing page...');
            
            // Refresh page after a short delay
            setTimeout(() => {
                window.location.reload();
            }, 1500);
            
        } catch (error) {
            console.error('Failed to update service:', error);
            showErrorToast(error.message || 'Failed to update service');
        }
    }
}

// Global functions for button actions
window.viewDependencyGraph = function(serviceId) {
    window.location.href = `/ServiceCatalog?view=graph&highlight=${serviceId}`;
};

window.exportServiceInfo = function(serviceId) {
    // Implement service export functionality
    console.log('Export service info for:', serviceId);
    showToast('Export functionality coming soon!');
};

window.shareService = function(serviceId) {
    if (navigator.share) {
        navigator.share({
            title: document.title,
            url: window.location.href
        }).catch(console.error);
    } else {
        // Fallback: copy URL to clipboard
        navigator.clipboard.writeText(window.location.href).then(() => {
            showToast('Service URL copied to clipboard!');
        }).catch(() => {
            showErrorToast('Failed to copy URL to clipboard');
        });
    }
};

window.deleteService = function(serviceId) {
    if (confirm('Are you sure you want to delete this service? This action cannot be undone.')) {
        const apiClient = new ApiClient();
        apiClient.delete(`/api/services/${serviceId}`)
            .then(() => {
                showToast('Service deleted successfully! Redirecting...');
                setTimeout(() => {
                    window.location.href = '/ServiceCatalog';
                }, 1500);
            })
            .catch(error => {
                console.error('Failed to delete service:', error);
                showErrorToast(error.message || 'Failed to delete service');
            });
    }
};

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ServiceDetailsManager();
});

export { ServiceDetailsManager };