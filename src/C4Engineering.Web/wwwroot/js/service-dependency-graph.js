/**
 * Service Dependency Graph Visualization using D3.js
 * Interactive network diagram showing service relationships
 */
import { ApiClient } from './shared/api-client.js';

class ServiceDependencyGraph {
    constructor(containerId, options = {}) {
        this.containerId = containerId;
        this.apiClient = new ApiClient();
        this.options = {
            width: options.width || 800,
            height: options.height || 600,
            nodeRadius: options.nodeRadius || 40,
            linkDistance: options.linkDistance || 150,
            chargeStrength: options.chargeStrength || -300,
            ...options
        };
        
        this.svg = null;
        this.simulation = null;
        this.nodes = [];
        this.links = [];
        this.selectedNode = null;
        
        this.init();
    }

    async init() {
        this.setupSVG();
        await this.loadServiceData();
        this.setupSimulation();
        this.render();
        this.setupEventListeners();
    }

    setupSVG() {
        const container = d3.select(`#${this.containerId}`);
        
        // Clear existing content
        container.selectAll('*').remove();
        
        this.svg = container
            .append('svg')
            .attr('width', this.options.width)
            .attr('height', this.options.height)
            .attr('viewBox', `0 0 ${this.options.width} ${this.options.height}`)
            .style('border', '1px solid #dee2e6')
            .style('border-radius', '8px');

        // Define arrow markers for different relationship types
        const defs = this.svg.append('defs');
        
        defs.append('marker')
            .attr('id', 'dependency-arrow')
            .attr('viewBox', '0 -5 10 10')
            .attr('refX', 15)
            .attr('refY', -1.5)
            .attr('markerWidth', 6)
            .attr('markerHeight', 6)
            .attr('orient', 'auto')
            .append('path')
            .attr('d', 'M0,-5L10,0L0,5')
            .attr('fill', '#6c757d');

        defs.append('marker')
            .attr('id', 'api-arrow')
            .attr('viewBox', '0 -5 10 10')
            .attr('refX', 15)
            .attr('refY', -1.5)
            .attr('markerWidth', 6)
            .attr('markerHeight', 6)
            .attr('orient', 'auto')
            .append('path')
            .attr('d', 'M0,-5L10,0L0,5')
            .attr('fill', '#0d6efd');

        // Create main groups
        this.linkGroup = this.svg.append('g').attr('class', 'links');
        this.nodeGroup = this.svg.append('g').attr('class', 'nodes');
        this.labelGroup = this.svg.append('g').attr('class', 'labels');
    }

    async loadServiceData() {
        try {
            const services = await this.apiClient.get('/api/services');
            
            // Transform services into nodes
            this.nodes = services.map(service => ({
                id: service.id,
                name: service.name,
                type: service.type,
                owner: service.owner,
                lifecycle: service.lifecycle,
                system: service.system,
                dependsOn: service.dependsOn || [],
                providesApis: service.providesApis || [],
                consumesApis: service.consumesApis || [],
                repository: service.repository,
                documentation: service.documentation,
                metadata: service.metadata,
                // Visual properties
                radius: this.getNodeRadius(service),
                color: this.getNodeColor(service),
                x: Math.random() * this.options.width,
                y: Math.random() * this.options.height
            }));

            // Create links based on dependencies and API relationships
            this.links = [];
            
            this.nodes.forEach(node => {
                // Add dependency links
                node.dependsOn.forEach(dependencyId => {
                    const target = this.nodes.find(n => n.id === dependencyId);
                    if (target) {
                        this.links.push({
                            source: node.id,
                            target: target.id,
                            type: 'dependency',
                            label: 'depends on'
                        });
                    }
                });

                // Add API consumption links
                node.consumesApis.forEach(apiName => {
                    // Find services that provide this API
                    const providers = this.nodes.filter(n => n.providesApis.includes(apiName));
                    providers.forEach(provider => {
                        if (provider.id !== node.id) {
                            this.links.push({
                                source: node.id,
                                target: provider.id,
                                type: 'api',
                                label: `uses ${apiName}`
                            });
                        }
                    });
                });
            });

        } catch (error) {
            console.error('Failed to load service data:', error);
        }
    }

