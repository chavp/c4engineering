# C4Engineering Platform MVP

A platform engineering MVP that combines **Backstage-inspired service catalog** functionality with **interactive C4 model architecture visualization**, **comprehensive project management**, and **local Docker Desktop deployment** capabilities.

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Docker Desktop (for deployment features)

### Running the Application

1. **Clone and navigate to the project:**
   ```bash
   git clone <repository-url>
   cd C4Engineering
   ```

2. **Build and run:**
   ```bash
   dotnet build
   cd src/C4Engineering.Web
   dotnet run
   ```

3. **Access the application:**
   - Web UI: http://localhost:5000
   - API Documentation: http://localhost:5000/swagger
   - Service Catalog: http://localhost:5000/ServiceCatalog
   - Projects: http://localhost:5000/Projects
   - Architecture Editor: http://localhost:5000/Architecture/Editor

## 🏗️ Architecture Overview

### Technology Stack
- **Backend**: ASP.NET Core 8.0 MVC with Web API controllers
- **Frontend**: Server-side Razor views with Bootstrap 5.3 and vanilla JavaScript modules
- **Real-time**: SignalR hubs for collaborative diagram editing
- **Storage**: JSON files in structured directory layout for MVP
- **Integration**: Docker.DotNet for Docker Desktop API communication
- **Visualization**: D3.js for interactive C4 diagrams

### Project Structure
```
C4Engineering/
├── src/C4Engineering.Web/           # Main web application
│   ├── Controllers/                 # MVC and API controllers
│   │   ├── Api/                    # REST API controllers  
│   │   ├── ArchitectureController   # C4 diagram management
│   │   ├── ProjectsController       # Project management
│   │   └── ServiceCatalogController # Service catalog
│   ├── Data/                       # Data access layer
│   │   └── Repositories/           # Repository implementations
│   ├── Hubs/                       # SignalR hubs
│   ├── Models/                     # Domain models
│   │   ├── Architecture/           # C4 diagram models
│   │   ├── Project/               # Project management models
│   │   ├── ServiceCatalog/        # Service catalog models
│   │   └── Requests/              # API request/response models
│   ├── Services/                   # Business logic layer
│   ├── Views/                      # Razor views
│   │   ├── Architecture/          # Diagram editor views
│   │   ├── Projects/              # Project management views
│   │   └── ServiceCatalog/        # Service catalog views
│   └── wwwroot/                    # Static files and data
│       ├── css/                   # Custom styles
│       ├── js/                    # JavaScript modules
│       └── data/                  # JSON data storage
├── tests/C4Engineering.Tests/       # Unit and integration tests
└── specs/                          # API specifications
```

## 🎯 Features Implemented

### ✅ **NEW** Project Management (Platform Engineering)
- **🎨 Project Templates**: Pre-configured templates (Web App, Microservices, Data Platform)
- **📊 Project Dashboard**: Visual project cards with metrics and status
- **👥 Team Management**: Role-based team member management (Owner, Maintainer, Developer)
- **🔗 Resource Linking**: Connect services, diagrams, and pipelines to projects
- **🏷️ Advanced Tagging**: Flexible tagging and categorization system
- **⚙️ Project Settings**: Repository links, documentation, Slack/Jira integration
- **🔍 Advanced Filtering**: Filter by type, status, owner, tags with real-time search
- **📋 Project Templates**: Quick project creation with sensible defaults

#### Project Features
- **Create from Templates**: Choose from Web Application, Microservices, or Data Platform templates
- **Custom Project Creation**: Full flexibility for unique project requirements
- **Project Relationships**: Link services, diagrams, and pipelines to projects
- **Team Collaboration**: Manage team members with different permission levels
- **Project Metadata**: Track creation dates, versions, and custom fields

### ✅ **ENHANCED** C4 Architecture Visualization & Editing
- **🎨 Interactive Diagram Editor**: Drag-and-drop C4 element creation
- **🔗 Advanced Relationship Management**: 
  - **Add Relationships**: Modal form or visual drag-to-connect
  - **Edit Relationships**: Real-time property editing with style options
  - **Delete Relationships**: Multiple deletion methods with cascade support
- **⚡ Real-time Collaboration**: Multi-user diagram editing with SignalR
- **🎯 Multiple Diagram Types**: Context, Container, Component, and Code diagrams
- **🎨 Rich Styling Options**: Custom colors, line styles, arrow types
- **📱 Responsive Design**: Works on desktop, tablet, and mobile
- **⌨️ Keyboard Shortcuts**: Full keyboard navigation and shortcuts
- **💾 Auto-save & Export**: PNG, SVG, PDF export with auto-save functionality

