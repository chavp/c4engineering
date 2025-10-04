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
        this.selectedRelationship = null;
        this.connectionMode = false;
        this.connectionStart = null;
        this.diagramId = this.getDiagramIdFromUrl();
        this.currentZoom = 1;
        this.snapToGrid = true;
        this.gridSize = 20;
        
        // Relationship management
        this.isEditingRelationship = false;
        this.currentRelationshipId = null;
        
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

    // Helper method to create arrow markers
    createArrowMarker(defs, id, path, fill) {
        defs.append('marker')
            .attr('id', id)
            .attr('viewBox', '0 -5 10 10')
            .attr('refX', id.includes('start') ? 2 : 8)
            .attr('refY', 0)
            .attr('markerWidth', 6)
            .attr('markerHeight', 6)
            .attr('orient', 'auto')
            .append('path')
            .attr('d', path)
            .attr('fill', fill);
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

        // Define comprehensive arrow markers and patterns for Draw.io-style connections
        const defs = this.svg.append('defs');
        
        // Standard arrow markers
        this.createArrowMarker(defs, 'arrow', 'M0,-5L10,0L0,5', '#495057');
        this.createArrowMarker(defs, 'arrow-start', 'M10,-5L0,0L10,5', '#495057');
        
        // Diamond markers
        this.createArrowMarker(defs, 'diamond', 'M0,0L5,-5L10,0L5,5Z', '#495057');
        this.createArrowMarker(defs, 'diamond-start', 'M10,0L5,-5L0,0L5,5Z', '#495057');
        
        // Circle markers
        defs.append('marker')
            .attr('id', 'circle')
            .attr('viewBox', '0 -5 10 10')
            .attr('refX', 5)
            .attr('refY', 0)
            .attr('markerWidth', 8)
            .attr('markerHeight', 8)
            .attr('orient', 'auto')
            .append('circle')
            .attr('cx', 5)
            .attr('cy', 0)
            .attr('r', 3)
            .attr('fill', '#495057');

        defs.append('marker')
            .attr('id', 'circle-start')
            .attr('viewBox', '0 -5 10 10')
            .attr('refX', 5)
            .attr('refY', 0)
            .attr('markerWidth', 8)
            .attr('markerHeight', 8)
            .attr('orient', 'auto')
            .append('circle')
            .attr('cx', 5)
            .attr('cy', 0)
            .attr('r', 3)
            .attr('fill', '#495057');

        // Square markers
        this.createArrowMarker(defs, 'square', 'M2,2L8,2L8,8L2,8Z', '#495057');
        this.createArrowMarker(defs, 'square-start', 'M2,2L8,2L8,8L2,8Z', '#495057');

        // Open arrow markers
        defs.append('marker')
            .attr('id', 'open')
            .attr('viewBox', '0 -5 10 10')
            .attr('refX', 8)
            .attr('refY', 0)
            .attr('markerWidth', 6)
            .attr('markerHeight', 6)
            .attr('orient', 'auto')
            .append('path')
            .attr('d', 'M0,-5L10,0L0,5')
            .attr('fill', 'none')
            .attr('stroke', '#495057')
            .attr('stroke-width', 2);

        defs.append('marker')
            .attr('id', 'open-start')
            .attr('viewBox', '0 -5 10 10')
            .attr('refX', 2)
            .attr('refY', 0)
            .attr('markerWidth', 6)
            .attr('markerHeight', 6)
            .attr('orient', 'auto')
            .append('path')
            .attr('d', 'M10,-5L0,0L10,5')
            .attr('fill', 'none')
            .attr('stroke', '#495057')
            .attr('stroke-width', 2);

        // Filled arrow markers (thicker)
        this.createArrowMarker(defs, 'filled', 'M0,-8L15,0L0,8Z', '#495057');
        this.createArrowMarker(defs, 'filled-start', 'M15,-8L0,0L15,8Z', '#495057');

        // Double arrow markers
        defs.append('marker')
            .attr('id', 'double')
            .attr('viewBox', '0 -8 20 16')
            .attr('refX', 18)
            .attr('refY', 0)
            .attr('markerWidth', 10)
            .attr('markerHeight', 10)
            .attr('orient', 'auto')
            .selectAll('path')
            .data(['M0,-5L8,0L0,5', 'M8,-5L16,0L8,5'])
            .enter()
            .append('path')
            .attr('d', d => d)
            .attr('fill', '#495057');

        // Wave filter for wave pattern
        const waveFilter = defs.append('filter')
            .attr('id', 'wave-filter');
        
        waveFilter.append('feTurbulence')
            .attr('baseFrequency', '0.02')
            .attr('numOctaves', '3')
            .attr('result', 'noise');
            
        waveFilter.append('feDisplacementMap')
            .attr('in', 'SourceGraphic')
            .attr('in2', 'noise')
            .attr('scale', '2');

        // Patterns for advanced styling
        const wavePattern = defs.append('pattern')
            .attr('id', 'wave-pattern')
            .attr('patternUnits', 'userSpaceOnUse')
            .attr('width', 20)
            .attr('height', 10);
            
        wavePattern.append('path')
            .attr('d', 'M0,5 Q5,0 10,5 Q15,10 20,5')
            .attr('stroke', '#6c757d')
            .attr('stroke-width', 1)
            .attr('fill', 'none');

        // Create main group for zoom/pan
        this.mainGroup = this.svg.append('g').attr('class', 'main-group');
        
        // Create separate groups for relationships and elements
        this.relationshipsGroup = this.mainGroup.append('g').attr('class', 'relationships-group');
        this.elementsGroup = this.mainGroup.append('g').attr('class', 'elements-group');
        
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

        // Relationship management event listeners
        d3.select('#addRelationshipBtn').on('click', () => {
            this.showRelationshipModal();
        });

        d3.select('#connectionModeBtn').on('click', () => {
            this.toggleConnectionMode();
        });

        d3.select('#saveRelationshipBtn').on('click', () => {
            this.saveRelationshipFromModal();
        });

        d3.select('#deleteRelationship').on('click', () => {
            this.deleteSelectedRelationship();
        });

        d3.select('#exitConnectionMode').on('click', () => {
            this.exitConnectionMode();
        });

        // Relationship form listeners
        d3.select('#relationshipForm').on('input', () => {
            this.updateSelectedRelationshipProperties();
        });

        d3.select('#modalRelationshipWidth').on('input', (event) => {
            d3.select('#widthValue').text(`${event.target.value}px`);
            this.updateConnectionPreview();
        });

        d3.select('#modalRelationshipOpacity').on('input', (event) => {
            d3.select('#opacityValue').text(`${Math.round(event.target.value * 100)}%`);
            this.updateConnectionPreview();
        });

        // Add event listeners for all connection style controls
        d3.selectAll('#modalConnectionType, #modalRelationshipLineStyle, #modalRelationshipPattern, #modalRelationshipStartArrow, #modalRelationshipArrowStyle, #modalSourceAnchor, #modalTargetAnchor, #modalRelationshipColor, #modalRelationshipShadow')
            .on('change', () => {
                this.updateConnectionPreview();
            });

        // Context menu for relationships
        d3.select('#editRelationshipContext').on('click', () => {
            this.editRelationshipFromContext();
        });

        d3.select('#duplicateRelationshipContext').on('click', () => {
            this.duplicateRelationshipFromContext();
        });

        d3.select('#deleteRelationshipContext').on('click', () => {
            this.deleteRelationshipFromContext();
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
        const group = this.elementsGroup
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
        // Click to select or handle connection mode
        group.on('click', (event) => {
            event.stopPropagation();
            if (this.connectionMode) {
                this.handleElementClickForConnection(element.id);
            } else {
                this.selectElement(element.id);
            }
        });

        // Double-click to edit name
        group.on('dblclick', (event) => {
            event.stopPropagation();
            if (!this.connectionMode) {
                this.editElementName(element.id);
            }
        });

        // Context menu
        group.on('contextmenu', (event) => {
            event.preventDefault();
            if (!this.connectionMode) {
                this.showContextMenu(event, element.id);
            }
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
        this.selectedRelationship = null;
        this.hideElementProperties();
        this.hideRelationshipProperties();
    }

    hideRelationshipProperties() {
        document.getElementById('relationshipProperties').style.display = 'none';
        document.getElementById('noSelection').style.display = 'block';
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

            // Update relationship list
            this.updateRelationshipsList();

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
        if (event.key === 'Delete') {
            if (this.selectedElement) {
                this.deleteSelectedElement();
            } else if (this.selectedRelationship) {
                this.deleteSelectedRelationship();
            }
        } else if (event.key === 'Escape') {
            if (this.connectionMode) {
                this.exitConnectionMode();
            } else {
                this.clearSelection();
            }
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

    // === RELATIONSHIP MANAGEMENT METHODS ===

    showRelationshipModal(relationshipId = null) {
        this.populateElementDropdowns();
        
        if (relationshipId) {
            // Editing existing relationship
            this.isEditingRelationship = true;
            this.currentRelationshipId = relationshipId;
            const relationship = this.relationships.get(relationshipId);
            
            if (relationship) {
                document.getElementById('relationshipModalTitle').textContent = 'Edit Relationship';
                document.getElementById('modalSourceElement').value = relationship.sourceId;
                document.getElementById('modalTargetElement').value = relationship.targetId;
                document.getElementById('modalRelationshipDescription').value = relationship.description || '';
                document.getElementById('modalRelationshipTechnology').value = relationship.technology || '';
                
                // Connection and styling options
                document.getElementById('modalConnectionType').value = relationship.style?.connectionType || 'curved';
                document.getElementById('modalRelationshipColor').value = relationship.style?.color || '#6c757d';
                document.getElementById('modalRelationshipWidth').value = relationship.style?.width || 2;
                document.getElementById('modalRelationshipLineStyle').value = relationship.style?.lineStyle || 'solid';
                document.getElementById('modalRelationshipPattern').value = relationship.style?.pattern || 'none';
                document.getElementById('modalRelationshipStartArrow').value = relationship.style?.startArrowStyle || 'none';
                document.getElementById('modalRelationshipArrowStyle').value = relationship.style?.arrowStyle || 'arrow';
                document.getElementById('modalRelationshipOpacity').value = relationship.style?.opacity || 1.0;
                document.getElementById('modalRelationshipShadow').value = relationship.style?.shadow || 'none';
                
                // Connection points
                document.getElementById('modalSourceAnchor').value = relationship.style?.connectionPoints?.sourceAnchor || 'auto';
                document.getElementById('modalTargetAnchor').value = relationship.style?.connectionPoints?.targetAnchor || 'auto';
                
                // Update display values
                document.getElementById('widthValue').textContent = `${relationship.style?.width || 2}px`;
                document.getElementById('opacityValue').textContent = `${Math.round((relationship.style?.opacity || 1.0) * 100)}%`;
            }
        } else {
            // Creating new relationship
            this.isEditingRelationship = false;
            this.currentRelationshipId = null;
            document.getElementById('relationshipModalTitle').textContent = 'Add Relationship';
            document.getElementById('relationshipModalForm').reset();
            
            // Set default values
            document.getElementById('modalConnectionType').value = 'curved';
            document.getElementById('modalRelationshipColor').value = '#6c757d';
            document.getElementById('modalRelationshipWidth').value = 2;
            document.getElementById('modalRelationshipLineStyle').value = 'solid';
            document.getElementById('modalRelationshipPattern').value = 'none';
            document.getElementById('modalRelationshipStartArrow').value = 'none';
            document.getElementById('modalRelationshipArrowStyle').value = 'arrow';
            document.getElementById('modalRelationshipOpacity').value = 1.0;
            document.getElementById('modalRelationshipShadow').value = 'none';
            document.getElementById('modalSourceAnchor').value = 'auto';
            document.getElementById('modalTargetAnchor').value = 'auto';
            
            document.getElementById('widthValue').textContent = '2px';
            document.getElementById('opacityValue').textContent = '100%';
        }

        const modal = new bootstrap.Modal(document.getElementById('relationshipModal'));
        modal.show();
        
        // Initialize connection preview after modal is shown
        setTimeout(() => {
            this.updateConnectionPreview();
        }, 200);
    }

    populateElementDropdowns() {
        const sourceSelect = document.getElementById('modalSourceElement');
        const targetSelect = document.getElementById('modalTargetElement');
        
        // Clear existing options except first
        sourceSelect.innerHTML = '<option value="">Select source element...</option>';
        targetSelect.innerHTML = '<option value="">Select target element...</option>';
        
        // Add elements to dropdowns
        this.elements.forEach((element, id) => {
            const sourceOption = document.createElement('option');
            sourceOption.value = id;
            sourceOption.textContent = element.name;
            sourceSelect.appendChild(sourceOption);
            
            const targetOption = document.createElement('option');
            targetOption.value = id;
            targetOption.textContent = element.name;
            targetSelect.appendChild(targetOption);
        });
    }

    saveRelationshipFromModal() {
        const form = document.getElementById('relationshipModalForm');
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        const sourceId = document.getElementById('modalSourceElement').value;
        const targetId = document.getElementById('modalTargetElement').value;
        const description = document.getElementById('modalRelationshipDescription').value;
        const technology = document.getElementById('modalRelationshipTechnology').value;
        
        // Advanced styling options
        const connectionType = document.getElementById('modalConnectionType').value;
        const color = document.getElementById('modalRelationshipColor').value;
        const width = document.getElementById('modalRelationshipWidth').value;
        const lineStyle = document.getElementById('modalRelationshipLineStyle').value;
        const pattern = document.getElementById('modalRelationshipPattern').value;
        const startArrow = document.getElementById('modalRelationshipStartArrow').value;
        const endArrow = document.getElementById('modalRelationshipArrowStyle').value;
        const opacity = document.getElementById('modalRelationshipOpacity').value;
        const shadow = document.getElementById('modalRelationshipShadow').value;
        const sourceAnchor = document.getElementById('modalSourceAnchor').value;
        const targetAnchor = document.getElementById('modalTargetAnchor').value;

        if (sourceId === targetId) {
            alert('Source and target elements cannot be the same');
            return;
        }

        const relationshipData = {
            id: this.isEditingRelationship ? this.currentRelationshipId : this.generateId(),
            sourceId: sourceId,
            targetId: targetId,
            description: description,
            technology: technology,
            style: {
                connectionType: connectionType,
                color: color,
                width: parseFloat(width),
                lineStyle: lineStyle,
                pattern: pattern,
                startArrowStyle: startArrow,
                arrowStyle: endArrow,
                opacity: parseFloat(opacity),
                shadow: shadow,
                connectionPoints: {
                    sourceAnchor: sourceAnchor,
                    targetAnchor: targetAnchor
                }
            }
        };

        if (this.isEditingRelationship) {
            this.updateRelationship(relationshipData);
        } else {
            this.addRelationship(relationshipData);
        }

        const modal = bootstrap.Modal.getInstance(document.getElementById('relationshipModal'));
        modal.hide();
    }

    addRelationship(relationshipData) {
        this.relationships.set(relationshipData.id, relationshipData);
        this.renderRelationship(relationshipData);
        this.updateRelationshipsList();

        // Broadcast to collaborators
        if (this.isCollaborationEnabled) {
            this.connection.invoke('AddRelationship', this.diagramId, relationshipData);
        }
    }

    updateRelationship(relationshipData) {
        this.relationships.set(relationshipData.id, relationshipData);
        
        // Remove old visual representation
        d3.select(`[data-id="${relationshipData.id}"]`).remove();
        
        // Render updated relationship
        this.renderRelationship(relationshipData);
        this.updateRelationshipsList();

        // Broadcast to collaborators
        if (this.isCollaborationEnabled) {
            this.connection.invoke('UpdateRelationship', this.diagramId, relationshipData);
        }
    }

    deleteRelationship(relationshipId) {
        if (this.relationships.has(relationshipId)) {
            this.relationships.delete(relationshipId);
            d3.select(`[data-id="${relationshipId}"]`).remove();
            this.updateRelationshipsList();
            
            // Clear selection if this relationship was selected
            if (this.selectedRelationship === relationshipId) {
                this.clearRelationshipSelection();
            }

            // Broadcast to collaborators
            if (this.isCollaborationEnabled) {
                this.connection.invoke('RemoveRelationship', this.diagramId, relationshipId);
            }
        }
    }

    deleteSelectedRelationship() {
        if (this.selectedRelationship) {
            if (confirm('Are you sure you want to delete this relationship?')) {
                this.deleteRelationship(this.selectedRelationship);
            }
        }
    }

    toggleConnectionMode() {
        this.connectionMode = !this.connectionMode;
        const btn = document.getElementById('connectionModeBtn');
        
        if (this.connectionMode) {
            btn.classList.add('active');
            btn.classList.remove('btn-outline-secondary');
            btn.classList.add('btn-secondary');
            this.svg.style('cursor', 'crosshair');
            
            // Show connection mode modal
            const modal = new bootstrap.Modal(document.getElementById('connectionModeModal'));
            modal.show();
        } else {
            this.exitConnectionMode();
        }
    }

    exitConnectionMode() {
        this.connectionMode = false;
        this.connectionStart = null;
        const btn = document.getElementById('connectionModeBtn');
        
        btn.classList.remove('active');
        btn.classList.add('btn-outline-secondary');
        btn.classList.remove('btn-secondary');
        this.svg.style('cursor', 'default');
        
        // Hide connection mode modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('connectionModeModal'));
        if (modal) {
            modal.hide();
        }
    }

    handleElementClickForConnection(elementId) {
        if (!this.connectionMode) return;

        if (!this.connectionStart) {
            // First click - set source
            this.connectionStart = elementId;
            d3.select(`[data-id="${elementId}"]`).classed('connection-source', true);
        } else if (this.connectionStart !== elementId) {
            // Second click - create connection
            d3.select(`[data-id="${this.connectionStart}"]`).classed('connection-source', false);
            
            // Pre-populate modal with selected elements
            const sourceElement = this.elements.get(this.connectionStart);
            const targetElement = this.elements.get(elementId);
            
            this.showRelationshipModal();
            
            // Set the dropdowns after modal is shown
            setTimeout(() => {
                document.getElementById('modalSourceElement').value = this.connectionStart;
                document.getElementById('modalTargetElement').value = elementId;
                document.getElementById('modalRelationshipDescription').value = `${sourceElement.name} uses ${targetElement.name}`;
            }, 100);
            
            this.connectionStart = null;
        }
    }

    selectRelationship(relationshipId) {
        this.clearSelection();  // Clear element selection
        this.selectedRelationship = relationshipId;
        
        // Highlight selected relationship
        d3.selectAll('.c4-relationship').classed('selected', false);
        d3.select(`[data-id="${relationshipId}"]`).classed('selected', true);
        
        // Show relationship properties
        this.showRelationshipProperties(relationshipId);
    }

    clearRelationshipSelection() {
        this.selectedRelationship = null;
        d3.selectAll('.c4-relationship').classed('selected', false);
        document.getElementById('relationshipProperties').style.display = 'none';
        document.getElementById('noSelection').style.display = 'block';
    }

    showRelationshipProperties(relationshipId) {
        const relationship = this.relationships.get(relationshipId);
        if (!relationship) return;

        document.getElementById('noSelection').style.display = 'none';
        document.getElementById('elementProperties').style.display = 'none';
        document.getElementById('relationshipProperties').style.display = 'block';

        // Populate all relationship form fields
        document.getElementById('relationshipDescription').value = relationship.description || '';
        document.getElementById('relationshipTechnology').value = relationship.technology || '';
        document.getElementById('relationshipType').value = relationship.type || 'uses';
        document.getElementById('relationshipConnectionType').value = relationship.style?.connectionType || 'curved';
        document.getElementById('relationshipColor').value = relationship.style?.color || '#6c757d';
        document.getElementById('relationshipWidth').value = relationship.style?.width || 2;
        document.getElementById('relationshipLineStyle').value = relationship.style?.lineStyle || 'solid';
        document.getElementById('relationshipPattern').value = relationship.style?.pattern || 'none';
        document.getElementById('relationshipStartArrow').value = relationship.style?.startArrowStyle || 'none';
        document.getElementById('relationshipEndArrow').value = relationship.style?.arrowStyle || 'arrow';
        document.getElementById('relationshipSourceAnchor').value = relationship.style?.connectionPoints?.sourceAnchor || 'auto';
        document.getElementById('relationshipTargetAnchor').value = relationship.style?.connectionPoints?.targetAnchor || 'auto';
    }

    updateSelectedRelationshipProperties() {
        if (!this.selectedRelationship) return;

        const relationship = this.relationships.get(this.selectedRelationship);
        if (!relationship) return;

        const updatedRelationship = {
            ...relationship,
            description: document.getElementById('relationshipDescription').value,
            technology: document.getElementById('relationshipTechnology').value,
            type: document.getElementById('relationshipType').value,
            style: {
                ...relationship.style,
                connectionType: document.getElementById('relationshipConnectionType').value,
                color: document.getElementById('relationshipColor').value,
                width: parseFloat(document.getElementById('relationshipWidth').value),
                lineStyle: document.getElementById('relationshipLineStyle').value,
                pattern: document.getElementById('relationshipPattern').value,
                startArrowStyle: document.getElementById('relationshipStartArrow').value,
                arrowStyle: document.getElementById('relationshipEndArrow').value,
                connectionPoints: {
                    sourceAnchor: document.getElementById('relationshipSourceAnchor').value,
                    targetAnchor: document.getElementById('relationshipTargetAnchor').value
                }
            }
        };

        this.updateRelationship(updatedRelationship);
    }

    // Update connection preview in modal
    updateConnectionPreview() {
        const preview = d3.select('#connectionPreview');
        if (preview.empty()) return;

        // Clear previous preview
        preview.selectAll('*').remove();

        // Get current form values
        const connectionType = document.getElementById('modalConnectionType')?.value || 'curved';
        const color = document.getElementById('modalRelationshipColor')?.value || '#6c757d';
        const width = document.getElementById('modalRelationshipWidth')?.value || 2;
        const lineStyle = document.getElementById('modalRelationshipLineStyle')?.value || 'solid';
        const pattern = document.getElementById('modalRelationshipPattern')?.value || 'none';
        const startArrow = document.getElementById('modalRelationshipStartArrow')?.value || 'none';
        const endArrow = document.getElementById('modalRelationshipArrowStyle')?.value || 'arrow';
        const opacity = document.getElementById('modalRelationshipOpacity')?.value || 1;
        const shadow = document.getElementById('modalRelationshipShadow')?.value || 'none';

        // Create preview elements
        const previewRect = preview.node().getBoundingClientRect();
        const fromPoint = { x: 20, y: previewRect.height / 2 };
        const toPoint = { x: previewRect.width - 20, y: previewRect.height / 2 };

        const style = {
            connectionType,
            color,
            width: parseFloat(width),
            lineStyle,
            pattern,
            startArrowStyle: startArrow,
            arrowStyle: endArrow,
            opacity: parseFloat(opacity),
            shadow
        };

        // Create preview path
        const pathData = this.createConnectionPath(fromPoint, toPoint, style);
        
        const path = preview.append('path')
            .attr('d', pathData)
            .attr('fill', 'none')
            .attr('stroke', color)
            .attr('stroke-width', width)
            .attr('opacity', opacity);

        // Apply line style
        this.applyLineStyle(path, style);

        // Apply visual effects
        this.applyVisualEffects(path, style);

        // Add arrows if specified
        if (startArrow !== 'none') {
            path.attr('marker-start', `url(#${startArrow}-start)`);
        }
        if (endArrow !== 'none') {
            path.attr('marker-end', `url(#${endArrow})`);
        }

        // Add pattern if specified
        if (pattern !== 'none') {
            this.addPatternOverlay(preview, pathData, style);
        }

        // Add sample labels
        const midPoint = { x: previewRect.width / 2, y: previewRect.height / 2 };
        
        preview.append('text')
            .attr('x', midPoint.x)
            .attr('y', midPoint.y - 5)
            .attr('text-anchor', 'middle')
            .attr('fill', color)
            .style('font-size', '10px')
            .style('font-weight', 'bold')
            .text('Sample Connection');

        preview.append('text')
            .attr('x', midPoint.x)
            .attr('y', midPoint.y + 12)
            .attr('text-anchor', 'middle')
            .attr('fill', '#666')
            .style('font-size', '8px')
            .style('font-style', 'italic')
            .text('[Technology]');
    }

    updateRelationshipsList() {
        const container = document.getElementById('relationshipList');
        if (!container) return;
        
        // Clear existing content first
        container.innerHTML = '';
        
        if (this.relationships.size === 0) {
            container.innerHTML = '<div class="text-muted small text-center py-2" id="noRelationships">No relationships defined</div>';
            return;
        }

        this.relationships.forEach((relationship, id) => {
            const sourceElement = this.elements.get(relationship.sourceId);
            const targetElement = this.elements.get(relationship.targetId);
            
            if (!sourceElement || !targetElement) {
                console.warn(`Relationship ${id} references missing elements: ${relationship.sourceId} or ${relationship.targetId}`);
                return;
            }

            const item = document.createElement('div');
            item.className = 'list-group-item list-group-item-action py-2';
            item.setAttribute('data-relationship-id', id);
            
            item.innerHTML = `
                <div class="d-flex justify-content-between align-items-center">
                    <div class="flex-grow-1">
                        <div class="small fw-medium">${sourceElement.name} → ${targetElement.name}</div>
                        <div class="small text-muted">${relationship.description || 'No description'}</div>
                    </div>
                    <div class="btn-group btn-group-sm" role="group">
                        <button type="button" class="btn btn-outline-primary btn-sm edit-relationship" title="Edit">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button type="button" class="btn btn-outline-danger btn-sm delete-relationship" title="Delete">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
            `;

            // Add event listeners with safety checks
            item.addEventListener('click', (e) => {
                if (!e.target.closest('.btn-group')) {
                    this.selectRelationship(id);
                }
            });

            const editBtn = item.querySelector('.edit-relationship');
            const deleteBtn = item.querySelector('.delete-relationship');
            
            if (editBtn) {
                editBtn.addEventListener('click', (e) => {
                    e.stopPropagation();
                    this.showRelationshipModal(id);
                });
            }

            if (deleteBtn) {
                deleteBtn.addEventListener('click', (e) => {
                    e.stopPropagation();
                    if (confirm('Are you sure you want to delete this relationship?')) {
                        this.deleteRelationship(id);
                    }
                });
            }

            container.appendChild(item);
        });
    }

    // Context menu methods
    editRelationshipFromContext() {
        if (this.selectedRelationship) {
            this.showRelationshipModal(this.selectedRelationship);
        }
    }

    duplicateRelationshipFromContext() {
        if (this.selectedRelationship) {
            const original = this.relationships.get(this.selectedRelationship);
            if (original) {
                const duplicate = {
                    ...original,
                    id: this.generateId()
                };
                this.addRelationship(duplicate);
            }
        }
    }

    deleteRelationshipFromContext() {
        if (this.selectedRelationship) {
            if (confirm('Are you sure you want to delete this relationship?')) {
                this.deleteRelationship(this.selectedRelationship);
            }
        }
    }

    // Enhanced renderRelationship method with Draw.io-style connection types
    renderRelationship(relationship) {
        const fromElement = this.elements.get(relationship.sourceId);
        const toElement = this.elements.get(relationship.targetId);

        if (!fromElement || !toElement) return;

        // Calculate connection points based on anchor settings
        const connectionPoints = this.calculateConnectionPoints(fromElement, toElement, relationship.style);
        const fromPoint = connectionPoints.from;
        const toPoint = connectionPoints.to;

        const group = this.relationshipsGroup
            .append('g')
            .attr('class', 'c4-relationship')
            .attr('data-id', relationship.id);

        // Create path based on connection type
        const pathData = this.createConnectionPath(fromPoint, toPoint, relationship.style);
        
        // Create the main relationship path
        const path = group.append('path')
            .attr('d', pathData)
            .attr('fill', 'none')
            .attr('stroke', relationship.style?.color || '#6c757d')
            .attr('stroke-width', relationship.style?.width || 2)
            .attr('opacity', relationship.style?.opacity || 1.0)
            .attr('cursor', 'pointer');

        // Apply line styles
        this.applyLineStyle(path, relationship.style);

        // Apply visual effects
        this.applyVisualEffects(path, relationship.style);

        // Add arrows
        this.addArrowMarkers(path, relationship.style);

        // Add pattern overlay
        if (relationship.style?.pattern && relationship.style.pattern !== 'none') {
            this.addPatternOverlay(group, pathData, relationship.style);
        }

        // Add description label if exists
        if (relationship.description) {
            this.addRelationshipLabel(group, pathData, relationship.description, relationship.style);
        }

        // Add technology label if exists
        if (relationship.technology) {
            this.addTechnologyLabel(group, pathData, relationship.technology, relationship.style);
        }

        // Setup interactions
        this.setupRelationshipInteractions(group, relationship);
    }

    // Calculate optimal connection points based on anchor settings
    calculateConnectionPoints(fromElement, toElement, style) {
        const sourceAnchor = style?.connectionPoints?.sourceAnchor || 'auto';
        const targetAnchor = style?.connectionPoints?.targetAnchor || 'auto';

        const fromRect = {
            x: fromElement.x,
            y: fromElement.y,
            width: fromElement.width,
            height: fromElement.height
        };

        const toRect = {
            x: toElement.x,
            y: toElement.y,
            width: toElement.width,
            height: toElement.height
        };

        let fromPoint, toPoint;

        if (sourceAnchor === 'auto') {
            fromPoint = this.getOptimalConnectionPoint(fromRect, toRect);
        } else {
            fromPoint = this.getAnchorPoint(fromRect, sourceAnchor);
        }

        if (targetAnchor === 'auto') {
            toPoint = this.getOptimalConnectionPoint(toRect, fromRect);
        } else {
            toPoint = this.getAnchorPoint(toRect, targetAnchor);
        }

        // Apply offsets if specified
        if (style?.connectionPoints?.sourceOffset) {
            fromPoint.x += style.connectionPoints.sourceOffset.x || 0;
            fromPoint.y += style.connectionPoints.sourceOffset.y || 0;
        }

        if (style?.connectionPoints?.targetOffset) {
            toPoint.x += style.connectionPoints.targetOffset.x || 0;
            toPoint.y += style.connectionPoints.targetOffset.y || 0;
        }

        return { from: fromPoint, to: toPoint };
    }

    // Get connection point for specific anchor
    getAnchorPoint(rect, anchor) {
        switch (anchor) {
            case 'top':
                return { x: rect.x + rect.width / 2, y: rect.y };
            case 'right':
                return { x: rect.x + rect.width, y: rect.y + rect.height / 2 };
            case 'bottom':
                return { x: rect.x + rect.width / 2, y: rect.y + rect.height };
            case 'left':
                return { x: rect.x, y: rect.y + rect.height / 2 };
            case 'center':
                return { x: rect.x + rect.width / 2, y: rect.y + rect.height / 2 };
            default:
                return { x: rect.x + rect.width / 2, y: rect.y + rect.height / 2 };
        }
    }

    // Get optimal connection point (closest edge point to target)
    getOptimalConnectionPoint(fromRect, toRect) {
        const fromCenter = {
            x: fromRect.x + fromRect.width / 2,
            y: fromRect.y + fromRect.height / 2
        };

        const toCenter = {
            x: toRect.x + toRect.width / 2,
            y: toRect.y + toRect.height / 2
        };

        // Calculate direction vector
        const dx = toCenter.x - fromCenter.x;
        const dy = toCenter.y - fromCenter.y;

        // Determine which side of the rectangle to use
        const absRatio = Math.abs(dy / dx);
        const heightRatio = fromRect.height / fromRect.width;

        let connectionPoint;

        if (absRatio > heightRatio) {
            // Connect from top or bottom
            if (dy > 0) {
                connectionPoint = { x: fromCenter.x, y: fromRect.y + fromRect.height };
            } else {
                connectionPoint = { x: fromCenter.x, y: fromRect.y };
            }
        } else {
            // Connect from left or right
            if (dx > 0) {
                connectionPoint = { x: fromRect.x + fromRect.width, y: fromCenter.y };
            } else {
                connectionPoint = { x: fromRect.x, y: fromCenter.y };
            }
        }

        return connectionPoint;
    }

    // Create connection path based on connection type
    createConnectionPath(fromPoint, toPoint, style) {
        const connectionType = style?.connectionType || 'curved';

        switch (connectionType) {
            case 'straight':
                return this.createStraightPath(fromPoint, toPoint);
            case 'curved':
                return this.createCurvedPath(fromPoint, toPoint);
            case 'orthogonal':
                return this.createOrthogonalPath(fromPoint, toPoint);
            case 'bezier':
                return this.createBezierPath(fromPoint, toPoint, style?.controlPoints);
            case 'stepped':
                return this.createSteppedPath(fromPoint, toPoint);
            case 'arc':
                return this.createArcPath(fromPoint, toPoint);
            default:
                return this.createCurvedPath(fromPoint, toPoint);
        }
    }

    // Straight line connection
    createStraightPath(from, to) {
        return `M ${from.x} ${from.y} L ${to.x} ${to.y}`;
    }

    // Curved connection (default)
    createCurvedPath(from, to) {
        const dx = to.x - from.x;
        const dy = to.y - from.y;
        const distance = Math.sqrt(dx * dx + dy * dy);
        
        // Control point for curve
        const controlOffset = Math.min(distance * 0.3, 50);
        const midX = (from.x + to.x) / 2;
        const midY = (from.y + to.y) / 2;
        
        // Perpendicular offset for curve
        const perpX = -dy / distance * controlOffset;
        const perpY = dx / distance * controlOffset;
        
        return `M ${from.x} ${from.y} Q ${midX + perpX} ${midY + perpY} ${to.x} ${to.y}`;
    }

    // Orthogonal connection (right angles)
    createOrthogonalPath(from, to) {
        const midX = (from.x + to.x) / 2;
        const midY = (from.y + to.y) / 2;

        // Determine if we should route horizontally or vertically first
        const dx = Math.abs(to.x - from.x);
        const dy = Math.abs(to.y - from.y);

        if (dx > dy) {
            // Route horizontally first
            return `M ${from.x} ${from.y} L ${midX} ${from.y} L ${midX} ${to.y} L ${to.x} ${to.y}`;
        } else {
            // Route vertically first
            return `M ${from.x} ${from.y} L ${from.x} ${midY} L ${to.x} ${midY} L ${to.x} ${to.y}`;
        }
    }

    // Smooth Bezier curve
    createBezierPath(from, to, controlPoints = null) {
        if (controlPoints && controlPoints.length >= 2) {
            // Use custom control points
            const cp1 = controlPoints[0].position;
            const cp2 = controlPoints[1].position;
            return `M ${from.x} ${from.y} C ${cp1.x} ${cp1.y} ${cp2.x} ${cp2.y} ${to.x} ${to.y}`;
        } else {
            // Auto-generate control points
            const dx = to.x - from.x;
            const dy = to.y - from.y;
            
            const cp1x = from.x + dx * 0.3;
            const cp1y = from.y;
            const cp2x = to.x - dx * 0.3;
            const cp2y = to.y;
            
            return `M ${from.x} ${from.y} C ${cp1x} ${cp1y} ${cp2x} ${cp2y} ${to.x} ${to.y}`;
        }
    }

    // Stepped connection
    createSteppedPath(from, to) {
        const steps = 3;
        const stepX = (to.x - from.x) / steps;
        const stepY = (to.y - from.y) / steps;
        const stepHeight = 20;

        let path = `M ${from.x} ${from.y}`;
        
        for (let i = 1; i <= steps; i++) {
            const x = from.x + stepX * i;
            const y = from.y + stepY * i;
            const offsetY = (i % 2 === 1) ? stepHeight : -stepHeight;
            
            if (i === 1) {
                path += ` L ${x} ${from.y + offsetY}`;
            }
            path += ` L ${x} ${y + offsetY}`;
            if (i === steps) {
                path += ` L ${to.x} ${to.y}`;
            }
        }
        
        return path;
    }

    // Arc connection
    createArcPath(from, to) {
        const dx = to.x - from.x;
        const dy = to.y - from.y;
        const distance = Math.sqrt(dx * dx + dy * dy);
        
        // Create an arc with radius proportional to distance
        const radius = distance * 0.3;
        const largeArcFlag = radius > distance / 2 ? 1 : 0;
        const sweepFlag = dy > 0 ? 1 : 0;
        
        return `M ${from.x} ${from.y} A ${radius} ${radius} 0 ${largeArcFlag} ${sweepFlag} ${to.x} ${to.y}`;
    }

    // Apply line styles (dashed, dotted, etc.)
    applyLineStyle(path, style) {
        const lineStyle = style?.lineStyle || 'solid';
        
        switch (lineStyle) {
            case 'dashed':
                path.attr('stroke-dasharray', '8,4');
                break;
            case 'dotted':
                path.attr('stroke-dasharray', '2,2');
                break;
            case 'dashdot':
                path.attr('stroke-dasharray', '8,4,2,4');
                break;
            case 'longdash':
                path.attr('stroke-dasharray', '12,6');
                break;
            case 'solid':
            default:
                path.attr('stroke-dasharray', 'none');
                break;
        }
    }

    // Apply visual effects (shadow, etc.)
    applyVisualEffects(path, style) {
        const shadow = style?.shadow || 'none';
        
        switch (shadow) {
            case 'light':
                path.style('filter', 'drop-shadow(1px 1px 2px rgba(0,0,0,0.2))');
                break;
            case 'medium':
                path.style('filter', 'drop-shadow(2px 2px 4px rgba(0,0,0,0.3))');
                break;
            case 'heavy':
                path.style('filter', 'drop-shadow(3px 3px 6px rgba(0,0,0,0.4))');
                break;
            case 'none':
            default:
                path.style('filter', 'none');
                break;
        }
    }

    // Add arrow markers
    addArrowMarkers(path, style) {
        const startArrow = style?.startArrowStyle || 'none';
        const endArrow = style?.arrowStyle || 'arrow';

        if (startArrow !== 'none') {
            path.attr('marker-start', `url(#${startArrow}-start)`);
        }

        if (endArrow !== 'none') {
            path.attr('marker-end', `url(#${endArrow})`);
        }
    }

    // Add pattern overlay (wave, zigzag)
    addPatternOverlay(group, pathData, style) {
        const pattern = style?.pattern || 'none';
        
        if (pattern === 'wave') {
            // Create wave pattern overlay
            const wavePath = group.append('path')
                .attr('d', pathData)
                .attr('fill', 'none')
                .attr('stroke', style?.color || '#6c757d')
                .attr('stroke-width', 1)
                .attr('stroke-dasharray', 'none')
                .attr('opacity', 0.5)
                .style('filter', 'url(#wave-filter)');
        } else if (pattern === 'zigzag') {
            // Create zigzag pattern overlay
            const zigzagPath = group.append('path')
                .attr('d', pathData)
                .attr('fill', 'none')
                .attr('stroke', style?.color || '#6c757d')
                .attr('stroke-width', 1)
                .attr('stroke-dasharray', '3,3')
                .attr('opacity', 0.7);
        }
    }

    // Add relationship label with smart positioning
    addRelationshipLabel(group, pathData, description, style) {
        const pathElement = document.createElementNS('http://www.w3.org/2000/svg', 'path');
        pathElement.setAttribute('d', pathData);
        
        const pathLength = pathElement.getTotalLength();
        const midPoint = pathElement.getPointAtLength(pathLength / 2);
        
        const label = group.append('text')
            .attr('class', 'c4-relationship-label')
            .attr('x', midPoint.x)
            .attr('y', midPoint.y - 5)
            .attr('text-anchor', 'middle')
            .attr('fill', style?.color || '#6c757d')
            .style('font-size', '12px')
            .style('font-weight', 'bold')
            .text(description);

        // Add background for better readability
        const bbox = label.node().getBBox();
        group.insert('rect', '.c4-relationship-label')
            .attr('x', bbox.x - 4)
            .attr('y', bbox.y - 2)
            .attr('width', bbox.width + 8)
            .attr('height', bbox.height + 4)
            .attr('fill', 'white')
            .attr('fill-opacity', 0.9)
            .attr('rx', 3);
    }

    // Add technology label
    addTechnologyLabel(group, pathData, technology, style) {
        const pathElement = document.createElementNS('http://www.w3.org/2000/svg', 'path');
        pathElement.setAttribute('d', pathData);
        
        const pathLength = pathElement.getTotalLength();
        const midPoint = pathElement.getPointAtLength(pathLength / 2);

        group.append('text')
            .attr('class', 'c4-relationship-tech')
            .attr('x', midPoint.x)
            .attr('y', midPoint.y + 15)
            .attr('text-anchor', 'middle')
            .attr('fill', '#666')
            .style('font-size', '10px')
            .style('font-style', 'italic')
            .text(`[${technology}]`);
    }

    // Setup relationship interactions
    setupRelationshipInteractions(group, relationship) {
        group.on('click', (event) => {
            event.stopPropagation();
            this.selectRelationship(relationship.id);
        });

        group.on('contextmenu', (event) => {
            event.preventDefault();
            this.showRelationshipContextMenu(event, relationship.id);
        });

        group.on('mouseover', () => {
            group.classed('relationship-hover', true);
        });

        group.on('mouseout', () => {
            group.classed('relationship-hover', false);
        });
    }

    showRelationshipContextMenu(event, relationshipId) {
        this.selectedRelationship = relationshipId;
        const menu = document.getElementById('relationshipContextMenu');
        
        menu.style.display = 'block';
        menu.style.left = `${event.pageX}px`;
        menu.style.top = `${event.pageY}px`;
        
        // Hide menu when clicking elsewhere
        setTimeout(() => {
            document.addEventListener('click', () => {
                menu.style.display = 'none';
            }, { once: true });
        }, 100);
    }

    // Show relationship modal for adding/editing relationships
    showRelationshipModal(relationshipId = null) {
        const modal = document.getElementById('relationshipModal');
        const modalTitle = document.getElementById('relationshipModalTitle');
        const modalBootstrap = new bootstrap.Modal(modal);
        
        this.isEditingRelationship = relationshipId !== null;
        this.currentRelationshipId = relationshipId;
        
        // Update modal title
        modalTitle.textContent = this.isEditingRelationship ? 'Edit Relationship' : 'Add Relationship';
        
        // Populate element dropdowns
        this.populateElementDropdowns();
        
        if (this.isEditingRelationship) {
            // Load existing relationship data
            this.loadRelationshipIntoModal(relationshipId);
        } else {
            // Reset form for new relationship
            this.resetRelationshipModal();
        }
        
        modalBootstrap.show();
        this.updateConnectionPreview();
    }

    // Populate source and target element dropdowns
    populateElementDropdowns() {
        const sourceSelect = document.getElementById('modalSourceElement');
        const targetSelect = document.getElementById('modalTargetElement');
        
        sourceSelect.innerHTML = '<option value="">Select source element...</option>';
        targetSelect.innerHTML = '<option value="">Select target element...</option>';
        
        this.elements.forEach((element, id) => {
            const option = document.createElement('option');
            option.value = id;
            option.textContent = `${element.name} (${element.type})`;
            
            sourceSelect.appendChild(option.cloneNode(true));
            targetSelect.appendChild(option);
        });
    }

    // Load relationship data into modal for editing
    loadRelationshipIntoModal(relationshipId) {
        const relationship = this.relationships.get(relationshipId);
        if (!relationship) return;
        
        document.getElementById('modalSourceElement').value = relationship.sourceId || '';
        document.getElementById('modalTargetElement').value = relationship.targetId || '';
        document.getElementById('modalRelationshipDescription').value = relationship.description || '';
        document.getElementById('modalRelationshipTechnology').value = relationship.technology || '';
        
        // Load style properties
        const style = relationship.style || {};
        document.getElementById('modalRelationshipColor').value = style.color || '#6c757d';
        document.getElementById('modalRelationshipWidth').value = style.width || 2;
        document.getElementById('modalConnectionType').value = style.connectionType || 'curved';
        document.getElementById('modalRelationshipLineStyle').value = style.lineStyle || 'solid';
        document.getElementById('modalRelationshipPattern').value = style.pattern || 'none';
        document.getElementById('modalRelationshipStartArrow').value = style.startArrowStyle || 'none';
        document.getElementById('modalRelationshipArrowStyle').value = style.arrowStyle || 'arrow';
        document.getElementById('modalSourceAnchor').value = style.connectionPoints?.sourceAnchor || 'auto';
        document.getElementById('modalTargetAnchor').value = style.connectionPoints?.targetAnchor || 'auto';
        document.getElementById('modalRelationshipOpacity').value = style.opacity || 1.0;
        document.getElementById('modalRelationshipShadow').value = style.shadow || 'none';
        
        // Update value displays
        document.getElementById('widthValue').textContent = `${style.width || 2}px`;
        document.getElementById('opacityValue').textContent = `${Math.round((style.opacity || 1.0) * 100)}%`;
    }

    // Reset relationship modal for new relationship
    resetRelationshipModal() {
        document.getElementById('relationshipModalForm').reset();
        
        // Set default values
        document.getElementById('modalRelationshipColor').value = '#6c757d';
        document.getElementById('modalRelationshipWidth').value = 2;
        document.getElementById('modalConnectionType').value = 'curved';
        document.getElementById('modalRelationshipLineStyle').value = 'solid';
        document.getElementById('modalRelationshipPattern').value = 'none';
        document.getElementById('modalRelationshipStartArrow').value = 'none';
        document.getElementById('modalRelationshipArrowStyle').value = 'arrow';
        document.getElementById('modalSourceAnchor').value = 'auto';
        document.getElementById('modalTargetAnchor').value = 'auto';
        document.getElementById('modalRelationshipOpacity').value = 1.0;
        document.getElementById('modalRelationshipShadow').value = 'none';
        
        // Update value displays
        document.getElementById('widthValue').textContent = '2px';
        document.getElementById('opacityValue').textContent = '100%';
    }

    // Save relationship from modal
    saveRelationshipFromModal() {
        const sourceId = document.getElementById('modalSourceElement').value;
        const targetId = document.getElementById('modalTargetElement').value;
        const description = document.getElementById('modalRelationshipDescription').value;
        const technology = document.getElementById('modalRelationshipTechnology').value;
        
        if (!sourceId || !targetId) {
            alert('Please select both source and target elements.');
            return;
        }
        
        if (!description) {
            alert('Please provide a description for the relationship.');
            return;
        }
        
        const relationshipData = {
            id: this.currentRelationshipId || this.generateId(),
            sourceId: sourceId,
            targetId: targetId,
            description: description,
            technology: technology || null,
            style: {
                color: document.getElementById('modalRelationshipColor').value,
                width: parseFloat(document.getElementById('modalRelationshipWidth').value),
                connectionType: document.getElementById('modalConnectionType').value,
                lineStyle: document.getElementById('modalRelationshipLineStyle').value,
                pattern: document.getElementById('modalRelationshipPattern').value,
                startArrowStyle: document.getElementById('modalRelationshipStartArrow').value,
                arrowStyle: document.getElementById('modalRelationshipArrowStyle').value,
                opacity: parseFloat(document.getElementById('modalRelationshipOpacity').value),
                shadow: document.getElementById('modalRelationshipShadow').value,
                connectionPoints: {
                    sourceAnchor: document.getElementById('modalSourceAnchor').value,
                    targetAnchor: document.getElementById('modalTargetAnchor').value
                }
            }
        };
        
        if (this.isEditingRelationship) {
            this.updateRelationship(relationshipData);
        } else {
            this.addRelationship(relationshipData);
        }
        
        // Close modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('relationshipModal'));
        modal.hide();
    }

    // Toggle connection mode
    toggleConnectionMode() {
        this.connectionMode = !this.connectionMode;
        const btn = document.getElementById('connectionModeBtn');
        
        if (this.connectionMode) {
            btn.classList.add('btn-primary');
            btn.classList.remove('btn-outline-secondary');
            this.showConnectionModeInstructions();
        } else {
            btn.classList.remove('btn-primary');
            btn.classList.add('btn-outline-secondary');
            this.connectionStart = null;
        }
    }

    // Show connection mode instructions
    showConnectionModeInstructions() {
        const modal = new bootstrap.Modal(document.getElementById('connectionModeModal'));
        modal.show();
    }

    // Exit connection mode
    exitConnectionMode() {
        this.connectionMode = false;
        this.connectionStart = null;
        
        const btn = document.getElementById('connectionModeBtn');
        btn.classList.remove('btn-primary');
        btn.classList.add('btn-outline-secondary');
        
        const modal = bootstrap.Modal.getInstance(document.getElementById('connectionModeModal'));
        if (modal) modal.hide();
    }

    // Update connection preview in modal
    updateConnectionPreview() {
        const previewSvg = d3.select('#connectionPreview');
        previewSvg.selectAll('*').remove();
        
        // Create sample connection for preview
        const fromPoint = { x: 20, y: 50 };
        const toPoint = { x: 280, y: 50 };
        
        const style = {
            connectionType: document.getElementById('modalConnectionType').value,
            color: document.getElementById('modalRelationshipColor').value,
            width: parseFloat(document.getElementById('modalRelationshipWidth').value),
            lineStyle: document.getElementById('modalRelationshipLineStyle').value,
            pattern: document.getElementById('modalRelationshipPattern').value,
            startArrowStyle: document.getElementById('modalRelationshipStartArrow').value,
            arrowStyle: document.getElementById('modalRelationshipArrowStyle').value,
            opacity: parseFloat(document.getElementById('modalRelationshipOpacity').value),
            shadow: document.getElementById('modalRelationshipShadow').value
        };
        
        // Create defs for markers in preview
        const defs = previewSvg.append('defs');
        this.createArrowMarker(defs, 'preview-arrow', 'M0,-5L10,0L0,5', style.color);
        this.createArrowMarker(defs, 'preview-arrow-start', 'M10,-5L0,0L10,5', style.color);
        
        // Create connection path
        const pathData = this.createConnectionPath(fromPoint, toPoint, style);
        const path = previewSvg.append('path')
            .attr('d', pathData)
            .attr('fill', 'none')
            .attr('stroke', style.color)
            .attr('stroke-width', style.width)
            .attr('opacity', style.opacity);
        
        // Apply line style
        this.applyLineStyle(path, style);
        
        // Apply visual effects
        this.applyVisualEffects(path, style);
        
        // Add arrows
        if (style.startArrowStyle !== 'none') {
            path.attr('marker-start', 'url(#preview-arrow-start)');
        }
        if (style.arrowStyle !== 'none') {
            path.attr('marker-end', 'url(#preview-arrow)');
        }
    }

    // Context menu handlers
    editRelationshipFromContext() {
        if (this.selectedRelationship) {
            this.showRelationshipModal(this.selectedRelationship);
        }
    }

    duplicateRelationshipFromContext() {
        if (this.selectedRelationship) {
            const original = this.relationships.get(this.selectedRelationship);
            if (original) {
                const duplicate = {
                    ...original,
                    id: this.generateId(),
                    description: `${original.description} (Copy)`
                };
                this.addRelationship(duplicate);
            }
        }
    }

    deleteRelationshipFromContext() {
        if (this.selectedRelationship) {
            if (confirm('Are you sure you want to delete this relationship?')) {
                this.deleteRelationship(this.selectedRelationship);
            }
        }
    }

    // Handle keyboard shortcuts
    handleKeyDown(event) {
        if (event.key === 'Escape') {
            if (this.connectionMode) {
                this.exitConnectionMode();
            }
        } else if (event.key === 'Delete' && this.selectedRelationship) {
            this.deleteSelectedRelationship();
        }
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new C4DiagramEditor();
});