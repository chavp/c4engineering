/**
 * Architecture Diagrams Index
 * Manages the architecture diagrams listing and creation
 */
import { ApiClient } from './shared/api-client.js';
import { showSuccessToast, showErrorToast, showLoading, hideLoading, showEmptyState, hideEmptyState, formatDate } from './shared/ui-helpers.js';

class ArchitectureManager {
    constructor() {
        this.apiClient = new ApiClient();
        this.diagrams = [];
        this.filteredDiagrams = [];
        this.currentC4Level = 'Context';
        
        this.init();
    }

    async init() {
        this.setupEventListeners();
        await this.loadDiagrams();
        await this.loadSystems();
        await this.loadServices();
    }

    setupEventListeners() {
        // C4 Level filter
        document.querySelectorAll('input[name="c4Level"]').forEach(radio => {
            radio.addEventListener('change', (e) => {
                this.currentC4Level = e.target.value;
                this.filterDiagrams();
            });
        });

        // Create diagram button
        document.getElementById('createDiagramBtn')?.addEventListener('click', () => {
            this.showCreateDiagramModal();
        });

        document.getElementById('createFirstDiagramBtn')?.addEventListener('click', () => {
            this.showCreateDiagramModal();
        });

        // Import from service button
        document.getElementById('importFromServiceBtn')?.addEventListener('click', () => {
            this.showImportServiceModal();
        });

        // Create diagram form
        document.getElementById('createDiagramForm')?.addEventListener('submit', (e) => {
            this.handleCreateDiagram(e);
        });

        // Import service form
        document.getElementById('importServiceForm')?.addEventListener('submit', (e) => {
            this.handleImportFromService(e);
        });

        // Real-time collaboration toggle
        document.getElementById('realTimeMode')?.addEventListener('change', (e) => {
            this.toggleRealTimeMode(e.target.checked);
        });
    }

    async loadDiagrams() {
        try {
            showLoading();
            const diagrams = await this.apiClient.get('/api/diagrams');
            this.diagrams = diagrams || [];
            this.filterDiagrams();
        } catch (error) {
            console.error('Failed to load diagrams:', error);
            showErrorToast('Failed to load diagrams');
            this.diagrams = [];
            this.renderDiagrams();
        } finally {
            hideLoading();
        }
    }

    async loadSystems() {
        try {
            const services = await this.apiClient.get('/api/services');
            const systems = [...new Set(services.filter(s => s.system).map(s => s.system))];
            
            const systemSelect = document.getElementById('diagramSystem');
            if (systemSelect) {
                systems.forEach(system => {
                    const option = document.createElement('option');
                    option.value = system;
                    option.textContent = system;
                    systemSelect.appendChild(option);
                });
            }
        } catch (error) {
            console.error('Failed to load systems:', error);
        }
    }

    async loadServices() {
        try {
            const services = await this.apiClient.get('/api/services');
            const serviceSelect = document.getElementById('sourceService');
            
            if (serviceSelect) {
                services.forEach(service => {
                    const option = document.createElement('option');
                    option.value = service.id;
                    option.textContent = `${service.name} (${service.owner})`;
                    serviceSelect.appendChild(option);
                });
            }
        } catch (error) {
            console.error('Failed to load services:', error);
        }
    }

    filterDiagrams() {
        this.filteredDiagrams = this.diagrams.filter(diagram => 
            diagram.type === this.currentC4Level
        );
        this.renderDiagrams();
    }

    renderDiagrams() {
        const container = document.getElementById('diagramsGrid');
        if (!container) return;

        if (this.filteredDiagrams.length === 0) {
            container.innerHTML = '';
            showEmptyState();
            return;
        }

        hideEmptyState();
        
        const diagramsHtml = this.filteredDiagrams.map(diagram => this.createDiagramCard(diagram)).join('');
        container.innerHTML = diagramsHtml;
        
        // Add event listeners to diagram cards
        this.attachDiagramCardListeners();
    }

