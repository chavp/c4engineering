/**
 * C4 Diagram Editor with D3.js
 * Interactive diagram creation and editing with real-time collaboration
 */
import { ApiClient } from './shared/api-client.js';
import { showSuccessToast, showErrorToast } from './shared/ui-helpers.js';

class C4DiagramEditor {
    constructor() {
        this.apiClient = new ApiClient();
        this.svg = null;
        this.zoom = null;
        this.drag = null;
        this.elements = new Map();
        this.relationships = new Map();
        this.selectedElement = null;
        this.connectionMode = false;
        this.connectionStart = null;
        this.diagramId = this.getDiagramIdFromUrl();
        this.currentZoom = 1;
        this.snapToGrid = true;
        this.gridSize = 20;
        
        // SignalR connection for real-time collaboration
        this.connection = null;
        this.isCollaborationEnabled = false;
        
        this.init();
    }

    init() {
        this.setupSVG();
        this.setupDragAndDrop();
        this.setupEventListeners();
        this.setupSignalR();
        this.loadDiagram();
    }

    setupSVG() {
        const container = d3.select('#diagramContainer');
        const containerRect = container.node().getBoundingClientRect();
        
        this.svg = container.select('canvas').remove();
        this.svg = container
            .append('svg')
            .attr('class', 'diagram-svg')
            .attr('width', '100%')
            .attr('height', '600px')
            .style('background', 'transparent');

        // Define arrow markers
        const defs = this.svg.append('defs');
        defs.append('marker')
            .attr('id', 'arrowhead')
            .attr('viewBox', '0 -5 10 10')
            .attr('refX', 8)
            .attr('refY', 0)
            .attr('markerWidth', 6)
            .attr('markerHeight', 6)
            .attr('orient', 'auto')
            .append('path')
            .attr('d', 'M0,-5L10,0L0,5')
            .attr('fill', '#495057');

        // Create main group for zoom/pan
        this.mainGroup = this.svg.append('g').attr('class', 'main-group');
        
        // Setup zoom behavior
        this.zoom = d3.zoom()
            .scaleExtent([0.1, 3])
            .on('zoom', (event) => {
                this.mainGroup.attr('transform', event.transform);
                this.currentZoom = event.transform.k;
                this.updateZoomLabel();
            });

        this.svg.call(this.zoom);

        // Handle canvas clicks
        this.svg.on('click', (event) => {
            if (event.target === this.svg.node()) {
                this.clearSelection();
            }
        });

        // Handle drops on canvas
        this.svg.on('drop', (event) => {
            event.preventDefault();
            const elementType = event.dataTransfer.getData('text/plain');
            if (elementType) {
                const point = d3.pointer(event, this.mainGroup.node());
                this.createElement(elementType, point[0], point[1]);
            }
        });

        this.svg.on('dragover', (event) => {
            event.preventDefault();
        });
    }

    setupDragAndDrop() {
        // Setup palette items as draggable
        d3.selectAll('.palette-item')
            .on('dragstart', (event) => {
                const elementType = event.target.getAttribute('data-type');
                event.dataTransfer.setData('text/plain', elementType);
                event.target.classList.add('dragging');
            })
            .on('dragend', (event) => {
                event.target.classList.remove('dragging');
            });
    }

    setupEventListeners() {
        // Zoom controls
        d3.select('#zoomIn').on('click', () => {
            this.svg.transition().call(this.zoom.scaleBy, 1.2);
        });

        d3.select('#zoomOut').on('click', () => {
            this.svg.transition().call(this.zoom.scaleBy, 0.8);
        });

        d3.select('#zoomReset').on('click', () => {
            this.svg.transition().call(this.zoom.transform, d3.zoomIdentity);
        });

        // Snap to grid toggle
        d3.select('#snapToGrid').on('change', (event) => {
            this.snapToGrid = event.target.checked;
        });

        // Save button
        d3.select('#saveBtn').on('click', () => {
            this.saveDiagram();
        });

        // Export buttons
        d3.select('#exportPng').on('click', () => this.exportDiagram('png'));
        d3.select('#exportSvg').on('click', () => this.exportDiagram('svg'));
        d3.select('#exportPdf').on('click', () => this.exportDiagram('pdf'));

        // Properties form
        d3.select('#propertiesForm').on('input', () => {
            this.updateSelectedElementProperties();
        });

        d3.select('#deleteElement').on('click', () => {
            this.deleteSelectedElement();
        });

        // Keyboard shortcuts
        d3.select('body').on('keydown', (event) => {
            this.handleKeyDown(event);
        });
    }

