/**
 * Service Catalog Management
 * Handles service discovery, filtering, and management operations
 */
import { ApiClient } from './shared/api-client.js';
import { showSuccessToast, showErrorToast, showLoading, hideLoading, showEmptyState, hideEmptyState, formatDate, getStatusBadgeClass, debounce } from './shared/ui-helpers.js';

class ServiceCatalogManager {
    constructor() {
        this.apiClient = new ApiClient();
        this.services = [];
        this.filteredServices = [];
        this.currentFilters = {
            team: '',
            type: '',
            status: '',
            search: ''
        };
        
        this.init();
    }

    async init() {
        this.setupEventListeners();
        await this.loadServices();
        await this.loadTeams();
    }

    setupEventListeners() {
        // Filter form
        const filterForm = document.getElementById('serviceFilters');
        if (filterForm) {
            filterForm.addEventListener('change', (e) => this.handleFilterChange(e));
            filterForm.addEventListener('input', debounce((e) => this.handleFilterChange(e), 300));
        }

        // Add service form
        const addServiceForm = document.getElementById('addServiceForm');
        if (addServiceForm) {
            addServiceForm.addEventListener('submit', (e) => this.handleAddService(e));
        }

        // Modal events
        const addServiceModal = document.getElementById('addServiceModal');
        if (addServiceModal) {
            addServiceModal.addEventListener('hidden.bs.modal', () => this.resetAddServiceForm());
        }
    }

    async loadServices() {
        try {
            showLoading();
            const services = await this.apiClient.get('/api/services');
            this.services = services || [];
            this.applyFilters();
        } catch (error) {
            console.error('Failed to load services:', error);
            showErrorToast('Failed to load services');
            this.services = [];
            this.renderServices();
        } finally {
            hideLoading();
        }
    }

    async loadTeams() {
        try {
            const teams = await this.apiClient.get('/api/services/teams');
            const teamFilter = document.getElementById('teamFilter');
            if (teamFilter && teams) {
                teams.forEach(team => {
                    const option = document.createElement('option');
                    option.value = team;
                    option.textContent = team;
                    teamFilter.appendChild(option);
                });
            }
        } catch (error) {
            console.error('Failed to load teams:', error);
        }
    }

    handleFilterChange(event) {
        const target = event.target;
        const filterId = target.id;
        
        switch (filterId) {
            case 'teamFilter':
                this.currentFilters.team = target.value;
                break;
            case 'typeFilter':
                this.currentFilters.type = target.value;
                break;
            case 'statusFilter':
                this.currentFilters.status = target.value;
                break;
            case 'searchFilter':
                this.currentFilters.search = target.value.toLowerCase();
                break;
        }
        
        this.applyFilters();
    }

    applyFilters() {
        this.filteredServices = this.services.filter(service => {
            // Team filter
            if (this.currentFilters.team && service.owner !== this.currentFilters.team) {
                return false;
            }
            
            // Type filter
            if (this.currentFilters.type && service.type !== this.currentFilters.type) {
                return false;
            }
            
            // Status filter (assuming we have status in service data)
            if (this.currentFilters.status && service.status !== this.currentFilters.status) {
                return false;
            }
            
            // Search filter
            if (this.currentFilters.search) {
                const searchTerm = this.currentFilters.search;
                const searchableText = [
                    service.name,
                    service.description || '',
                    service.owner,
                    ...(service.tags || [])
                ].join(' ').toLowerCase();
                
                if (!searchableText.includes(searchTerm)) {
                    return false;
                }
            }
            
            return true;
        });
        
        this.renderServices();
    }

    renderServices() {
        const container = document.getElementById('servicesGrid');
        if (!container) return;

        if (this.filteredServices.length === 0) {
            container.innerHTML = '';
            showEmptyState();
            return;
        }

        hideEmptyState();
        
        const servicesHtml = this.filteredServices.map(service => this.createServiceCard(service)).join('');
        container.innerHTML = servicesHtml;
        
        // Add event listeners to service cards
        this.attachServiceCardListeners();
    }

