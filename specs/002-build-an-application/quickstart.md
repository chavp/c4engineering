# Quickstart Guide: C4Engineering Platform MVP

**Purpose**: Validate core user workflows from service catalog to C4 visualization to local Docker deployment
**Prerequisites**: Docker Desktop installed and running, .NET 8 SDK installed
**Estimated Time**: 15-20 minutes

## Setup and Installation

### 1. Clone and Build the Application
```bash
# Clone the repository
git clone https://github.com/company/c4engineering-platform.git
cd c4engineering-platform

# Restore dependencies and build
dotnet restore
dotnet build

# Run the application
dotnet run --project src/C4Engineering.Web
```

**Expected Result**: Application starts on `http://localhost:5000` with startup logs showing successful initialization.

### 2. Verify Docker Desktop Integration
```bash
# Test Docker Desktop connectivity
docker ps
docker images
```

**Expected Result**: Docker commands execute successfully, showing running containers and available images.

### 3. Initialize Sample Data
Navigate to `http://localhost:5000/setup` and click "Initialize Sample Data"

**Expected Result**: 
- 3 sample services created (payment-service, user-service, notification-service)
- 2 sample teams created (payments-team, platform-team)
- 1 sample C4 Context diagram generated automatically

## Core Workflow Validation

### Scenario 1: Service Discovery and Catalog Management

#### Step 1.1: Browse Service Catalog
1. Navigate to `http://localhost:5000/services`
2. Verify the service catalog displays with sample services
3. Filter services by team using the dropdown filter
4. Search for "payment" in the search box

**Expected Results**:
- All 3 services display in a responsive grid layout
- Filtering by "payments-team" shows only payment-service
- Search returns relevant services with highlighted matches
- Each service card shows name, description, owner, and status indicators

#### Step 1.2: Create New Service
1. Click "Add Service" button
2. Fill form with:
   - ID: `order-service`
   - Name: `Order Management Service`
   - Description: `Handles order creation and tracking`
   - Type: `Service`
   - Owner: `payments-team`
   - Repository: `https://github.com/company/order-service`
3. Click "Create Service"

**Expected Results**:
- Form validation provides immediate feedback
- Service created successfully with toast notification
- Service appears immediately in catalog list
- Service is available for diagram creation

### Scenario 2: C4 Model Architecture Visualization

#### Step 2.1: View Auto-Generated Context Diagram
1. From service catalog, click "View Architecture" on payment-service
2. Verify C4 Context diagram loads showing system boundaries
3. Observe automatic positioning of related services
4. Test zoom and pan functionality

**Expected Results**:
- Context diagram renders within 2 seconds
- Payment system positioned centrally with connected external systems
- Relationships show labeled arrows with interaction descriptions
- Canvas supports smooth zoom and pan operations

#### Step 2.2: Create Container-Level Diagram
1. Double-click on the payment system element
2. Confirm navigation to Container-level view
3. Use drag-and-drop to add containers from the palette:
   - Drag "Web Application" container to canvas
   - Drag "Database" container to canvas
   - Drag "API Gateway" container to canvas
4. Create relationships by dragging between containers
5. Edit container properties by right-clicking

**Expected Results**:
- Smooth transition from Context to Container level
- Component palette loads with appropriate container types
- Drag-and-drop operations snap to grid with visual feedback
- Relationship creation shows real-time preview during drag
- Property editing dialog opens with form validation

#### Step 2.3: Configure Pipeline from Diagram
1. Right-click on Web Application container
2. Select "Configure Pipeline" from context menu
3. Configure build stage:
   - Commands: `dotnet restore`, `dotnet build`, `dotnet test`
   - Working Directory: `/src`
   - Timeout: 300 seconds
4. Configure Docker build stage:
   - Dockerfile: `Dockerfile`
   - Image Name: `payment-web-app`
   - Image Tag: `latest`
5. Configure local deployment stage:
   - Container Name: `payment-web-local`
   - Ports: `8080:80`
   - Environment: `ASPNETCORE_ENVIRONMENT=Development`

**Expected Results**:
- Configuration dialog opens with tabbed interface for each stage
- Form validation prevents invalid configurations
- Docker Desktop availability checked and confirmed
- Pipeline configuration saved with success notification
- Container element shows pipeline indicator icon

### Scenario 3: Pipeline Execution and Local Deployment

