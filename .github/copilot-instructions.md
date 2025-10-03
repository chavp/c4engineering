# GitHub Copilot Instructions: C4Engineering Platform MVP

## Project Overview
C4Engineering Platform is a platform engineering MVP that combines **Backstage-inspired service catalog** functionality with **interactive C4 model architecture visualization** and **local Docker Desktop deployment** capabilities. Built with C# .NET 8, ASP.NET Core, Bootstrap 5, and vanilla JavaScript.

## Architecture Pattern
- **Backend**: ASP.NET Core 8.0 MVC with Web API controllers
- **Frontend**: Server-side Razor views with Bootstrap 5.3 and vanilla JavaScript modules
- **Real-time**: SignalR hubs for collaborative diagram editing
- **Storage**: JSON files in structured directory layout for MVP
- **Integration**: Docker.DotNet for Docker Desktop API communication

## Code Style and Conventions

### C# Backend Conventions
```csharp
// Use async/await for all I/O operations
public async Task<ServiceModel> GetServiceAsync(string serviceId)
{
    var service = await _repository.GetByIdAsync(serviceId);
    return service ?? throw new NotFoundException($"Service {serviceId} not found");
}

// Use dependency injection with interfaces
public class ServiceCatalogService : IServiceCatalogService
{
    private readonly IServiceRepository _repository;
    private readonly ILogger<ServiceCatalogService> _logger;

    public ServiceCatalogService(IServiceRepository repository, ILogger<ServiceCatalogService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}

// Use record types for DTOs and requests
public record CreateServiceRequest(
    string Id,
    string Name,
    string Description,
    string Type,
    string Owner,
    string? Repository = null,
    List<string>? Tags = null);

// Use nullable reference types consistently
public class ServiceModel
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<string> Tags { get; init; } = new();
}
```

### JavaScript Module Conventions
```javascript
// Use ES6 modules with clear imports/exports
// File: wwwroot/js/service-catalog.js
import { ApiClient } from './shared/api-client.js';
import { showToast, showErrorModal } from './shared/ui-helpers.js';

class ServiceCatalogManager {
    constructor() {
        this.apiClient = new ApiClient();
        this.initializeEventListeners();
    }

    async loadServices(filters = {}) {
        try {
            const services = await this.apiClient.get('/api/services', filters);
            this.renderServiceGrid(services);
        } catch (error) {
            showErrorModal('Failed to load services', error.message);
        }
    }

    initializeEventListeners() {
        document.getElementById('service-filter')?.addEventListener('change', (e) => {
            this.loadServices({ team: e.target.value });
        });
    }
}

export { ServiceCatalogManager };
```

### Razor View Conventions
```html
<!-- Use semantic HTML with Bootstrap classes -->
<div class="container-fluid">
    <div class="row">
        <div class="col-md-3">
            <div class="card">
                <div class="card-header">
                    <h5 class="card-title">Service Filters</h5>
                </div>
                <div class="card-body">
                    <select class="form-select" id="team-filter" aria-label="Filter by team">
                        <option value="">All Teams</option>
                        @foreach (var team in Model.Teams)
                        {
                            <option value="@team.Id">@team.Name</option>
                        }
                    </select>
                </div>
            </div>
        </div>
        <div class="col-md-9">
            <div id="service-grid" class="row g-3">
                <!-- Services populated by JavaScript -->
            </div>
        </div>
    </div>
</div>
```

## Domain Models and Entities

### Service Catalog Domain
```csharp
public class ServiceModel
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ServiceType Type { get; init; }
    public string Owner { get; init; } = string.Empty;
    public string? Repository { get; init; }
    public string? Documentation { get; init; }
    public string? ApiSpec { get; init; }
    public List<string> Tags { get; init; } = new();
    public ServiceLifecycle Lifecycle { get; init; }
    public string? System { get; init; }
    public List<string> DependsOn { get; init; } = new();
    public List<string> ProvidesApis { get; init; } = new();
    public List<string> ConsumesApis { get; init; } = new();
    public ServiceMetadata Metadata { get; init; } = new();
}

public enum ServiceType { Service, Website, Library }
public enum ServiceLifecycle { Experimental, Development, Production, Deprecated }
```