#### Relationship Management Features
- **Visual Connection Mode**: Click-and-connect workflow for intuitive relationship creation
- **Relationship Types**: Uses, Calls, Sends, Includes, Extends, Depends with color coding
- **Style Customization**: Line width, style (solid/dashed/dotted), arrow types, colors
- **Smart Labeling**: Technology labels, descriptions with auto-positioning
- **Context Menus**: Right-click menus for quick relationship actions
- **Bulk Operations**: Select and manage multiple relationships
- **Real-time Updates**: Live collaboration with conflict resolution

### ✅ Service Catalog (Backstage-inspired)
- **Service Discovery**: Browse and search services across your organization
- **Service Registration**: Add new services with comprehensive metadata
- **Team Management**: Organize services by ownership teams
- **Service Dependencies**: Track and visualize dependencies between services
- **Lifecycle Management**: Track service lifecycle stages (dev, staging, production)
- **API Integration**: Complete CRUD operations via REST endpoints
- **Service Details**: Rich service information with external links

#### Sample Services
The application comes with sample data including:
- **User Service**: Core authentication and user management
- **Notification Service**: Email and push notifications  
- **Frontend Application**: React-based user interface
- **Payment Service**: Payment processing service

### ✅ Technical Infrastructure
- **JSON-based Storage**: File-based data storage for MVP simplicity
- **Repository Pattern**: Clean separation of data access logic with async operations
- **Service Layer**: Business logic abstraction with comprehensive error handling
- **Dependency Injection**: Proper IoC container configuration
- **SignalR Hubs**: Real-time communication for collaborative features
- **OpenAPI/Swagger**: Comprehensive API documentation with examples
- **Modular Architecture**: Clean separation of concerns across layers

### ✅ Frontend Components
- **Modern Bootstrap 5**: Responsive design with custom CSS variables
- **ES6 JavaScript Modules**: Maintainable, modular frontend architecture
- **Interactive Components**: Service cards, project cards, diagram elements
- **Real-time Updates**: SignalR integration for live collaboration
- **Advanced Filtering**: Multi-criteria filtering with instant search
- **Accessibility Support**: ARIA labels, keyboard navigation, screen reader support
- **Mobile Optimization**: Touch-friendly interfaces for mobile devices

## 🛠️ **UPDATED** API Endpoints

### Projects API (`/api/projects`) - **NEW**
- `GET /api/projects` - List all projects with optional filtering
- `GET /api/projects/{id}` - Get specific project details  
- `POST /api/projects` - Create new project
- `PUT /api/projects/{id}` - Update existing project
- `DELETE /api/projects/{id}` - Delete project
- `GET /api/projects/search?q={term}` - Search projects
- `GET /api/projects/templates` - Get available project templates
- `POST /api/projects/from-template/{templateId}` - Create project from template
- `POST /api/projects/{id}/services/{serviceId}` - Add service to project
- `DELETE /api/projects/{id}/services/{serviceId}` - Remove service from project
- `POST /api/projects/{id}/members` - Add team member to project
- `DELETE /api/projects/{id}/members/{email}` - Remove team member

### Diagrams API (`/api/diagrams`) - **ENHANCED**
- `GET /api/diagrams` - List all diagrams with filtering by type/system
- `GET /api/diagrams/{id}` - Get specific diagram with elements and relationships
- `POST /api/diagrams` - Create new diagram
- `PUT /api/diagrams/{id}` - Update diagram (elements, relationships, metadata)
- `DELETE /api/diagrams/{id}` - Delete diagram
- `POST /api/diagrams/{id}/elements` - Add element to diagram
- `PUT /api/diagrams/{id}/elements/{elementId}` - Update diagram element
- `DELETE /api/diagrams/{id}/elements/{elementId}` - Delete diagram element
- `POST /api/diagrams/{id}/relationships` - Add relationship between elements
- `PUT /api/diagrams/{id}/relationships/{relationshipId}` - Update relationship
- `DELETE /api/diagrams/{id}/relationships/{relationshipId}` - Delete relationship

### Services API (`/api/services`) - **EXISTING**
- `GET /api/services` - List all services with optional filtering
- `GET /api/services/{id}` - Get specific service details
- `POST /api/services` - Create new service
- `PUT /api/services/{id}` - Update existing service
- `DELETE /api/services/{id}` - Delete service
- `GET /api/services/teams` - Get all teams
- `GET /api/services/{id}/dependencies` - Get service dependencies