    async setupSignalR() {
        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl('/hubs/diagrams')
                .build();

            this.connection.on('UserJoined', (userId) => {
                this.showCollaborationNotification(`${userId} joined the diagram`);
            });

            this.connection.on('UserLeft', (userId) => {
                this.showCollaborationNotification(`${userId} left the diagram`);
            });

            this.connection.on('ElementUpdated', (element) => {
                this.handleRemoteElementUpdate(element);
            });

            this.connection.on('ElementAdded', (element) => {
                this.handleRemoteElementAdd(element);
            });

            this.connection.on('ElementRemoved', (elementId) => {
                this.handleRemoteElementRemove(elementId);
            });

            this.connection.on('RelationshipAdded', (relationship) => {
                this.handleRemoteRelationshipAdd(relationship);
            });

            this.connection.on('RelationshipRemoved', (relationshipId) => {
                this.handleRemoteRelationshipRemove(relationshipId);
            });

            await this.connection.start();
            
            if (this.diagramId) {
                await this.connection.invoke('JoinDiagram', this.diagramId);
                this.isCollaborationEnabled = true;
            }
        } catch (error) {
            console.warn('SignalR connection failed:', error);
        }
    }

    createElement(type, x, y) {
        if (this.snapToGrid) {
            x = Math.round(x / this.gridSize) * this.gridSize;
            y = Math.round(y / this.gridSize) * this.gridSize;
        }

        const element = {
            id: this.generateId(),
            type: type,
            x: x,
            y: y,
            width: this.getDefaultWidth(type),
            height: this.getDefaultHeight(type),
            name: this.getDefaultName(type),
            description: '',
            technology: '',
            color: this.getDefaultColor(type)
        };

        this.elements.set(element.id, element);
        this.renderElement(element);
        this.selectElement(element.id);

        // Broadcast to collaborators
        if (this.isCollaborationEnabled) {
            this.connection.invoke('AddElement', this.diagramId, element);
        }
    }

    renderElement(element) {
        const group = this.mainGroup
            .append('g')
            .attr('class', 'c4-element')
            .attr('data-id', element.id)
            .attr('transform', `translate(${element.x}, ${element.y})`);

        // Main rectangle
        group.append('rect')
            .attr('class', `c4-element-rect c4-${element.type}`)
            .attr('width', element.width)
            .attr('height', element.height)
            .attr('fill', element.color)
            .attr('stroke', '#fff')
            .attr('stroke-width', 2);

        // Element name
        group.append('text')
            .attr('class', 'c4-element-text c4-element-name')
            .attr('x', element.width / 2)
            .attr('y', element.height / 2 - 15)
            .text(element.name);

        // Element type
        group.append('text')
            .attr('class', 'c4-element-text c4-element-type')
            .attr('x', element.width / 2)
            .attr('y', element.height / 2)
            .text(`[${element.type.replace('-', ' ')}]`);

        // Element description (if exists)
        if (element.description) {
            group.append('text')
                .attr('class', 'c4-element-text c4-element-description')
                .attr('x', element.width / 2)
                .attr('y', element.height / 2 + 15)
                .text(element.description);
        }

        // Add connection points
        this.addConnectionPoints(group, element);

        // Setup interactions
        this.setupElementInteractions(group, element);
    }

    addConnectionPoints(group, element) {
        const points = [
            { x: element.width / 2, y: 0 }, // top
            { x: element.width, y: element.height / 2 }, // right
            { x: element.width / 2, y: element.height }, // bottom
            { x: 0, y: element.height / 2 } // left
        ];

        points.forEach((point, index) => {
            group.append('circle')
                .attr('class', 'connection-point')
                .attr('cx', point.x)
                .attr('cy', point.y)
                .attr('data-side', ['top', 'right', 'bottom', 'left'][index])
                .on('click', (event) => {
                    event.stopPropagation();
                    this.handleConnectionPointClick(element.id, point, event);
                });
        });
    }

    setupElementInteractions(group, element) {
        // Click to select
        group.on('click', (event) => {
            event.stopPropagation();
            this.selectElement(element.id);
        });

        // Double-click to edit name
        group.on('dblclick', (event) => {
            event.stopPropagation();
            this.editElementName(element.id);
        });

        // Context menu
        group.on('contextmenu', (event) => {
            event.preventDefault();
            this.showContextMenu(event, element.id);
        });

        // Drag behavior
        const drag = d3.drag()
            .on('start', (event) => {
                this.selectElement(element.id);
            })
            .on('drag', (event) => {
                let newX = element.x + event.dx;
                let newY = element.y + event.dy;

                if (this.snapToGrid) {
                    newX = Math.round(newX / this.gridSize) * this.gridSize;
                    newY = Math.round(newY / this.gridSize) * this.gridSize;
                }

                element.x = newX;
                element.y = newY;

                group.attr('transform', `translate(${newX}, ${newY})`);
                this.updateElementRelationships(element.id);
            })
            .on('end', () => {
                // Broadcast change to collaborators
                if (this.isCollaborationEnabled) {
                    this.connection.invoke('UpdateElement', this.diagramId, element);
                }
            });

        group.call(drag);
    }

    selectElement(elementId) {
        // Clear previous selection
        this.clearSelection();

        const element = this.elements.get(elementId);
        if (!element) return;

        this.selectedElement = elementId;

        // Visual selection
        d3.select(`[data-id="${elementId}"]`).classed('selected', true);

        // Update properties panel
        this.showElementProperties(element);
    }

    clearSelection() {
        d3.selectAll('.c4-element').classed('selected', false);
        d3.selectAll('.c4-relationship').classed('selected', false);
        this.selectedElement = null;
        this.hideElementProperties();
    }

    showElementProperties(element) {
        d3.select('#noSelection').style('display', 'none');
        d3.select('#elementProperties').style('display', 'block');

        d3.select('#elementName').property('value', element.name);
        d3.select('#elementDescription').property('value', element.description);
        d3.select('#elementTechnology').property('value', element.technology);
        d3.select('#elementColor').property('value', element.color);
    }

    hideElementProperties() {
        d3.select('#noSelection').style('display', 'block');
        d3.select('#elementProperties').style('display', 'none');
    }

    updateSelectedElementProperties() {
        if (!this.selectedElement) return;

        const element = this.elements.get(this.selectedElement);
        if (!element) return;

        element.name = d3.select('#elementName').property('value');
        element.description = d3.select('#elementDescription').property('value');
        element.technology = d3.select('#elementTechnology').property('value');
        element.color = d3.select('#elementColor').property('value');

        // Update visual representation
        this.updateElementVisual(element);

        // Broadcast change to collaborators
        if (this.isCollaborationEnabled) {
            this.connection.invoke('UpdateElement', this.diagramId, element);
        }
    }

    updateElementVisual(element) {
        const group = d3.select(`[data-id="${element.id}"]`);
        
        group.select('.c4-element-rect').attr('fill', element.color);
        group.select('.c4-element-name').text(element.name);
        
        if (element.description) {
            let descText = group.select('.c4-element-description');
            if (descText.empty()) {
                descText = group.append('text')
                    .attr('class', 'c4-element-text c4-element-description')
                    .attr('x', element.width / 2)
                    .attr('y', element.height / 2 + 15);
            }
            descText.text(element.description.substring(0, 50) + (element.description.length > 50 ? '...' : ''));
        }
    }

    createRelationship(fromId, toId, label = '') {
        const fromElement = this.elements.get(fromId);
        const toElement = this.elements.get(toId);
        
        if (!fromElement || !toElement) return;

        const relationship = {
            id: this.generateId(),
            from: fromId,
            to: toId,
            label: label,
            type: 'uses'
        };

        this.relationships.set(relationship.id, relationship);
        this.renderRelationship(relationship);

        // Broadcast to collaborators
        if (this.isCollaborationEnabled) {
            this.connection.invoke('AddRelationship', this.diagramId, relationship);
        }
    }

    renderRelationship(relationship) {
        const fromElement = this.elements.get(relationship.from);
        const toElement = this.elements.get(relationship.to);

        const fromCenter = {
            x: fromElement.x + fromElement.width / 2,
            y: fromElement.y + fromElement.height / 2
        };

        const toCenter = {
            x: toElement.x + toElement.width / 2,
            y: toElement.y + toElement.height / 2
        };

        // Calculate connection points on element edges
        const fromPoint = this.getConnectionPoint(fromElement, toCenter);
        const toPoint = this.getConnectionPoint(toElement, fromCenter);

        const group = this.mainGroup
            .append('g')
            .attr('class', 'c4-relationship')
            .attr('data-id', relationship.id);

        // Draw the line
        group.append('line')
            .attr('x1', fromPoint.x)
            .attr('y1', fromPoint.y)
            .attr('x2', toPoint.x)
            .attr('y2', toPoint.y)
            .attr('marker-end', 'url(#arrowhead)');

        // Add label if exists
        if (relationship.label) {
            const midX = (fromPoint.x + toPoint.x) / 2;
            const midY = (fromPoint.y + toPoint.y) / 2;

            group.append('text')
                .attr('class', 'c4-relationship-label')
                .attr('x', midX)
                .attr('y', midY)
                .text(relationship.label);
        }

        // Setup interactions
        group.on('click', (event) => {
            event.stopPropagation();
            this.selectRelationship(relationship.id);
        });
    }

    getConnectionPoint(element, targetCenter) {
        const elementCenter = {
            x: element.x + element.width / 2,
            y: element.y + element.height / 2
        };

        const dx = targetCenter.x - elementCenter.x;
        const dy = targetCenter.y - elementCenter.y;

        // Determine which edge to connect to
        const aspectRatio = element.width / element.height;
        const threshold = Math.abs(dy / dx);

        if (threshold > aspectRatio) {
            // Connect to top or bottom
            return {
                x: elementCenter.x,
                y: dy > 0 ? element.y + element.height : element.y
            };
        } else {
            // Connect to left or right
            return {
                x: dx > 0 ? element.x + element.width : element.x,
                y: elementCenter.y
            };
        }
    }

    updateElementRelationships(elementId) {
        this.relationships.forEach((relationship) => {
            if (relationship.from === elementId || relationship.to === elementId) {
                // Remove old visual
                d3.select(`[data-id="${relationship.id}"]`).remove();
                // Render updated
                this.renderRelationship(relationship);
            }
        });
    }

    handleConnectionPointClick(elementId, point, event) {
        if (!this.connectionMode) {
            this.connectionMode = true;
            this.connectionStart = { elementId, point };
            
            // Show connection preview
            this.showConnectionPreview(event);
        } else {
            // Complete connection
            if (this.connectionStart.elementId !== elementId) {
                const label = prompt('Enter relationship label (optional):') || '';
                this.createRelationship(this.connectionStart.elementId, elementId, label);
            }
            this.cancelConnection();
        }
    }

    showConnectionPreview(event) {
        // Implementation for showing connection line preview
        // This would follow mouse movement until connection is completed
    }

    cancelConnection() {
        this.connectionMode = false;
        this.connectionStart = null;
        d3.select('.connection-preview').remove();
    }

    async saveDiagram() {
        try {
            const diagramData = {
                id: this.diagramId,
                elements: Array.from(this.elements.values()),
                relationships: Array.from(this.relationships.values())
            };

            await this.apiClient.put(`/api/diagrams/${this.diagramId}`, diagramData);
            showSuccessToast('Diagram saved successfully!');
        } catch (error) {
            console.error('Failed to save diagram:', error);
            showErrorToast('Failed to save diagram');
        }
    }

    async loadDiagram() {
        if (!this.diagramId) return;

        try {
            const diagram = await this.apiClient.get(`/api/diagrams/${this.diagramId}`);
            
            // Load elements
            if (diagram.elements) {
                diagram.elements.forEach(element => {
                    this.elements.set(element.id, element);
                    this.renderElement(element);
                });
            }

            // Load relationships
            if (diagram.relationships) {
                diagram.relationships.forEach(relationship => {
                    this.relationships.set(relationship.id, relationship);
                    this.renderRelationship(relationship);
                });
            }

            // Update diagram title
            d3.select('#diagramTitle').text(diagram.name || 'Untitled Diagram');
            d3.select('#diagramMeta').text(`C4 ${diagram.type} Diagram`);
        } catch (error) {
            console.error('Failed to load diagram:', error);
        }
    }

    exportDiagram(format) {
        const svgElement = this.svg.node();
        
        switch (format) {
            case 'svg':
                this.downloadSVG(svgElement);
                break;
            case 'png':
                this.downloadPNG(svgElement);
                break;
            case 'pdf':
                this.downloadPDF(svgElement);
                break;
        }
    }

    downloadSVG(svgElement) {
        const serializer = new XMLSerializer();
        const svgString = serializer.serializeToString(svgElement);
        const blob = new Blob([svgString], { type: 'image/svg+xml' });
        this.downloadBlob(blob, 'diagram.svg');
    }

    downloadPNG(svgElement) {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        const img = new Image();
        
        const serializer = new XMLSerializer();
        const svgString = serializer.serializeToString(svgElement);
        const blob = new Blob([svgString], { type: 'image/svg+xml' });
        const url = URL.createObjectURL(blob);
        
        img.onload = () => {
            canvas.width = img.width;
            canvas.height = img.height;
            ctx.drawImage(img, 0, 0);
            
            canvas.toBlob((pngBlob) => {
                this.downloadBlob(pngBlob, 'diagram.png');
                URL.revokeObjectURL(url);
            });
        };
        
        img.src = url;
    }

    downloadBlob(blob, filename) {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }

    // Utility methods
    generateId() {
        return 'elem_' + Math.random().toString(36).substr(2, 9);
    }

    getDiagramIdFromUrl() {
        const params = new URLSearchParams(window.location.search);
        return params.get('id') || 'new-diagram';
    }

    getDefaultWidth(type) {
        const widths = {
            'person': 120,
            'system': 180,
            'external-system': 180,
            'container': 160,
            'component': 140,
            'database': 120,
            'api': 140,
            'web-app': 160,
            'mobile-app': 140,
            'message-queue': 140
        };
        return widths[type] || 160;
    }

    getDefaultHeight(type) {
        const heights = {
            'person': 100,
            'system': 120,
            'external-system': 120,
            'container': 100,
            'component': 80,
            'database': 80,
            'api': 80,
            'web-app': 100,
            'mobile-app': 100,
            'message-queue': 80
        };
        return heights[type] || 100;
    }

    getDefaultName(type) {
        const names = {
            'person': 'User',
            'system': 'Software System',
            'external-system': 'External System',
            'container': 'Container',
            'component': 'Component',
            'database': 'Database',
            'api': 'API',
            'web-app': 'Web Application',
            'mobile-app': 'Mobile App',
            'message-queue': 'Message Queue'
        };
        return names[type] || 'Element';
    }

    getDefaultColor(type) {
        const colors = {
            'person': '#1168bd',
            'system': '#999999',
            'external-system': '#666666',
            'container': '#1168bd',
            'component': '#85bbf0',
            'database': '#198754',
            'api': '#fd7e14',
            'web-app': '#20c997',
            'mobile-app': '#6f42c1',
            'message-queue': '#dc3545'
        };
        return colors[type] || '#1168bd';
    }

    updateZoomLabel() {
        d3.select('#zoomReset').text(`${Math.round(this.currentZoom * 100)}%`);
    }

    handleKeyDown(event) {
        if (event.key === 'Delete' && this.selectedElement) {
            this.deleteSelectedElement();
        } else if (event.key === 'Escape') {
            this.clearSelection();
            this.cancelConnection();
        }
    }

    deleteSelectedElement() {
        if (!this.selectedElement) return;

        // Remove relationships connected to this element
        const relationsToDelete = [];
        this.relationships.forEach((relationship, id) => {
            if (relationship.from === this.selectedElement || relationship.to === this.selectedElement) {
                relationsToDelete.push(id);
            }
        });

        relationsToDelete.forEach(id => {
            this.relationships.delete(id);
            d3.select(`[data-id="${id}"]`).remove();
        });

        // Remove the element
        this.elements.delete(this.selectedElement);
        d3.select(`[data-id="${this.selectedElement}"]`).remove();

        // Broadcast to collaborators
        if (this.isCollaborationEnabled) {
            this.connection.invoke('RemoveElement', this.diagramId, this.selectedElement);
        }

        this.clearSelection();
    }

    // Real-time collaboration handlers
    handleRemoteElementUpdate(element) {
        this.elements.set(element.id, element);
        d3.select(`[data-id="${element.id}"]`).remove();
        this.renderElement(element);
        
        // Visual feedback for remote update
        d3.select(`[data-id="${element.id}"]`).classed('real-time-update', true);
        setTimeout(() => {
            d3.select(`[data-id="${element.id}"]`).classed('real-time-update', false);
        }, 1000);
    }

    handleRemoteElementAdd(element) {
        this.elements.set(element.id, element);
        this.renderElement(element);
    }

    handleRemoteElementRemove(elementId) {
        this.elements.delete(elementId);
        d3.select(`[data-id="${elementId}"]`).remove();
    }

    handleRemoteRelationshipAdd(relationship) {
        this.relationships.set(relationship.id, relationship);
        this.renderRelationship(relationship);
    }

    handleRemoteRelationshipRemove(relationshipId) {
        this.relationships.delete(relationshipId);
        d3.select(`[data-id="${relationshipId}"]`).remove();
    }

    showCollaborationNotification(message) {
        console.log('Collaboration:', message);
        // You could show this in a toast or status bar
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new C4DiagramEditor();
});