### C4 Diagram Domain
```csharp
public class DiagramModel
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DiagramType Type { get; init; }
    public string? System { get; init; }
    public string? Description { get; init; }
    public List<DiagramElement> Elements { get; init; } = new();
    public List<DiagramRelationship> Relationships { get; init; } = new();
    public DiagramMetadata Metadata { get; init; } = new();
}

public class DiagramElement
{
    public string Id { get; init; } = string.Empty;
    public ElementType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Technology { get; init; }
    public Position Position { get; init; } = new();
    public Size Size { get; init; } = new();
    public ElementStyle Style { get; init; } = new();
    public Dictionary<string, object> Properties { get; init; } = new();
}

public enum DiagramType { Context, Container, Component, Code }
public enum ElementType { Person, System, ExternalSystem, Container, Component, CodeElement }
```

### Pipeline Domain
```csharp
public class PipelineConfiguration
{
    public string Id { get; init; } = string.Empty;
    public string ServiceId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<PipelineStage> Stages { get; init; } = new();
    public PipelineTriggers Triggers { get; init; } = new();
    public PipelineMetadata Metadata { get; init; } = new();
}

public class PipelineStage
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public StageType Type { get; init; }
    public List<string> Commands { get; init; } = new();
    public string? WorkingDirectory { get; init; }
    public string? DockerFile { get; init; }
    public string? ImageName { get; init; }
    public string? ImageTag { get; init; }
    public Dictionary<string, string> Environment { get; init; } = new();
    public int Timeout { get; init; } = 300;
    public int RetryCount { get; init; } = 0;
}

public enum StageType { Build, Test, DockerBuild, DockerDeploy, Custom }
```

## Data Access Patterns

### Repository Pattern with JSON Storage
```csharp
public interface IServiceRepository
{
    Task<IEnumerable<ServiceModel>> GetAllAsync();
    Task<ServiceModel?> GetByIdAsync(string id);
    Task<ServiceModel> CreateAsync(ServiceModel service);
    Task<ServiceModel> UpdateAsync(ServiceModel service);
    Task DeleteAsync(string id);
    Task<IEnumerable<ServiceModel>> FindByOwnerAsync(string owner);
}

public class JsonServiceRepository : IServiceRepository
{
    private readonly string _dataDirectory;
    private readonly ILogger<JsonServiceRepository> _logger;

    public JsonServiceRepository(IConfiguration configuration, ILogger<JsonServiceRepository> logger)
    {
        _dataDirectory = Path.Combine(configuration["DataDirectory"] ?? "wwwroot/data", "services");
        _logger = logger;
        Directory.CreateDirectory(_dataDirectory);
    }

    public async Task<ServiceModel> CreateAsync(ServiceModel service)
    {
        var filePath = Path.Combine(_dataDirectory, $"{service.Id}.json");
        if (File.Exists(filePath))
            throw new InvalidOperationException($"Service {service.Id} already exists");

        var json = JsonSerializer.Serialize(service, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
        
        await UpdateIndexAsync();
        return service;
    }

    private async Task UpdateIndexAsync()
    {
        var files = Directory.GetFiles(_dataDirectory, "*.json")
            .Where(f => !Path.GetFileName(f).Equals("index.json"))
            .Select(Path.GetFileNameWithoutExtension)
            .ToList();

        var indexPath = Path.Combine(_dataDirectory, "index.json");
        var json = JsonSerializer.Serialize(files, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(indexPath, json);
    }
}
```

## API Controller Patterns

### RESTful Controllers with OpenAPI
```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ServicesController : ControllerBase
{
    private readonly IServiceCatalogService _service;
    private readonly ILogger<ServicesController> _logger;

    public ServicesController(IServiceCatalogService service, ILogger<ServicesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all services with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServiceModel>), 200)]
    public async Task<ActionResult<IEnumerable<ServiceModel>>> GetServices(
        [FromQuery] string? team = null,
        [FromQuery] string? system = null,
        [FromQuery] ServiceLifecycle? lifecycle = null)
    {
        var services = await _service.GetServicesAsync();
        
        if (!string.IsNullOrEmpty(team))
            services = services.Where(s => s.Owner == team);
            
        if (!string.IsNullOrEmpty(system))
            services = services.Where(s => s.System == system);
            
        if (lifecycle.HasValue)
            services = services.Where(s => s.Lifecycle == lifecycle);

        return Ok(services);
    }

    /// <summary>
    /// Create a new service
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceModel), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<ActionResult<ServiceModel>> CreateService([FromBody] CreateServiceRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var service = await _service.CreateServiceAsync(request);
            return CreatedAtAction(nameof(GetService), new { serviceId = service.Id }, service);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
}
```

