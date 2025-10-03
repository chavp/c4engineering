/**
 * Enhanced Service Catalog Management with Graph Visualization
 * Combines grid view with D3.js dependency graph
 */
import { ApiClient } from './shared/api-client.js';
import { showSuccessToast, showErrorToast, showLoading, hideLoading, showEmptyState, hideEmptyState, formatDate, getStatusBadgeClass, debounce } from './shared/ui-helpers.js';
import { ServiceDependencyGraph } from './service-dependency-graph.js';

class EnhancedServiceCatalogManager {
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
        this.currentView = 'grid';
        this.dependencyGraph = null;
        
        this.init();
    }

    async init() {
        this.setupEventListeners();
        await this.loadServices();
        await this.loadTeams();
        this.initializeGraph();
    }

    setupEventListeners() {
        // View mode toggle
        document.querySelectorAll('input[name="viewMode"]').forEach(radio => {
            radio.addEventListener('change', (e) => {
                this.switchView(e.target.value);
            });
        });

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

        // Graph controls
        document.getElementById('resetGraphView')?.addEventListener('click', () => {
            if (this.dependencyGraph) {
                this.dependencyGraph.resetFilter();
            }
        });

        document.getElementById('centerGraph')?.addEventListener('click', () => {
            // Center the graph view
            if (this.dependencyGraph) {
                this.dependencyGraph.resetFilter();
                // Could implement centering logic here
            }
        });

        document.getElementById('exportGraph')?.addEventListener('click', () => {
            this.exportGraphView();
        });

        document.getElementById('showLabels')?.addEventListener('change', (e) => {
            this.toggleGraphLabels(e.target.checked);
        });
    }

    async loadServices() {
        try {
            showLoading();
            const services = await this.apiClient.get('/api/services');
            this.services = services || [];
            this.applyFilters();
            
            // Update graph if it's initialized
            if (this.dependencyGraph && this.currentView === 'graph') {
                await this.refreshGraph();
            }
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
                // Clear existing options except "All Teams"
                teamFilter.innerHTML = '<option value="">All Teams</option>';
                
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

    initializeGraph() {
        if (!this.dependencyGraph) {
            this.dependencyGraph = new ServiceDependencyGraph('dependencyGraph', {
                width: 800,
                height: 600,
                nodeRadius: 35,
                linkDistance: 120,
                chargeStrength: -400
            });
        }
    }

    switchView(viewMode) {
        this.currentView = viewMode;
        
        if (viewMode === 'grid') {
            document.getElementById('gridViewContainer').style.display = 'block';
            document.getElementById('graphViewContainer').style.display = 'none';
            this.renderServices();
        } else if (viewMode === 'graph') {
            document.getElementById('gridViewContainer').style.display = 'none';
            document.getElementById('graphViewContainer').style.display = 'block';
            this.refreshGraph();
        }
    }

    async refreshGraph() {
        if (this.dependencyGraph) {
            // Reinitialize the graph with current data
            await this.dependencyGraph.init();
            this.applyGraphFilters();
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
            
            // Status filter (map to lifecycle)
            if (this.currentFilters.status) {
                const lifecycle = this.mapStatusToLifecycle(service.lifecycle);
                if (lifecycle !== this.currentFilters.status) {
                    return false;
                }
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
        
        if (this.currentView === 'grid') {
            this.renderServices();
        } else if (this.currentView === 'graph') {
            this.applyGraphFilters();
        }
    }

    applyGraphFilters() {
        if (this.dependencyGraph) {
            const filterFn = (node) => {
                return this.filteredServices.some(service => service.id === node.id);
            };
            this.dependencyGraph.filterNodes(filterFn);
        }
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
        const statusClass = getStatusBadgeClass(this.mapLifecycleToStatus(service.lifecycle));
        const tagsHtml = (service.tags || []).slice(0, 3).map(tag => 
            `<span class="badge bg-light text-dark me-1">${tag}</span>`
        ).join('');
        
        const dependencyCount = (service.dependsOn || []).length;
        const apiCount = (service.providesApis || []).length + (service.consumesApis || []).length;
        
        return `
            <div class="col-md-6 col-lg-4 mb-4">
                <div class="card h-100 service-card" data-service-id="${service.id}">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <div class="d-flex align-items-center">
                            <i class="bi bi-${this.getServiceIcon(service.type)} me-2"></i>
                            <h6 class="mb-0">${service.name}</h6>
                        </div>
                        <span class="badge ${statusClass}">${this.mapLifecycleToStatus(service.lifecycle)}</span>
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
                        ${service.system ? `
                            <div class="mb-2">
                                <small class="text-muted">System:</small>
                                <div>${service.system}</div>
                            </div>
                        ` : ''}
                        ${tagsHtml ? `<div class="mb-2">${tagsHtml}</div>` : ''}
                        <div class="row">
                            <div class="col-6">
                                <small class="text-muted">Dependencies:</small>
                                <div class="fw-semibold">${dependencyCount}</div>
                            </div>
                            <div class="col-6">
                                <small class="text-muted">APIs:</small>
                                <div class="fw-semibold">${apiCount}</div>
                            </div>
                        </div>
                        <div class="mt-2">
                            <small class="text-muted">Updated:</small>
                            <div>${formatDate(service.metadata?.updatedAt || service.metadata?.createdAt)}</div>
                        </div>
                    </div>
                    <div class="card-footer">
                        <div class="btn-group w-100" role="group">
                            <button type="button" class="btn btn-outline-primary btn-sm view-service-btn">
                                <i class="bi bi-eye"></i> View
                            </button>
                            <button type="button" class="btn btn-outline-info btn-sm view-graph-btn">
                                <i class="bi bi-diagram-3"></i> Graph
                            </button>
                            <button type="button" class="btn btn-outline-success btn-sm view-architecture-btn">
                                <i class="bi bi-box-seam"></i> Architecture
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

        document.querySelectorAll('.view-graph-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const serviceId = e.target.closest('.service-card').dataset.serviceId;
                this.viewServiceInGraph(serviceId);
            });
        });

        document.querySelectorAll('.view-architecture-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const serviceId = e.target.closest('.service-card').dataset.serviceId;
                this.viewServiceArchitecture(serviceId);
            });
        });
    }

    async handleAddService(event) {
        event.preventDefault();
        
        const formData = new FormData(event.target);
        const serviceData = {
            id: this.generateServiceId(formData.get('name')),
            name: formData.get('name'),
            type: formData.get('type'),
            description: formData.get('description'),
            owner: formData.get('owner'),
            repository: formData.get('repository'),
            documentation: formData.get('documentation'),
            tags: formData.get('tags') ? formData.get('tags').split(',').map(tag => tag.trim()) : [],
            lifecycle: formData.get('lifecycle') || 'Production',
            system: formData.get('system') || null
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

    viewServiceInGraph(serviceId) {
        // Switch to graph view and focus on the service
        document.getElementById('graphView').checked = true;
        this.switchView('graph');
        
        // Focus on the specific service node
        setTimeout(() => {
            if (this.dependencyGraph) {
                this.dependencyGraph.focusOnNode(serviceId);
            }
        }, 500);
    }

    viewServiceArchitecture(serviceId) {
        // Navigate to architecture view with service context
        window.location.href = `/Architecture?serviceId=${serviceId}`;
    }

    exportGraphView() {
        if (this.dependencyGraph) {
            // Export the current graph view as SVG
            const svgElement = document.querySelector('#dependencyGraph svg');
            if (svgElement) {
                const serializer = new XMLSerializer();
                const svgString = serializer.serializeToString(svgElement);
                const blob = new Blob([svgString], { type: 'image/svg+xml' });
                
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = 'service-dependency-graph.svg';
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(url);
            }
        }
    }

    toggleGraphLabels(show) {
        if (this.dependencyGraph) {
            const labels = document.querySelectorAll('#dependencyGraph .node-label, #dependencyGraph .owner-label');
            labels.forEach(label => {
                label.style.display = show ? 'block' : 'none';
            });
        }
    }

    // Utility methods
    generateServiceId(name) {
        return name.toLowerCase()
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/^-+|-+$/g, '');
    }

    getServiceIcon(type) {
        const icons = {
            'Service': 'box-seam',
            'Website': 'browser-chrome',
            'Library': 'archive',
            'Database': 'database'
        };
        return icons[type] || 'box-seam';
    }

    mapLifecycleToStatus(lifecycle) {
        const mapping = {
            'Experimental': 'Experimental',
            'Development': 'Development',
            'Production': 'Active',
            'Deprecated': 'Deprecated'
        };
        return mapping[lifecycle] || 'Active';
    }

    mapStatusToLifecycle(lifecycle) {
        // For filtering - map status filter values to lifecycle values
        const mapping = {
            'Experimental': 'Experimental',
            'Development': 'Development',
            'Production': 'Production',
            'Deprecated': 'Deprecated'
        };
        return mapping[lifecycle] || lifecycle;
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new EnhancedServiceCatalogManager();
});