    createDiagramCard(diagram) {
        const lastModified = formatDate(diagram.updatedAt || diagram.createdAt);
        const elementCount = (diagram.elements || []).length;
        const relationshipCount = (diagram.relationships || []).length;
        
        return `
            <div class="col-md-6 col-lg-4 mb-4">
                <div class="card h-100 diagram-card" data-diagram-id="${diagram.id}">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <div class="d-flex align-items-center">
                            <i class="bi bi-diagram-3 me-2"></i>
                            <h6 class="mb-0">${diagram.name}</h6>
                        </div>
                        <span class="badge bg-primary">${diagram.type}</span>
                    </div>
                    <div class="card-body">
                        <p class="card-text text-muted small mb-2">${diagram.description || 'No description available'}</p>
                        
                        ${diagram.system ? `
                            <div class="mb-2">
                                <small class="text-muted">System:</small>
                                <div class="fw-semibold">${diagram.system}</div>
                            </div>
                        ` : ''}
                        
                        <div class="mb-2">
                            <small class="text-muted">Elements:</small>
                            <div>${elementCount} elements, ${relationshipCount} relationships</div>
                        </div>
                        
                        <div class="mb-2">
                            <small class="text-muted">Last Modified:</small>
                            <div>${lastModified}</div>
                        </div>
                        
                        ${diagram.isCollaborative ? `
                            <div class="mb-2">
                                <span class="badge bg-success">
                                    <i class="bi bi-people"></i> Collaborative
                                </span>
                            </div>
                        ` : ''}
                    </div>
                    <div class="card-footer">
                        <div class="btn-group w-100" role="group">
                            <button type="button" class="btn btn-outline-primary btn-sm view-diagram-btn">
                                <i class="bi bi-eye"></i> View
                            </button>
                            <button type="button" class="btn btn-outline-info btn-sm edit-diagram-btn">
                                <i class="bi bi-pencil"></i> Edit
                            </button>
                            <button type="button" class="btn btn-outline-success btn-sm export-diagram-btn">
                                <i class="bi bi-download"></i> Export
                            </button>
                            <div class="btn-group" role="group">
                                <button type="button" class="btn btn-outline-secondary btn-sm dropdown-toggle" data-bs-toggle="dropdown">
                                    <i class="bi bi-three-dots"></i>
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a class="dropdown-item duplicate-diagram-btn" href="#">
                                        <i class="bi bi-files"></i> Duplicate
                                    </a></li>
                                    <li><a class="dropdown-item configure-pipeline-btn" href="#">
                                        <i class="bi bi-gear"></i> Configure Pipeline
                                    </a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a class="dropdown-item text-danger delete-diagram-btn" href="#">
                                        <i class="bi bi-trash"></i> Delete
                                    </a></li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    attachDiagramCardListeners() {
        document.querySelectorAll('.view-diagram-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const diagramId = e.target.closest('.diagram-card').dataset.diagramId;
                this.viewDiagram(diagramId, 'view');
            });
        });

        document.querySelectorAll('.edit-diagram-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const diagramId = e.target.closest('.diagram-card').dataset.diagramId;
                this.viewDiagram(diagramId, 'edit');
            });
        });

        document.querySelectorAll('.export-diagram-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const diagramId = e.target.closest('.diagram-card').dataset.diagramId;
                this.exportDiagram(diagramId);
            });
        });

        document.querySelectorAll('.duplicate-diagram-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const diagramId = e.target.closest('.diagram-card').dataset.diagramId;
                this.duplicateDiagram(diagramId);
            });
        });

        document.querySelectorAll('.configure-pipeline-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const diagramId = e.target.closest('.diagram-card').dataset.diagramId;
                this.configurePipeline(diagramId);
            });
        });

        document.querySelectorAll('.delete-diagram-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const diagramId = e.target.closest('.diagram-card').dataset.diagramId;
                this.deleteDiagram(diagramId);
            });
        });
    }

    showCreateDiagramModal() {
        const modal = new bootstrap.Modal(document.getElementById('createDiagramModal'));
        modal.show();
    }

    showImportServiceModal() {
        const modal = new bootstrap.Modal(document.getElementById('importServiceModal'));
        modal.show();
    }

    async handleCreateDiagram(event) {
        event.preventDefault();
        
        const formData = new FormData(event.target);
        const diagramData = {
            name: formData.get('name'),
            type: formData.get('type'),
            system: formData.get('system') || null,
            description: formData.get('description') || null
        };

        try {
            const newDiagram = await this.apiClient.post('/api/diagrams', diagramData);
            showSuccessToast('Diagram created successfully!');
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('createDiagramModal'));
            modal.hide();
            
            // Reload diagrams
            await this.loadDiagrams();
            
            // Navigate to editor
            this.viewDiagram(newDiagram.id, 'edit');
        } catch (error) {
            console.error('Failed to create diagram:', error);
            showErrorToast(error.message || 'Failed to create diagram');
        }
    }

    async handleImportFromService(event) {
        event.preventDefault();
        
        const formData = new FormData(event.target);
        const serviceId = formData.get('serviceId');
        const diagramType = formData.get('type');
        const diagramName = formData.get('name');

        try {
            const generatedDiagram = await this.apiClient.post('/api/diagrams/generate', {
                serviceId: serviceId,
                type: diagramType,
                name: diagramName
            });
            
            showSuccessToast('Diagram generated successfully!');
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('importServiceModal'));
            modal.hide();
            
            // Reload diagrams
            await this.loadDiagrams();
            
            // Navigate to editor
            this.viewDiagram(generatedDiagram.id, 'edit');
        } catch (error) {
            console.error('Failed to generate diagram:', error);
            showErrorToast(error.message || 'Failed to generate diagram');
        }
    }

    viewDiagram(diagramId, mode = 'view') {
        if (mode === 'edit') {
            window.location.href = `/Architecture/Editor?id=${diagramId}`;
        } else {
            window.location.href = `/Architecture/Editor?id=${diagramId}&mode=view`;
        }
    }

    async exportDiagram(diagramId) {
        try {
            // For now, navigate to editor with export action
            window.location.href = `/Architecture/Editor?id=${diagramId}&action=export`;
        } catch (error) {
            console.error('Failed to export diagram:', error);
            showErrorToast('Failed to export diagram');
        }
    }

    async duplicateDiagram(diagramId) {
        try {
            const originalDiagram = this.diagrams.find(d => d.id === diagramId);
            if (!originalDiagram) return;

            const duplicateData = {
                ...originalDiagram,
                id: undefined, // Let server generate new ID
                name: `${originalDiagram.name} (Copy)`,
                createdAt: undefined,
                updatedAt: undefined
            };

            const newDiagram = await this.apiClient.post('/api/diagrams', duplicateData);
            showSuccessToast('Diagram duplicated successfully!');
            
            await this.loadDiagrams();
        } catch (error) {
            console.error('Failed to duplicate diagram:', error);
            showErrorToast('Failed to duplicate diagram');
        }
    }

    async configurePipeline(diagramId) {
        // Navigate to pipeline configuration for this diagram
        window.location.href = `/Pipeline/Configure?diagramId=${diagramId}`;
    }

    async deleteDiagram(diagramId) {
        const diagram = this.diagrams.find(d => d.id === diagramId);
        if (!diagram) return;

        const confirmed = confirm(`Are you sure you want to delete "${diagram.name}"? This action cannot be undone.`);
        if (!confirmed) return;

        try {
            await this.apiClient.delete(`/api/diagrams/${diagramId}`);
            showSuccessToast('Diagram deleted successfully!');
            
            await this.loadDiagrams();
        } catch (error) {
            console.error('Failed to delete diagram:', error);
            showErrorToast('Failed to delete diagram');
        }
    }

    toggleRealTimeMode(enabled) {
        // This would enable/disable real-time collaboration features
        console.log('Real-time collaboration:', enabled ? 'enabled' : 'disabled');
        
        if (enabled) {
            // Show collaboration status
            this.showCollaborationStatus();
        } else {
            this.hideCollaborationStatus();
        }
    }

    showCollaborationStatus() {
        // Visual feedback that real-time mode is active
        const badge = document.createElement('span');
        badge.className = 'badge bg-success ms-2';
        badge.innerHTML = '<i class="bi bi-broadcast"></i> Live';
        badge.id = 'realTimeBadge';
        
        const heading = document.querySelector('h1.h2');
        if (heading && !document.getElementById('realTimeBadge')) {
            heading.appendChild(badge);
        }
    }

    hideCollaborationStatus() {
        const badge = document.getElementById('realTimeBadge');
        if (badge) {
            badge.remove();
        }
    }
}

// Auto-populate diagram name based on selected service
document.addEventListener('DOMContentLoaded', () => {
    const sourceServiceSelect = document.getElementById('sourceService');
    const generatedNameInput = document.getElementById('generatedDiagramName');
    const diagramTypeSelect = document.getElementById('generatedDiagramType');
    
    if (sourceServiceSelect && generatedNameInput && diagramTypeSelect) {
        const updateGeneratedName = () => {
            const selectedOption = sourceServiceSelect.options[sourceServiceSelect.selectedIndex];
            const diagramType = diagramTypeSelect.value;
            
            if (selectedOption && selectedOption.value) {
                const serviceName = selectedOption.textContent.split(' (')[0];
                const typeLabel = diagramType === 'Context' ? 'Context' : 'Container';
                generatedNameInput.value = `${serviceName} - ${typeLabel} Diagram`;
            }
        };
        
        sourceServiceSelect.addEventListener('change', updateGeneratedName);
        diagramTypeSelect.addEventListener('change', updateGeneratedName);
    }
    
    // Initialize the architecture manager
    new ArchitectureManager();
});