    setupSimulation() {
        this.simulation = d3.forceSimulation(this.nodes)
            .force('link', d3.forceLink(this.links)
                .id(d => d.id)
                .distance(this.options.linkDistance)
                .strength(0.5))
            .force('charge', d3.forceManyBody()
                .strength(this.options.chargeStrength))
            .force('center', d3.forceCenter(this.options.width / 2, this.options.height / 2))
            .force('collision', d3.forceCollide()
                .radius(d => d.radius + 10))
            .on('tick', () => this.ticked());
    }

    render() {
        this.renderLinks();
        this.renderNodes();
        this.renderLabels();
    }

    renderLinks() {
        const link = this.linkGroup
            .selectAll('line')
            .data(this.links)
            .join('line')
            .attr('class', 'service-link')
            .attr('stroke', d => d.type === 'api' ? '#0d6efd' : '#6c757d')
            .attr('stroke-width', d => d.type === 'api' ? 2 : 1.5)
            .attr('stroke-opacity', 0.6)
            .attr('marker-end', d => `url(#${d.type}-arrow)`);

        // Add link labels
        const linkLabels = this.labelGroup
            .selectAll('.link-label')
            .data(this.links)
            .join('text')
            .attr('class', 'link-label')
            .attr('font-size', '10px')
            .attr('fill', '#6c757d')
            .attr('text-anchor', 'middle')
            .attr('dy', -2)
            .text(d => d.label)
            .style('opacity', 0);

        this.linkElements = link;
        this.linkLabelElements = linkLabels;
    }

    renderNodes() {
        const node = this.nodeGroup
            .selectAll('g')
            .data(this.nodes)
            .join('g')
            .attr('class', 'service-node')
            .style('cursor', 'pointer');

        // Add node circles
        node.append('circle')
            .attr('r', d => d.radius)
            .attr('fill', d => d.color)
            .attr('stroke', '#fff')
            .attr('stroke-width', 2);

        // Add node icons
        node.append('text')
            .attr('class', 'node-icon')
            .attr('text-anchor', 'middle')
            .attr('dy', '0.35em')
            .attr('font-family', 'bootstrap-icons')
            .attr('font-size', d => d.radius * 0.6)
            .attr('fill', 'white')
            .text(d => this.getNodeIcon(d.type));

        // Add lifecycle indicators
        node.append('circle')
            .attr('class', 'lifecycle-indicator')
            .attr('r', 6)
            .attr('cx', d => d.radius * 0.7)
            .attr('cy', d => -d.radius * 0.7)
            .attr('fill', d => this.getLifecycleColor(d.lifecycle))
            .attr('stroke', '#fff')
            .attr('stroke-width', 1);

        // Add node interactions
        node
            .on('click', (event, d) => this.handleNodeClick(event, d))
            .on('mouseover', (event, d) => this.handleNodeMouseOver(event, d))
            .on('mouseout', (event, d) => this.handleNodeMouseOut(event, d))
            .call(this.setupDrag());

        this.nodeElements = node;
    }

    renderLabels() {
        const labels = this.labelGroup
            .selectAll('.node-label')
            .data(this.nodes)
            .join('text')
            .attr('class', 'node-label')
            .attr('text-anchor', 'middle')
            .attr('dy', d => d.radius + 15)
            .attr('font-size', '12px')
            .attr('font-weight', '500')
            .attr('fill', '#212529')
            .text(d => d.name);

        // Add owner labels
        const ownerLabels = this.labelGroup
            .selectAll('.owner-label')
            .data(this.nodes)
            .join('text')
            .attr('class', 'owner-label')
            .attr('text-anchor', 'middle')
            .attr('dy', d => d.radius + 30)
            .attr('font-size', '10px')
            .attr('fill', '#6c757d')
            .text(d => d.owner);

        this.labelElements = labels;
        this.ownerLabelElements = ownerLabels;
    }

    setupDrag() {
        return d3.drag()
            .on('start', (event, d) => {
                if (!event.active) this.simulation.alphaTarget(0.3).restart();
                d.fx = d.x;
                d.fy = d.y;
            })
            .on('drag', (event, d) => {
                d.fx = event.x;
                d.fy = event.y;
            })
            .on('end', (event, d) => {
                if (!event.active) this.simulation.alphaTarget(0);
                d.fx = null;
                d.fy = null;
            });
    }

    ticked() {
        this.linkElements
            .attr('x1', d => d.source.x)
            .attr('y1', d => d.source.y)
            .attr('x2', d => d.target.x)
            .attr('y2', d => d.target.y);

        this.linkLabelElements
            .attr('x', d => (d.source.x + d.target.x) / 2)
            .attr('y', d => (d.source.y + d.target.y) / 2);

        this.nodeElements
            .attr('transform', d => `translate(${d.x},${d.y})`);

        this.labelElements
            .attr('x', d => d.x)
            .attr('y', d => d.y);

        this.ownerLabelElements
            .attr('x', d => d.x)
            .attr('y', d => d.y);
    }