### Example API Usage

**Create a new project:**
```bash
curl -X POST "http://localhost:5000/api/projects" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "my-platform-project",
    "name": "My Platform Project",
    "description": "A comprehensive platform engineering project",
    "owner": "Platform Team",
    "type": "Microservices",
    "tags": ["microservices", "api", "platform"],
    "settings": {
      "repository": "https://github.com/company/platform-project",
      "isPublic": true,
      "enableNotifications": true
    }
  }'
```

**Create project from template:**
```bash
curl -X POST "http://localhost:5000/api/projects/from-template/web-app-template" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "my-web-app",
    "name": "My Web Application",
    "description": "Full-stack web application",
    "owner": "Development Team"
  }'
```

**Add relationship to diagram:**
```bash
curl -X POST "http://localhost:5000/api/diagrams/my-diagram/relationships" \
  -H "Content-Type: application/json" \
  -d '{
    "sourceId": "web-app",
    "targetId": "user-service", 
    "description": "Authenticates users via",
    "technology": "HTTPS/JWT",
    "style": {
      "color": "#007bff",
      "width": 2,
      "lineStyle": "solid",
      "arrowStyle": "arrow"
    }
  }'
```

## 🎨 **ENHANCED** User Interface

### **NEW** Projects Dashboard
- **Project Grid**: Visual cards showing project metrics and status
- **Template Gallery**: Quick access to project templates with previews
- **Advanced Filtering**: Filter by type, status, owner, tags with real-time search
- **Project Creation**: Modal-based project creation with template support
- **Project Details**: Comprehensive project overview with team management

### **ENHANCED** Architecture Editor
- **Interactive Canvas**: Drag-and-drop element creation with snap-to-grid
- **Component Palette**: Organized palette with C4 element types
- **Properties Panel**: Real-time element and relationship editing
- **Relationship Manager**: Dedicated panel for relationship management
- **Connection Mode**: Visual drag-to-connect workflow with instructions
- **Toolbar**: Save, export (PNG/SVG/PDF), share, and collaboration tools
- **Zoom Controls**: Pan, zoom, fit-to-screen with keyboard shortcuts

### Service Catalog - **EXISTING**  
- **Service Grid**: Visual grid of service cards with enhanced styling
- **Advanced Filtering**: Multi-criteria filtering with instant search
- **Service Details**: Comprehensive service information pages
- **Add Service Modal**: User-friendly service creation and editing

### **NEW** Navigation & Layout
- **Unified Sidebar**: Consistent navigation across all features
- **Breadcrumb Navigation**: Clear navigation hierarchy  
- **Responsive Design**: Mobile-first design with tablet optimization
- **Dark/Light Theme**: Theme support with user preferences

## 🔄 **ENHANCED** Real-time Features

### SignalR Hubs
- **DiagramCollaborationHub**: Real-time collaborative diagram editing
  - Multi-user element manipulation
  - Live relationship updates
  - User presence indicators
  - Conflict resolution for concurrent edits
- **PipelineStatusHub**: Live pipeline execution updates
- **ProjectActivityHub**: Real-time project updates and notifications

### Collaborative Features
- **Live Cursors**: See other users' cursors and activities
- **Change Broadcasting**: Instant updates for all connected users
- **User Presence**: Online/offline status for team members
- **Activity Notifications**: Real-time notifications for project activities

## 📊 **UPDATED** Data Models

### **NEW** Project Model
```json
{
  "id": "platform-engineering-demo",
  "name": "Platform Engineering Demo", 
  "description": "Demonstration project showcasing platform engineering capabilities",
  "owner": "Platform Team",
  "type": "infrastructure",
  "status": "active",
  "tags": ["platform", "demo", "infrastructure"],
  "services": ["user-service", "notification-service"],
  "diagrams": ["context-sample", "container-sample"],
  "pipelines": [],
  "teamMembers": [
    {
      "name": "Platform Team",
      "email": "platform.team@company.com",
      "role": "owner",
      "joinedAt": "2024-01-01T00:00:00Z"
    }
  ],
  "settings": {
    "repository": "https://github.com/company/platform-demo",
    "documentation": "https://docs.company.com/platform",
    "slackChannel": "#platform-engineering",
    "isPublic": true,
    "enableNotifications": true
  },
  "metadata": {
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-10T12:00:00Z",
    "version": "1.0",
    "template": null
  }
}
```

