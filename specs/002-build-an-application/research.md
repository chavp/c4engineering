# Research: C4Engineering Platform MVP Technical Decisions

**Feature**: C4Engineering Platform MVP with Backstage Integration  
**Date**: 2025-01-10  
**Status**: Complete

## Technology Stack Decisions

### Backend Framework: ASP.NET Core 8.0
**Decision**: Use ASP.NET Core 8.0 with MVC pattern for REST API and server-side rendering
**Rationale**: 
- Native C# .NET 8 support with latest performance improvements
- Built-in dependency injection, configuration, and logging
- Excellent Docker integration and cross-platform support
- SignalR integration for real-time collaboration features
- Minimal setup overhead compared to separate API/SPA architecture

### Frontend Approach: Server-Side Rendering + Progressive Enhancement
**Decision**: Razor views with Bootstrap 5.3 and vanilla JavaScript modules
**Rationale**:
- Faster initial page loads compared to SPA approaches
- SEO-friendly architecture for service catalog discoverability
- Progressive enhancement allows rich interactions without framework complexity
- Bootstrap provides consistent, accessible UI components out of the box
- Vanilla JS modules ensure minimal bundle size and faster loading

### Data Storage: JSON File System
**Decision**: Structured JSON files in wwwroot/data directory with repository pattern
**Rationale**:
- Zero database setup for MVP and local development
- Human-readable data format for debugging and manual editing
- Git-friendly for version control and collaboration
- Easy migration path to database when scaling requirements emerge
- File system watching enables real-time updates

### Real-Time Communication: SignalR
**Decision**: SignalR hubs for collaborative diagram editing and deployment status
**Rationale**:
- Native ASP.NET Core integration with automatic fallback support
- WebSocket-first with graceful degradation to long polling
- Strongly-typed hub methods for type safety
- Automatic reconnection and connection management
- Minimal client-side JavaScript required

### Docker Integration: Docker.DotNet
**Decision**: Docker.DotNet library for Docker Desktop API integration
**Rationale**:
- Official Docker SDK for .NET with comprehensive API coverage
- Async/await support for non-blocking operations
- Stream support for real-time logs and build output
- Cross-platform compatibility matching .NET Core support
- Well-documented with active maintenance

## Architecture Patterns

### Domain-Driven Design (DDD) Lite
**Services**: Business logic separated into domain services
- ServiceCatalogService: Manages service discovery and metadata
- DiagramService: Handles C4 model creation and validation
- PipelineService: Orchestrates build and deployment workflows
- DockerDeploymentService: Manages local Docker Desktop deployments

### Repository Pattern
**Data Access**: JSON file operations abstracted through repositories
- IServiceRepository: Service catalog CRUD operations
- IDiagramRepository: Architecture diagram persistence
- IPipelineRepository: Pipeline configuration storage
- IDeploymentRepository: Deployment history and status

### MVC with API Controllers
**Web Layer**: Controllers handle both UI and API endpoints
- ServiceCatalogController: REST API + Razor views for service management
- DiagramController: C4 model CRUD operations with real-time updates
- PipelineController: Build/deploy pipeline management
- DeploymentController: Docker Desktop deployment operations

## JavaScript Architecture

### Module Pattern
**Decision**: ES6 modules with explicit imports/exports, no bundling for MVP
**Structure**:
```javascript
// service-catalog.js - Service discovery and management
// diagram-editor.js - C4 model drag-and-drop editor  
// pipeline-config.js - Pipeline configuration UI
// deployment-manager.js - Docker deployment interface
// shared/api-client.js - Centralized API communication
// shared/signalr-client.js - Real-time communication
```

### Event-Driven Communication
**Pattern**: Custom events for module communication, avoiding tight coupling
**Implementation**: 
- ServiceCatalogChanged events trigger diagram updates
- DiagramModified events update deployment configurations
- DeploymentStatusChanged events update UI indicators

## Performance Considerations

### Client-Side Optimization
- **Lazy Loading**: JavaScript modules loaded on-demand by page
- **Image Optimization**: SVG icons for scalability, optimized PNG exports
- **Caching Strategy**: Browser caching for static assets, ETag headers for API responses
- **Minimal Dependencies**: Bootstrap + vanilla JS only, no heavy frameworks

### Server-Side Optimization  
- **Async Operations**: All I/O operations use async/await patterns
- **Response Caching**: In-memory caching for frequently accessed data
- **JSON Streaming**: Large diagram data streamed rather than loaded entirely
- **Connection Pooling**: SignalR connection management for concurrent users

## Security Architecture

### Authentication Strategy
**MVP**: Basic authentication with username/password stored in configuration
**Future**: Pluggable authentication supporting OAuth2, SAML, Active Directory

### Authorization Model
**Role-Based**: Admin, Developer, Viewer roles with hierarchical permissions
**Resource-Based**: Team ownership model for services and diagrams

### Input Validation
**Client-Side**: HTML5 validation + Bootstrap validation classes
**Server-Side**: Model validation attributes + custom validators for complex rules
**Docker Integration**: Command injection prevention through parameterized Docker API calls

## Integration Points

### Docker Desktop API
**Connection**: Local Docker daemon via named pipes (Windows) or Unix sockets (Linux/macOS)
**Operations**: Container lifecycle, image management, network configuration, log streaming
**Error Handling**: Graceful degradation when Docker Desktop unavailable

### File System Integration
**Watching**: FileSystemWatcher for real-time data updates
**Concurrency**: File locking strategy for concurrent access
**Backup**: Automatic JSON backup before destructive operations

### Future Extensibility
**Plugin Architecture**: Interface-based plugin system for custom deployment targets
**Webhook Support**: Outbound webhooks for integration with external CI/CD systems
**Import/Export**: Backstage entity format compatibility for migration scenarios

## Development Workflow

### Hot Reload
**Backend**: ASP.NET Core hot reload for C# code changes
**Frontend**: Browser refresh for static content, SignalR for real-time updates
**Data**: File system watching triggers automatic data reload

### Testing Strategy
**Unit Tests**: Service layer with mocked repositories
**Integration Tests**: Full HTTP pipeline with test data
**E2E Tests**: Playwright for critical user workflows
**Docker Tests**: Testcontainers for Docker integration scenarios

## Deployment Architecture

### Local Development
**Docker Compose**: Multi-container setup with the platform and test services
**HTTPS**: Self-signed certificates for local HTTPS development
**Debugging**: Remote debugging support for containerized applications

### Production Considerations (Future)
**Container Orchestration**: Kubernetes deployment manifests
**Database Migration**: Automated JSON-to-database migration tools
**Scaling**: Horizontal scaling through load balancer configuration
**Monitoring**: Application insights integration for telemetry

---

**Research Complete**: All technical decisions documented and ready for Phase 1 design artifacts.