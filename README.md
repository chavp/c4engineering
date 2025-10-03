# C4Engineering Platform MVP

A platform engineering MVP that combines **Backstage-inspired service catalog** functionality with **interactive C4 model architecture visualization** and **local Docker Desktop deployment** capabilities.

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
   - Web UI: http://localhost:5066
   - API Documentation: http://localhost:5066/api-docs
   - Service Catalog: http://localhost:5066/ServiceCatalog

## 🏗️ Architecture Overview

### Technology Stack
- **Backend**: ASP.NET Core 8.0 MVC with Web API controllers
- **Frontend**: Server-side Razor views with Bootstrap 5.3 and vanilla JavaScript modules
- **Real-time**: SignalR hubs for collaborative diagram editing
- **Storage**: JSON files in structured directory layout for MVP
- **Integration**: Docker.DotNet for Docker Desktop API communication

### Project Structure
```
C4Engineering/
├── src/C4Engineering.Web/           # Main web application
│   ├── Controllers/                 # MVC and API controllers
│   ├── Data/                       # Data access layer
│   │   └── Repositories/           # Repository implementations
│   ├── Hubs/                       # SignalR hubs
│   ├── Models/                     # Domain models
│   ├── Services/                   # Business logic layer
│   ├── Views/                      # Razor views
│   └── wwwroot/                    # Static files and data
├── tests/C4Engineering.Tests/       # Unit and integration tests
└── specs/                          # API specifications
```

## 🎯 Features Implemented

### ✅ Service Catalog (Backstage-inspired)
- **Service Discovery**: Browse and search services across your organization
- **Service Registration**: Add new services with metadata
- **Team Management**: Organize services by ownership teams
- **Service Dependencies**: Track dependencies between services
- **Lifecycle Management**: Track service lifecycle stages
- **REST API**: Complete CRUD operations via REST endpoints

#### Sample Services
The application comes with sample data including:
- **User Service**: Core authentication and user management
- **Notification Service**: Email and push notifications
- **Frontend Application**: React-based user interface

### ✅ Technical Infrastructure
- **JSON-based Storage**: File-based data storage for MVP simplicity
- **Repository Pattern**: Clean separation of data access logic
- **Service Layer**: Business logic abstraction
- **Dependency Injection**: Proper IoC container configuration
- **SignalR Hubs**: Real-time communication infrastructure
- **OpenAPI/Swagger**: Comprehensive API documentation

### ✅ Frontend Components
- **Responsive Design**: Bootstrap 5-based responsive interface
- **Service Cards**: Visual service representation with key metrics
- **Interactive Filters**: Filter services by team, type, lifecycle
- **Real-time Updates**: SignalR integration for live updates
- **Modular JavaScript**: ES6 modules for maintainable frontend code

## 🛠️ API Endpoints

### Services API (`/api/services`)
- `GET /api/services` - List all services with optional filtering
- `GET /api/services/{id}` - Get specific service details
- `POST /api/services` - Create new service
- `PUT /api/services/{id}` - Update existing service
- `DELETE /api/services/{id}` - Delete service
- `GET /api/services/teams` - Get all teams
- `GET /api/services/{id}/dependencies` - Get service dependencies

### Example API Usage

**Get all services:**
```bash
curl -X GET "http://localhost:5066/api/services" -H "accept: application/json"
```

**Create a new service:**
```bash
curl -X POST "http://localhost:5066/api/services" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "my-service",
    "name": "My Service",
    "description": "A sample service",
    "type": "Service",
    "owner": "My Team"
  }'
```

**Get teams:**
```bash
curl -X GET "http://localhost:5066/api/services/teams" -H "accept: application/json"
```

## 🎨 User Interface

### Dashboard
- **Service Overview**: Quick access to all platform features
- **Quick Actions**: Fast navigation to key functions
- **Service Statistics**: Overview of service catalog

### Service Catalog
- **Service Grid**: Visual grid of service cards
- **Advanced Filtering**: Filter by team, type, lifecycle, search terms
- **Service Details**: Comprehensive service information
- **Add Service Modal**: User-friendly service creation form

## 🔄 Real-time Features

### SignalR Hubs
- **DiagramCollaborationHub**: Real-time collaborative diagram editing
- **PipelineStatusHub**: Live pipeline execution updates

## 📊 Data Models

### Service Model
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

## 🚧 Future Roadmap

### 🔄 Next Phase Features
- **C4 Diagram Visualization**: Interactive architecture diagrams
- **Pipeline Management**: Build and deployment pipeline configuration
- **Docker Integration**: Container deployment to Docker Desktop
- **Architecture Generation**: Auto-generate C4 diagrams from service dependencies

### 🎯 Advanced Features (Future)
- **Authentication & Authorization**: User management and access control
- **Metrics Dashboard**: Service health and performance monitoring
- **Integration Hub**: Connect with external tools (GitHub, Slack, etc.)
- **Template System**: Service templates and scaffolding

## 🧪 Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure
- **Unit Tests**: Repository and service layer tests
- **Integration Tests**: API endpoint tests
- **Contract Tests**: API contract validation

## 🔧 Development

### Adding New Services
Services are stored as JSON files in `wwwroot/data/services/`. Each service requires:
1. A JSON file named `{service-id}.json`
2. Update to `index.json` file to include service ID

### Configuration
- **Data Directory**: Configured in `appsettings.json`
- **Logging**: Standard ASP.NET Core logging
- **CORS**: Configured for development

### Code Style
- **C# Conventions**: Modern C# with nullable reference types
- **JavaScript**: ES6 modules with clear separation of concerns
- **CSS**: Bootstrap 5 with custom CSS variables for theming

## 📈 Performance Considerations

### Current MVP Approach
- **File-based Storage**: Simple JSON files for rapid prototyping
- **In-memory Caching**: Repository pattern allows for future caching
- **Async Operations**: All I/O operations are asynchronous

### Production Readiness
For production deployment, consider:
- **Database**: Replace JSON storage with proper database
- **Caching**: Implement Redis or in-memory caching
- **Security**: Add authentication, authorization, and input validation
- **Monitoring**: Add health checks and metrics collection

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Follow the established code patterns
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

**Built with ❤️ using ASP.NET Core, Bootstrap, and modern web technologies**