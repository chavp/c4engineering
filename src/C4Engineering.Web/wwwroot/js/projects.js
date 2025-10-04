// Projects Page JavaScript Module
import { ApiClient } from './shared/api-client.js';
import { showToast, showErrorToast } from './shared/ui-helpers.js';

class ProjectsManager {
    constructor() {
        this.apiClient = new ApiClient();
        this.projects = [];
        this.filteredProjects = [];
        
        this.initializeEventListeners();
        this.loadProjects();
        this.populateOwnerFilter();
    }

    initializeEventListeners() {
        // Filter event listeners
        document.getElementById('typeFilter')?.addEventListener('change', () => this.applyFilters());
        document.getElementById('statusFilter')?.addEventListener('change', () => this.applyFilters());
        document.getElementById('ownerFilter')?.addEventListener('change', () => this.applyFilters());
        document.getElementById('searchFilter')?.addEventListener('input', () => this.applyFilters());
    }

    async loadProjects() {
        try {
            const spinner = document.getElementById('loadingSpinner');
            if (spinner) {
                spinner.style.display = 'block';
            }

            this.projects = await this.apiClient.get('/api/projects');
            this.filteredProjects = [...this.projects];
            this.renderProjects();
        } catch (error) {
            console.error('Failed to load projects:', error);
            showErrorToast('Failed to load projects');
        } finally {
            const spinner = document.getElementById('loadingSpinner');
            if (spinner) {
                spinner.style.display = 'none';
            }
        }
    }

    async populateOwnerFilter() {
        try {
            const owners = [...new Set(this.projects.map(p => p.owner))].sort();
            const ownerFilter = document.getElementById('ownerFilter');
            
            if (ownerFilter && owners.length > 0) {
                // Clear existing options except "All Owners"
                const allOption = ownerFilter.querySelector('option[value=""]');
                ownerFilter.innerHTML = '';
                ownerFilter.appendChild(allOption);
                
                // Add owner options
                owners.forEach(owner => {
                    const option = document.createElement('option');
                    option.value = owner;
                    option.textContent = owner;
                    ownerFilter.appendChild(option);
                });
            }
        } catch (error) {
            console.error('Failed to populate owner filter:', error);
        }
    }

    applyFilters() {
        const typeFilter = document.getElementById('typeFilter')?.value || '';
        const statusFilter = document.getElementById('statusFilter')?.value || '';
        const ownerFilter = document.getElementById('ownerFilter')?.value || '';
        const searchFilter = document.getElementById('searchFilter')?.value.toLowerCase() || '';

        this.filteredProjects = this.projects.filter(project => {
            const matchesType = !typeFilter || project.type === typeFilter;
            const matchesStatus = !statusFilter || project.status === statusFilter;
            const matchesOwner = !ownerFilter || project.owner === ownerFilter;
            const matchesSearch = !searchFilter || 
                project.name.toLowerCase().includes(searchFilter) ||
                (project.description && project.description.toLowerCase().includes(searchFilter)) ||
                project.owner.toLowerCase().includes(searchFilter) ||
                project.tags.some(tag => tag.toLowerCase().includes(searchFilter));

            return matchesType && matchesStatus && matchesOwner && matchesSearch;
        });

        this.renderProjects();
    }