    handleNodeClick(event, node) {
        // Clear previous selection
        this.clearSelection();
        
        // Select clicked node
        this.selectedNode = node;
        this.highlightNode(node);
        this.showNodeDetails(node);
    }

    handleNodeMouseOver(event, node) {
        // Show connected links
        this.linkElements
            .style('opacity', d => 
                (d.source.id === node.id || d.target.id === node.id) ? 1 : 0.1);
        
        this.linkLabelElements
            .style('opacity', d => 
                (d.source.id === node.id || d.target.id === node.id) ? 1 : 0);

        // Highlight connected nodes
        this.nodeElements
            .style('opacity', d => {
                const isConnected = this.links.some(link => 
                    (link.source.id === node.id && link.target.id === d.id) ||
                    (link.target.id === node.id && link.source.id === d.id)
                );
                return (d.id === node.id || isConnected) ? 1 : 0.3;
            });

        this.showTooltip(event, node);
    }

    handleNodeMouseOut(event, node) {
        if (!this.selectedNode) {
            // Reset all opacities
            this.linkElements.style('opacity', 0.6);
            this.linkLabelElements.style('opacity', 0);
            this.nodeElements.style('opacity', 1);
        }
        
        this.hideTooltip();
    }

    highlightNode(node) {
        // Highlight selected node and its connections
        this.linkElements
            .style('opacity', d => 
                (d.source.id === node.id || d.target.id === node.id) ? 1 : 0.1)
            .style('stroke-width', d => 
                (d.source.id === node.id || d.target.id === node.id) ? 3 : 1.5);

        this.nodeElements
            .select('circle')
            .style('stroke-width', d => d.id === node.id ? 4 : 2)
            .style('stroke', d => d.id === node.id ? '#0d6efd' : '#fff');
    }

    clearSelection() {
        this.selectedNode = null;
        
        // Reset all styling
        this.linkElements
            .style('opacity', 0.6)
            .style('stroke-width', d => d.type === 'api' ? 2 : 1.5);
        
        this.linkLabelElements.style('opacity', 0);
        
        this.nodeElements
            .style('opacity', 1)
            .select('circle')
            .style('stroke-width', 2)
            .style('stroke', '#fff');
    }

    showNodeDetails(node) {
        // Create or update details panel
        let detailsPanel = d3.select('#service-details-panel');
        
        if (detailsPanel.empty()) {
            detailsPanel = d3.select('body')
                .append('div')
                .attr('id', 'service-details-panel')
                .style('position', 'fixed')
                .style('top', '20px')
                .style('right', '20px')
                .style('width', '300px')
                .style('background', 'white')
                .style('border', '1px solid #dee2e6')
                .style('border-radius', '8px')
                .style('box-shadow', '0 0.5rem 1rem rgba(0, 0, 0, 0.15)')
                .style('padding', '16px')
                .style('z-index', '1000');
        }

        detailsPanel.html(`
            <div class="d-flex justify-content-between align-items-start mb-3">
                <h6 class="mb-0">${node.name}</h6>
                <button type="button" class="btn-close" onclick="this.parentElement.parentElement.remove()"></button>
            </div>
            <div class="mb-2">
                <small class="text-muted">Type:</small>
                <div>${node.type}</div>
            </div>
            <div class="mb-2">
                <small class="text-muted">Owner:</small>
                <div>${node.owner}</div>
            </div>
            <div class="mb-2">
                <small class="text-muted">Lifecycle:</small>
                <div>
                    <span class="badge" style="background-color: ${this.getLifecycleColor(node.lifecycle)}">
                        ${node.lifecycle}
                    </span>
                </div>
            </div>
            ${node.system ? `
                <div class="mb-2">
                    <small class="text-muted">System:</small>
                    <div>${node.system}</div>
                </div>
            ` : ''}
            <div class="mb-2">
                <small class="text-muted">Dependencies:</small>
                <div>${node.dependsOn.length} services</div>
            </div>
            <div class="mb-3">
                <small class="text-muted">APIs:</small>
                <div>Provides ${node.providesApis.length}, Consumes ${node.consumesApis.length}</div>
            </div>
            <div class="btn-group w-100" role="group">
                <a href="/ServiceCatalog/Details/${node.id}" class="btn btn-outline-primary btn-sm">Details</a>
                ${node.documentation ? `<a href="${node.documentation}" target="_blank" class="btn btn-outline-info btn-sm">Docs</a>` : ''}
                ${node.repository ? `<a href="${node.repository}" target="_blank" class="btn btn-outline-secondary btn-sm">Code</a>` : ''}
            </div>
        `);
    }