### **ENHANCED** Diagram Model  
```json
{
  "id": "context-sample",
  "name": "Sample Context Diagram",
  "type": "context",
  "description": "High-level system context",
  "elements": [
    {
      "id": "user",
      "name": "Customer",
      "type": "person",
      "x": 100, "y": 100,
      "width": 120, "height": 80,
      "color": "#1168bd",
      "description": "End user of the system"
    }
  ],
  "relationships": [
    {
      "id": "rel-1",
      "sourceId": "user",
      "targetId": "system",
      "description": "Uses system via",
      "technology": "HTTPS",
      "style": {
        "color": "#007bff",
        "width": 2,
        "lineStyle": "solid",
        "arrowStyle": "arrow"
      }
    }
  ],
  "metadata": {
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-10T12:00:00Z",
    "version": "1.0"
  }
}
```

### Service Model - **EXISTING**
```json
{
  "id": "user-service",
  "name": "User Service", 
  "description": "Core user management service",
  "type": "service",
  "owner": "Backend Team",
  "repository": "https://github.com/company/user-service",
  "documentation": "https://docs.company.com/user-service",
  "tags": ["authentication", "users", "microservice"],
  "lifecycle": "production",
  "system": "user-management",
  "dependsOn": [],
  "providesApis": ["user-api", "auth-api"],
  "consumesApis": ["notification-api"]
}
```

## 🚧 **UPDATED** Roadmap

### ✅ **COMPLETED** Recent Features
- **✅ Project Management System**: Complete project lifecycle management
- **✅ C4 Diagram Editor**: Interactive architecture visualization  
- **✅ Advanced Relationship Management**: Visual relationship creation/editing
- **✅ Real-time Collaboration**: Multi-user diagram editing
- **✅ Project Templates**: Quick-start templates for common architectures
- **✅ Enhanced Navigation**: Unified sidebar and responsive design

### 🔄 **CURRENT** Phase Features  
- **🔧 Pipeline Management**: Build and deployment pipeline configuration
- **🐳 Docker Integration**: Container deployment to Docker Desktop
- **📊 Architecture Generation**: Auto-generate C4 diagrams from service dependencies
- **🔐 Basic Authentication**: User session management

### 🎯 **NEXT** Phase Features
- **📈 Metrics Dashboard**: Service health and performance monitoring
- **🔗 Integration Hub**: Connect with external tools (GitHub, Slack, etc.)
- **📋 Template System**: Custom service and project templates
- **👥 Advanced RBAC**: Role-based access control with permissions
- **🌐 Multi-environment**: Support for dev/staging/prod environments

