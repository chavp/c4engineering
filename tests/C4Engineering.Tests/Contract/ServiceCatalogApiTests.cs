using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace C4Engineering.Tests.Contract;

/// <summary>
/// Contract tests for Service Catalog API endpoints based on OpenAPI specification
/// These tests MUST FAIL initially to enforce TDD methodology
/// </summary>
public class ServiceCatalogApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ServiceCatalogApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task GetServices_ReturnsSuccessStatusCode()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/services");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetServices_WithTeamFilter_ReturnsFilteredResults()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/services?team=payments-team");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var services = JsonSerializer.Deserialize<ServiceModel[]>(content, _jsonOptions);
        
        Assert.NotNull(services);
        // All returned services should belong to the specified team
        Assert.All(services, service => Assert.Equal("payments-team", service.Owner));
    }

    [Fact]
    public async Task GetServices_WithSystemFilter_ReturnsFilteredResults()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/services?system=e-commerce-platform");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var services = JsonSerializer.Deserialize<ServiceModel[]>(content, _jsonOptions);
        
        Assert.NotNull(services);
        Assert.All(services, service => Assert.Equal("e-commerce-platform", service.System));
    }

    [Fact]
    public async Task GetServices_WithLifecycleFilter_ReturnsFilteredResults()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/services?lifecycle=production");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var services = JsonSerializer.Deserialize<ServiceModel[]>(content, _jsonOptions);
        
        Assert.NotNull(services);
        Assert.All(services, service => Assert.Equal("production", service.Lifecycle));
    }

    [Fact]
    public async Task GetServiceById_ExistingService_ReturnsService()
    {
        // Arrange
        var serviceId = "payment-service";

        // Act
        var response = await _client.GetAsync($"/api/services/{serviceId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var service = JsonSerializer.Deserialize<ServiceModel>(content, _jsonOptions);
        
        Assert.NotNull(service);
        Assert.Equal(serviceId, service.Id);
    }

    [Fact]
    public async Task GetServiceById_NonExistentService_ReturnsNotFound()
    {
        // Arrange
        var serviceId = "non-existent-service";

        // Act
        var response = await _client.GetAsync($"/api/services/{serviceId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostService_ValidService_ReturnsCreatedService()
    {
        // Arrange
        var newService = new CreateServiceRequest
        {
            Id = "test-service",
            Name = "Test Service",
            Description = "A test service for contract testing",
            Type = "service",
            Owner = "test-team",
            Repository = "https://github.com/test/test-service",
            Tags = new List<string> { "test", "contract" },
            Lifecycle = "development"
        };

        var json = JsonSerializer.Serialize(newService, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/services", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdService = JsonSerializer.Deserialize<ServiceModel>(responseContent, _jsonOptions);
        
        Assert.NotNull(createdService);
        Assert.Equal(newService.Id, createdService.Id);
        Assert.Equal(newService.Name, createdService.Name);
        Assert.Equal(newService.Owner, createdService.Owner);
    }

    [Fact]
    public async Task PostService_InvalidService_ReturnsBadRequest()
    {
        // Arrange - Missing required fields
        var invalidService = new
        {
            Name = "Invalid Service",
            // Missing Id, Type, and Owner
        };

        var json = JsonSerializer.Serialize(invalidService, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/services", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutService_ExistingService_ReturnsUpdatedService()
    {
        // Arrange
        var serviceId = "payment-service";
        var updateRequest = new UpdateServiceRequest
        {
            Name = "Updated Payment Service",
            Description = "Updated description for payment service",
            Tags = new List<string> { "payment", "billing", "updated" }
        };

        var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/services/{serviceId}", content);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var updatedService = JsonSerializer.Deserialize<ServiceModel>(responseContent, _jsonOptions);
        
        Assert.NotNull(updatedService);
        Assert.Equal(serviceId, updatedService.Id);
        Assert.Equal(updateRequest.Name, updatedService.Name);
        Assert.Equal(updateRequest.Description, updatedService.Description);
    }

    [Fact]
    public async Task PutService_NonExistentService_ReturnsNotFound()
    {
        // Arrange
        var serviceId = "non-existent-service";
        var updateRequest = new UpdateServiceRequest
        {
            Name = "Updated Service"
        };

        var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/services/{serviceId}", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteService_ExistingService_ReturnsNoContent()
    {
        // Arrange
        var serviceId = "test-service-to-delete";

        // Act
        var response = await _client.DeleteAsync($"/api/services/{serviceId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteService_NonExistentService_ReturnsNotFound()
    {
        // Arrange
        var serviceId = "non-existent-service";

        // Act
        var response = await _client.DeleteAsync($"/api/services/{serviceId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetServiceDependencies_ExistingService_ReturnsDependencyGraph()
    {
        // Arrange
        var serviceId = "payment-service";

        // Act
        var response = await _client.GetAsync($"/api/services/{serviceId}/dependencies");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var dependencies = JsonSerializer.Deserialize<ServiceDependencies>(content, _jsonOptions);
        
        Assert.NotNull(dependencies);
        Assert.Equal(serviceId, dependencies.ServiceId);
        Assert.NotNull(dependencies.Dependencies);
        Assert.NotNull(dependencies.Dependents);
    }
}

// Data models for contract testing - these MUST match the OpenAPI specification
public class ServiceModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string? Repository { get; set; }
    public string? Documentation { get; set; }
    public string? ApiSpec { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Lifecycle { get; set; } = string.Empty;
    public string? System { get; set; }
    public List<string> DependsOn { get; set; } = new();
    public List<string> ProvidesApis { get; set; } = new();
    public List<string> ConsumesApis { get; set; } = new();
    public ServiceMetadata Metadata { get; set; } = new();
}

public class ServiceMetadata
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Version { get; set; }
    public string? HealthCheckUrl { get; set; }
}

public class CreateServiceRequest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string? Repository { get; set; }
    public string? Documentation { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Lifecycle { get; set; } = string.Empty;
    public string? System { get; set; }
}

public class UpdateServiceRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Owner { get; set; }
    public string? Repository { get; set; }
    public string? Documentation { get; set; }
    public List<string>? Tags { get; set; }
    public string? Lifecycle { get; set; }
}

public class ServiceDependencies
{
    public string ServiceId { get; set; } = string.Empty;
    public List<ServiceModel> Dependencies { get; set; } = new();
    public List<ServiceModel> Dependents { get; set; } = new();
}