    showTooltip(event, node) {
        let tooltip = d3.select('#dependency-tooltip');
        
        if (tooltip.empty()) {
            tooltip = d3.select('body')
                .append('div')
                .attr('id', 'dependency-tooltip')
                .style('position', 'absolute')
                .style('background', 'rgba(0, 0, 0, 0.8)')
                .style('color', 'white')
                .style('padding', '8px 12px')
                .style('border-radius', '4px')
                .style('font-size', '12px')
                .style('pointer-events', 'none')
                .style('z-index', '1001')
                .style('opacity', 0);
        }

        tooltip
            .style('left', (event.pageX + 10) + 'px')
            .style('top', (event.pageY - 10) + 'px')
            .html(`
                <div><strong>${node.name}</strong></div>
                <div>${node.owner} • ${node.type}</div>
                <div>${node.dependsOn.length} dependencies</div>
            `)
            .transition()
            .duration(200)
            .style('opacity', 1);
    }

    hideTooltip() {
        d3.select('#dependency-tooltip')
            .transition()
            .duration(200)
            .style('opacity', 0);
    }

    setupEventListeners() {
        // Add zoom and pan
        const zoom = d3.zoom()
            .scaleExtent([0.1, 4])
            .on('zoom', (event) => {
                this.nodeGroup.attr('transform', event.transform);
                this.linkGroup.attr('transform', event.transform);
                this.labelGroup.attr('transform', event.transform);
            });

        this.svg.call(zoom);

        // Clear selection on background click
        this.svg.on('click', (event) => {
            if (event.target === this.svg.node()) {
                this.clearSelection();
                d3.select('#service-details-panel').remove();
            }
        });
    }

    // Utility methods
    getNodeRadius(service) {
        const baseRadius = this.options.nodeRadius;
        const dependencyCount = (service.dependsOn || []).length + 
                               (service.providesApis || []).length + 
                               (service.consumesApis || []).length;
        return Math.max(baseRadius * 0.7, Math.min(baseRadius * 1.3, baseRadius + dependencyCount * 3));
    }

    getNodeColor(service) {
        const colors = {
            'Service': '#1168bd',
            'Website': '#20c997',
            'Library': '#6f42c1',
            'Database': '#198754'
        };
        return colors[service.type] || '#6c757d';
    }

    getNodeIcon(type) {
        const icons = {
            'Service': '\uf6ff', // box-seam
            'Website': '\uf735', // browser-chrome
            'Library': '\uf56e', // archive
            'Database': '\uf2dc'  // database
        };
        return icons[type] || '\uf6ff';
    }

    getLifecycleColor(lifecycle) {
        const colors = {
            'Experimental': '#ffc107',
            'Development': '#0dcaf0',
            'Production': '#198754',
            'Deprecated': '#6c757d'
        };
        return colors[lifecycle] || '#6c757d';
    }

    // Public API methods
    focusOnNode(nodeId) {
        const node = this.nodes.find(n => n.id === nodeId);
        if (node) {
            this.handleNodeClick(null, node);
            
            // Center the view on the node
            const zoom = d3.zoom();
            this.svg.transition()
                .duration(750)
                .call(zoom.transform, 
                    d3.zoomIdentity
                        .translate(this.options.width / 2, this.options.height / 2)
                        .scale(1.5)
                        .translate(-node.x, -node.y));
        }
    }

    highlightPath(fromNodeId, toNodeId) {
        // Implement path highlighting between two nodes
        // This could show the dependency chain between services
    }

    filterNodes(filterFn) {
        this.nodeElements
            .style('opacity', d => filterFn(d) ? 1 : 0.1);
        
        this.linkElements
            .style('opacity', d => 
                (filterFn(d.source) && filterFn(d.target)) ? 0.6 : 0.1);
    }

    resetFilter() {
        this.nodeElements.style('opacity', 1);
        this.linkElements.style('opacity', 0.6);
    }
}

export { ServiceDependencyGraph };