#### Step 3.1: Execute Build Pipeline
1. From the container context menu, select "Run Pipeline"
2. Monitor real-time execution in the pipeline status panel
3. Observe build logs streaming in real-time
4. Wait for successful completion

**Expected Results**:
- Pipeline execution starts immediately with status "Running"
- Real-time logs display build output with timestamps
- Progress indicators update for each stage
- Successful completion shows green status and duration
- Docker image created and visible in Docker Desktop

#### Step 3.2: Deploy to Local Docker Desktop
1. Click "Deploy to Local" from the pipeline completion notification
2. Monitor deployment status in the deployment dashboard
3. Verify container health checks pass
4. Access deployed application at `http://localhost:8080`

**Expected Results**:
- Deployment starts with container creation logs
- Health check status updates from "Starting" to "Healthy"
- Container appears in Docker Desktop dashboard
- Application accessible and responsive at specified URL
- Resource usage metrics display in deployment dashboard

#### Step 3.3: Manage Running Deployment
1. Navigate to Deployments section
2. View container logs in real-time
3. Monitor CPU and memory usage graphs
4. Test rollback functionality by clicking "Rollback to Previous"

**Expected Results**:
- Real-time logs stream with filtering capabilities
- Resource usage graphs update every 5 seconds
- Rollback creates new container with previous image
- Old container gracefully stopped and removed
- Health status confirms successful rollback

### Scenario 4: Real-Time Collaboration and Updates

#### Step 4.1: Test Real-Time Diagram Updates
1. Open the same diagram in two browser tabs
2. In first tab, modify a container's position
3. In second tab, add a new relationship
4. Observe real-time synchronization between tabs

**Expected Results**:
- Changes in first tab immediately appear in second tab
- No conflicts occur with simultaneous editing
- User presence indicators show active collaborators
- Change history maintains accurate audit trail

#### Step 4.2: Service Catalog Synchronization
1. Update service metadata from service catalog
2. Navigate to associated architecture diagram
3. Verify diagram elements reflect updated information
4. Confirm pipeline configurations remain consistent

**Expected Results**:
- Service changes propagate to diagram within 2 seconds
- Element properties update automatically
- Pipeline configurations validate against new service metadata
- No broken references or inconsistent state

## Validation Checklist

### Performance Validation
- [ ] Service catalog loads within 2 seconds
- [ ] Diagram rendering completes within 2 seconds
- [ ] API responses under 500ms (use browser dev tools)
- [ ] UI interactions respond within 200ms
- [ ] Real-time updates appear within 1 second

### Functionality Validation
- [ ] All CRUD operations work for services, diagrams, pipelines
- [ ] Docker Desktop integration successfully deploys containers
- [ ] Real-time collaboration functions without conflicts
- [ ] Export functionality generates PNG/SVG/PDF files
- [ ] Health monitoring accurately reflects container status

### User Experience Validation
- [ ] Bootstrap responsive design works on mobile/tablet/desktop
- [ ] Error messages provide clear, actionable guidance
- [ ] Form validation prevents invalid data entry
- [ ] Toast notifications confirm successful operations
- [ ] Help tooltips explain complex functionality

### Integration Validation
- [ ] Service catalog data persists across application restarts
- [ ] Diagram changes trigger appropriate pipeline notifications
- [ ] Docker containers clean up properly on deployment failures
- [ ] File system storage maintains data consistency
- [ ] SignalR connections handle network interruptions gracefully

## Troubleshooting Common Issues

### Docker Desktop Not Available
**Symptoms**: Pipeline configuration fails with "Docker not available" error
**Solution**: 
1. Ensure Docker Desktop is running
2. Check Docker API accessibility: `docker ps`
3. Restart application to reinitialize Docker connection

### Pipeline Execution Fails
**Symptoms**: Build stage fails with permission errors
**Solution**:
1. Verify working directory exists and is accessible
2. Check build commands are valid for the project type
3. Review execution logs for specific error details

### Real-Time Updates Not Working  
**Symptoms**: Changes in one browser tab don't appear in another
**Solution**:
1. Check browser console for SignalR connection errors
2. Verify WebSocket support in browser/network
3. Restart application to reset SignalR hub connections

### Performance Issues
**Symptoms**: Slow diagram rendering or API responses
**Solution**:
1. Check Docker Desktop resource allocation
2. Monitor application memory usage in Task Manager
3. Clear browser cache and reload application

---

**Quickstart Complete**: All core workflows validated and ready for development team testing.