## 🧪 **ENHANCED** Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/C4Engineering.Tests/
```

### Test Structure
- **Unit Tests**: Repository, service, and model tests
- **Integration Tests**: API endpoint tests with TestServer
- **Contract Tests**: API contract validation
- **Frontend Tests**: JavaScript module testing
- **End-to-End Tests**: Full workflow testing

### Test Coverage
- **Controllers**: API endpoint testing with mock dependencies
- **Services**: Business logic testing with comprehensive scenarios
- **Repositories**: Data access testing with JSON storage
- **Models**: Validation and serialization testing
- **JavaScript**: Module functionality and DOM interaction testing

## 🔧 **ENHANCED** Development

### Adding New Features

#### **Projects**
Projects are stored as JSON files in `wwwroot/data/projects/`:
1. Create JSON file named `{project-id}.json`
2. Update `index.json` to include project ID  
3. Follow ProjectModel schema for consistency

#### **Diagrams**
Diagrams use the C4 model structure:
1. Create JSON file in `wwwroot/data/diagrams/`
2. Include elements array with C4 element types
3. Define relationships between elements with styling
4. Use DiagramModel schema for validation

#### **Services**
Services follow Backstage service catalog patterns:
1. JSON file in `wwwroot/data/services/`
2. Include comprehensive metadata and dependencies
3. Update index.json for service discovery

### Configuration
- **Data Directory**: Configured in `appsettings.json` under `JsonDataOptions`
- **Logging**: Standard ASP.NET Core logging with Serilog support
- **CORS**: Configured for development with production overrides
- **SignalR**: Real-time communication configuration

### Code Style & Patterns
- **C# Conventions**: Modern C# 12+ with nullable reference types and records
- **JavaScript**: ES6+ modules with strict mode and modern patterns
- **CSS**: Bootstrap 5 with CSS custom properties for theming
- **Architecture**: Clean Architecture with CQRS patterns where appropriate

### **NEW** JavaScript Module Organization
```
wwwroot/js/
├── shared/                    # Shared utilities
│   ├── api-client.js         # REST API client
│   └── ui-helpers.js         # UI utility functions
├── diagram-editor.js         # C4 diagram editor (1400+ lines)
├── projects.js               # Project management
├── service-catalog.js        # Service catalog functionality
└── main.js                   # Application initialization
```

## 📈 **ENHANCED** Performance & Scalability

### Current MVP Optimizations
- **Async Operations**: All I/O operations use async/await patterns
- **JSON Streaming**: Large JSON files processed with streaming
- **Client-side Caching**: Browser caching for static resources
- **Lazy Loading**: JavaScript modules loaded on demand
- **SVG Optimization**: Efficient SVG rendering for diagrams
- **Real-time Efficiency**: Optimized SignalR message broadcasting

### Production Readiness Checklist
- **✅ Async Patterns**: Full async/await implementation
- **✅ Error Handling**: Comprehensive exception handling
- **✅ Logging**: Structured logging with correlation IDs
- **🔄 Database Migration**: Plan for production database
- **🔄 Caching Strategy**: Redis integration for scaling
- **🔄 Authentication**: Production-ready auth system
- **🔄 Monitoring**: Health checks and metrics collection
- **🔄 Security**: Input validation, XSS protection, CSRF tokens

### Scalability Considerations
- **Repository Pattern**: Easy migration to different storage systems
- **Service Layer**: Business logic isolated for microservices migration
- **API-First Design**: Clean separation for potential service decomposition
- **Real-time Architecture**: SignalR scales with Redis backplane
- **Frontend Modularity**: JavaScript modules support micro-frontend patterns

## 🛡️ **NEW** Security & Best Practices

### Security Implementation  
- **Input Validation**: Model validation with data annotations
- **XSS Protection**: Proper HTML encoding in Razor views
- **CSRF Protection**: Anti-forgery tokens in forms
- **Secure Headers**: Security headers middleware
- **Error Handling**: Secure error responses without information leakage

### Best Practices Followed
- **Clean Architecture**: Separation of concerns across layers
- **SOLID Principles**: Dependency injection and interface segregation
- **REST API Design**: RESTful endpoints with proper HTTP status codes
- **Responsive Design**: Mobile-first approach with progressive enhancement
- **Accessibility**: WCAG 2.1 AA compliance with ARIA labels

## 🤝 Contributing

### Development Workflow
1. **Fork** the repository
2. **Create** a feature branch (`feature/amazing-feature`)
3. **Follow** established code patterns and conventions
4. **Add** comprehensive tests for new functionality
5. **Update** documentation and README as needed
6. **Submit** a pull request with clear description

### Code Quality Standards
- **Test Coverage**: Maintain >80% code coverage
- **Documentation**: XML documentation for public APIs
- **Code Review**: All changes require peer review
- **Conventional Commits**: Use conventional commit message format
- **Linting**: Follow established C# and JavaScript style guides

## 📊 **NEW** Metrics & Monitoring

### Current Metrics Tracked
- **API Performance**: Response times and throughput
- **User Activity**: Feature usage and engagement
- **System Health**: Application uptime and errors
- **Real-time Usage**: Active collaboration sessions

### Observability Stack
- **Logging**: Structured logging with Serilog
- **Metrics**: Custom metrics with ASP.NET Core metrics
- **Tracing**: Activity-based request tracing
- **Health Checks**: Comprehensive health check endpoints

## 🔗 **NEW** Integration Capabilities

### External System Integration
- **GitHub**: Repository and documentation links
- **Slack**: Team communication integration
- **Jira**: Project management integration  
- **Docker**: Container deployment and management
- **CI/CD**: Pipeline configuration and monitoring

### API-First Architecture
- **OpenAPI/Swagger**: Comprehensive API documentation
- **REST Standards**: Proper HTTP methods and status codes
- **Versioning Strategy**: API versioning support
- **Rate Limiting**: Built-in rate limiting capabilities
- **CORS Support**: Cross-origin resource sharing configuration

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- **C4 Model**: Simon Brown's C4 model for software architecture
- **Backstage**: Spotify's developer portal inspiration
- **Bootstrap**: Front-end component framework
- **D3.js**: Data visualization and SVG manipulation
- **SignalR**: Real-time web functionality

---

**Built with ❤️ using ASP.NET Core 8, Bootstrap 5, D3.js, and modern web technologies**

> **Latest Updates**: Enhanced with comprehensive project management, advanced C4 diagram editing with relationship management, real-time collaboration, and production-ready architecture patterns.