    createServiceCard(service) {
        const statusClass = getStatusBadgeClass(service.status || 'Active');
        const tagsHtml = (service.tags || []).slice(0, 3).map(tag => 
            `<span class="badge bg-light text-dark me-1">${tag}</span>`
        ).join('');
        
        return `
            <div class="col-md-6 col-lg-4 mb-4">
                <div class="card h-100 service-card" data-service-id="${service.id}">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <div class="d-flex align-items-center">
                            <i class="bi bi-box-seam me-2"></i>
                            <h6 class="mb-0">${service.name}</h6>
                        </div>
                        <span class="badge ${statusClass}">${service.status || 'Active'}</span>
                    </div>
                    <div class="card-body">
                        <p class="card-text text-muted small mb-2">${service.description || 'No description available'}</p>
                        <div class="mb-2">
                            <small class="text-muted">Owner:</small>
                            <div class="fw-semibold">${service.owner}</div>
                        </div>
                        <div class="mb-2">
                            <small class="text-muted">Type:</small>
                            <div>${service.type}</div>
                        </div>
                        ${tagsHtml ? `<div class="mb-2">${tagsHtml}</div>` : ''}
                        <div class="mb-2">
                            <small class="text-muted">Dependencies:</small>
                            <div>${(service.dependencies || []).length} services</div>
                        </div>
                        <div>
                            <small class="text-muted">Updated:</small>
                            <div>${formatDate(service.updatedAt)}</div>
                        </div>
                    </div>
                    <div class="card-footer">
                        <div class="btn-group w-100" role="group">
                            <button type="button" class="btn btn-outline-primary btn-sm view-service-btn">
                                <i class="bi bi-eye"></i> View
                            </button>
                            <button type="button" class="btn btn-outline-info btn-sm view-architecture-btn">
                                <i class="bi bi-diagram-3"></i> Architecture
                            </button>
                            <button type="button" class="btn btn-outline-success btn-sm deploy-service-btn">
                                <i class="bi bi-play"></i> Deploy
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    attachServiceCardListeners() {
        document.querySelectorAll('.view-service-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const serviceId = e.target.closest('.service-card').dataset.serviceId;
                this.viewServiceDetails(serviceId);
            });
        });

        document.querySelectorAll('.view-architecture-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const serviceId = e.target.closest('.service-card').dataset.serviceId;
                this.viewServiceArchitecture(serviceId);
            });
        });

        document.querySelectorAll('.deploy-service-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const serviceId = e.target.closest('.service-card').dataset.serviceId;
                this.deployService(serviceId);
            });
        });
    }

    async handleAddService(event) {
        event.preventDefault();
        
        const formData = new FormData(event.target);
        const serviceData = {
            name: formData.get('name'),
            type: formData.get('type'),
            description: formData.get('description'),
            owner: formData.get('owner'),
            repository: formData.get('repository'),
            documentation: formData.get('documentation'),
            apiSpec: formData.get('apiSpec'),
            tags: formData.get('tags') ? formData.get('tags').split(',').map(tag => tag.trim()) : []
        };

        try {
            const newService = await this.apiClient.post('/api/services', serviceData);
            showSuccessToast('Service created successfully!');
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('addServiceModal'));
            modal.hide();
            
            // Reload services
            await this.loadServices();
        } catch (error) {
            console.error('Failed to create service:', error);
            showErrorToast(error.message || 'Failed to create service');
        }
    }

    resetAddServiceForm() {
        const form = document.getElementById('addServiceForm');
        if (form) {
            form.reset();
        }
    }

    viewServiceDetails(serviceId) {
        window.location.href = `/ServiceCatalog/Details/${serviceId}`;
    }

    viewServiceArchitecture(serviceId) {
        // Navigate to architecture view with service context
        window.location.href = `/Architecture?serviceId=${serviceId}`;
    }

    deployService(serviceId) {
        // Navigate to deployment with service pre-selected
        window.location.href = `/Deployment?deployService=${serviceId}`;
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ServiceCatalogManager();
});