    renderProjects() {
        const container = document.getElementById('projectsGrid');
        if (!container) return;

        if (this.filteredProjects.length === 0) {
            container.innerHTML = `
                <div class="col-12">
                    <div class="text-center py-5">
                        <div class="mb-3">
                            <i class="bi bi-search display-1 text-muted"></i>
                        </div>
                        <h5 class="text-muted">No projects found</h5>
                        <p class="text-muted">Try adjusting your filters or create a new project.</p>
                        <a href="/Projects/Create" class="btn btn-success">
                            <i class="bi bi-plus-circle"></i> Create New Project
                        </a>
                    </div>
                </div>
            `;
            return;
        }

        const projectsHtml = this.filteredProjects.map(project => `
            <div class="col-lg-4 col-md-6 mb-4 project-card" 
                 data-type="${project.type}" 
                 data-status="${project.status}" 
                 data-owner="${project.owner}">
                <div class="card h-100">
                    <div class="card-header d-flex justify-content-between align-items-start">
                        <div>
                            <h5 class="card-title mb-1">
                                <a href="/Projects/Details/${project.id}" class="text-decoration-none">
                                    ${project.name}
                                </a>
                            </h5>
                            <div class="d-flex align-items-center gap-2">
                                <span class="badge bg-primary">${project.type}</span>
                                <span class="badge ${this.getStatusBadgeClass(project.status)}">${project.status}</span>
                            </div>
                        </div>
                        <div class="dropdown">
                            <button class="btn btn-sm btn-outline-secondary" type="button" data-bs-toggle="dropdown">
                                <i class="bi bi-three-dots"></i>
                            </button>
                            <ul class="dropdown-menu">
                                <li><a class="dropdown-item" href="/Projects/Details/${project.id}">
                                    <i class="bi bi-eye"></i> View Details
                                </a></li>
                                <li><a class="dropdown-item" href="#" onclick="editProject('${project.id}')">
                                    <i class="bi bi-pencil"></i> Edit
                                </a></li>
                                <li><hr class="dropdown-divider"></li>
                                <li><a class="dropdown-item text-danger" href="#" onclick="deleteProject('${project.id}')">
                                    <i class="bi bi-trash"></i> Delete
                                </a></li>
                            </ul>
                        </div>
                    </div>
                    <div class="card-body">
                        <p class="card-text">${project.description || '<span class="text-muted">No description available.</span>'}</p>
                        
                        <div class="row text-center mt-3">
                            <div class="col-3">
                                <div class="fw-bold text-primary">${project.serviceCount}</div>
                                <small class="text-muted">Services</small>
                            </div>
                            <div class="col-3">
                                <div class="fw-bold text-info">${project.diagramCount}</div>
                                <small class="text-muted">Diagrams</small>
                            </div>
                            <div class="col-3">
                                <div class="fw-bold text-success">${project.pipelineCount}</div>
                                <small class="text-muted">Pipelines</small>
                            </div>
                            <div class="col-3">
                                <div class="fw-bold text-warning">${project.teamMemberCount}</div>
                                <small class="text-muted">Members</small>
                            </div>
                        </div>
                    </div>
                    <div class="card-footer">
                        <div class="d-flex justify-content-between align-items-center">
                            <small class="text-muted">Owner: ${project.owner}</small>
                            <small class="text-muted">Updated: ${new Date(project.updatedAt).toLocaleDateString()}</small>
                        </div>
                        ${project.tags.length > 0 ? `
                            <div class="mt-2">
                                ${project.tags.slice(0, 3).map(tag => `<span class="badge bg-light text-dark me-1">${tag}</span>`).join('')}
                                ${project.tags.length > 3 ? `<span class="badge bg-light text-dark">+${project.tags.length - 3} more</span>` : ''}
                            </div>
                        ` : ''}
                    </div>
                </div>
            </div>
        `).join('');

        container.innerHTML = projectsHtml;
    }

    getStatusBadgeClass(status) {
        const statusClasses = {
            'Planning': 'bg-warning',
            'Active': 'bg-success',
            'Development': 'bg-info',
            'Production': 'bg-success',
            'Maintenance': 'bg-secondary',
            'Deprecated': 'bg-danger',
            'Archived': 'bg-dark'
        };
        return statusClasses[status] || 'bg-secondary';
    }
}

// Global functions for project actions
window.editProject = function(projectId) {
    // Implement edit functionality
    console.log('Edit project:', projectId);
    showToast('Edit functionality coming soon!');
};

window.deleteProject = function(projectId) {
    if (confirm('Are you sure you want to delete this project? This action cannot be undone.')) {
        const apiClient = new ApiClient();
        apiClient.delete(`/api/projects/${projectId}`)
            .then(() => {
                showToast('Project deleted successfully!');
                // Reload projects
                window.location.reload();
            })
            .catch(error => {
                console.error('Failed to delete project:', error);
                showErrorToast(error.message || 'Failed to delete project');
            });
    }
};

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ProjectsManager();
});

export { ProjectsManager };