## SignalR Hub Patterns

### Collaborative Diagram Editing
```csharp
public class DiagramCollaborationHub : Hub
{
    private readonly IDiagramService _diagramService;

    public DiagramCollaborationHub(IDiagramService diagramService)
    {
        _diagramService = diagramService;
    }

    public async Task JoinDiagram(string diagramId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"diagram-{diagramId}");
        await Clients.Group($"diagram-{diagramId}").SendAsync("UserJoined", Context.UserIdentifier);
    }

    public async Task UpdateElement(string diagramId, DiagramElement element)
    {
        await _diagramService.UpdateElementAsync(diagramId, element);
        await Clients.OthersInGroup($"diagram-{diagramId}")
            .SendAsync("ElementUpdated", element);
    }

    public async Task AddRelationship(string diagramId, DiagramRelationship relationship)
    {
        await _diagramService.AddRelationshipAsync(diagramId, relationship);
        await Clients.OthersInGroup($"diagram-{diagramId}")
            .SendAsync("RelationshipAdded", relationship);
    }
}
```

## Testing Patterns

### Unit Tests with xUnit and Moq
```csharp
public class ServiceCatalogServiceTests
{
    private readonly Mock<IServiceRepository> _repositoryMock;
    private readonly Mock<ILogger<ServiceCatalogService>> _loggerMock;
    private readonly ServiceCatalogService _service;

    public ServiceCatalogServiceTests()
    {
        _repositoryMock = new Mock<IServiceRepository>();
        _loggerMock = new Mock<ILogger<ServiceCatalogService>>();
        _service = new ServiceCatalogService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateServiceAsync_ValidService_ReturnsCreatedService()
    {
        // Arrange
        var request = new CreateServiceRequest("test-service", "Test Service", "Description", "Service", "test-team");
        var expectedService = new ServiceModel { Id = "test-service", Name = "Test Service" };
        
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<ServiceModel>()))
            .ReturnsAsync(expectedService);

        // Act
        var result = await _service.CreateServiceAsync(request);

        // Assert
        Assert.Equal(expectedService.Id, result.Id);
        Assert.Equal(expectedService.Name, result.Name);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<ServiceModel>()), Times.Once);
    }
}
```

### Integration Tests with TestServer
```csharp
public class ServiceCatalogIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ServiceCatalogIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetServices_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/services");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var services = JsonSerializer.Deserialize<List<ServiceModel>>(content);
        Assert.NotNull(services);
    }
}
```

## Docker Integration Patterns

### Docker.DotNet Usage
```csharp
public class DockerDeploymentService : IDockerDeploymentService
{
    private readonly DockerClient _dockerClient;
    private readonly ILogger<DockerDeploymentService> _logger;

    public DockerDeploymentService(ILogger<DockerDeploymentService> logger)
    {
        _dockerClient = new DockerClientConfiguration().CreateClient();
        _logger = logger;
    }

    public async Task<string> DeployContainerAsync(DeploymentConfiguration config)
    {
        // Create container
        var createResponse = await _dockerClient.Containers.CreateContainerAsync(
            new CreateContainerParameters
            {
                Image = config.ImageName,
                Name = config.ContainerName,
                ExposedPorts = config.Ports.ToDictionary(p => p, _ => default(EmptyStruct)),
                HostConfig = new HostConfig
                {
                    PortBindings = config.Ports.ToDictionary(
                        p => p, 
                        p => new List<PortBinding> { new() { HostPort = p.Split(':')[0] } })
                },
                Env = config.Environment.Select(kv => $"{kv.Key}={kv.Value}").ToList()
            });

        // Start container
        await _dockerClient.Containers.StartContainerAsync(createResponse.ID, new ContainerStartParameters());
        
        return createResponse.ID;
    }
}
```

## Performance Considerations

### Async Best Practices
- Always use `async/await` for I/O operations
- Use `ConfigureAwait(false)` in library code
- Implement cancellation tokens for long-running operations
- Use streaming for large JSON responses

### Memory Management
- Dispose of large objects promptly
- Use object pooling for frequently created objects
- Stream large files instead of loading into memory
- Implement proper cleanup in SignalR hubs

### Frontend Optimization
- Lazy load JavaScript modules
- Use CSS/JS minification in production
- Implement browser caching headers
- Optimize diagram rendering with virtual scrolling

---

This file provides comprehensive guidance for GitHub Copilot to understand the project structure, patterns, and conventions used in the C4Engineering Platform MVP development.