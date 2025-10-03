using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace C4Engineering.Tests.Contract;

/// <summary>
/// Contract tests for Diagram API endpoints based on OpenAPI specification
/// These tests MUST FAIL initially to enforce TDD methodology
/// </summary>
public class DiagramApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public DiagramApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task GetDiagrams_ReturnsSuccessStatusCode()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/diagrams");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetDiagrams_WithTypeFilter_ReturnsFilteredResults()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/diagrams?type=context");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var diagrams = JsonSerializer.Deserialize<DiagramSummary[]>(content, _jsonOptions);
        
        Assert.NotNull(diagrams);
        Assert.All(diagrams, diagram => Assert.Equal("context", diagram.Type));
    }

    [Fact]
    public async Task GetDiagrams_WithSystemFilter_ReturnsFilteredResults()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/diagrams?system=payment-system");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var diagrams = JsonSerializer.Deserialize<DiagramSummary[]>(content, _jsonOptions);
        
        Assert.NotNull(diagrams);
        Assert.All(diagrams, diagram => Assert.Equal("payment-system", diagram.System));
    }

    [Fact]
    public async Task GetDiagramById_ExistingDiagram_ReturnsDiagram()
    {
        // Arrange
        var diagramId = "payment-system-context";

        // Act
        var response = await _client.GetAsync($"/api/diagrams/{diagramId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var diagram = JsonSerializer.Deserialize<Diagram>(content, _jsonOptions);
        
        Assert.NotNull(diagram);
        Assert.Equal(diagramId, diagram.Id);
        Assert.NotNull(diagram.Elements);
        Assert.NotNull(diagram.Relationships);
    }

    [Fact]
    public async Task GetDiagramById_NonExistentDiagram_ReturnsNotFound()
    {
        // Arrange
        var diagramId = "non-existent-diagram";

        // Act
        var response = await _client.GetAsync($"/api/diagrams/{diagramId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostDiagram_ValidDiagram_ReturnsCreatedDiagram()
    {
        // Arrange
        var newDiagram = new CreateDiagramRequest
        {
            Id = "test-diagram",
            Name = "Test System Context",
            Type = "context",
            System = "test-system",
            Description = "A test diagram for contract testing"
        };

        var json = JsonSerializer.Serialize(newDiagram, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/diagrams", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdDiagram = JsonSerializer.Deserialize<Diagram>(responseContent, _jsonOptions);
        
        Assert.NotNull(createdDiagram);
        Assert.Equal(newDiagram.Id, createdDiagram.Id);
        Assert.Equal(newDiagram.Name, createdDiagram.Name);
        Assert.Equal(newDiagram.Type, createdDiagram.Type);
    }

    [Fact]
    public async Task PutDiagram_ExistingDiagram_ReturnsUpdatedDiagram()
    {
        // Arrange
        var diagramId = "payment-system-context";
        var updateRequest = new UpdateDiagramRequest
        {
            Name = "Updated Payment System Context",
            Description = "Updated description for payment system context diagram"
        };

        var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/diagrams/{diagramId}", content);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var updatedDiagram = JsonSerializer.Deserialize<Diagram>(responseContent, _jsonOptions);
        
        Assert.NotNull(updatedDiagram);
        Assert.Equal(diagramId, updatedDiagram.Id);
        Assert.Equal(updateRequest.Name, updatedDiagram.Name);
    }

    [Fact]
    public async Task DeleteDiagram_ExistingDiagram_ReturnsNoContent()
    {
        // Arrange
        var diagramId = "test-diagram-to-delete";

        // Act
        var response = await _client.DeleteAsync($"/api/diagrams/{diagramId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task PostDiagramElement_ValidElement_ReturnsCreatedElement()
    {
        // Arrange
        var diagramId = "payment-system-context";
        var newElement = new DiagramElement
        {
            Id = "test-element",
            Type = "system",
            Name = "Test System",
            Description = "A test system element",
            Position = new Position { X = 100, Y = 100 },
            Size = new Size { Width = 120, Height = 80 }
        };

        var json = JsonSerializer.Serialize(newElement, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/diagrams/{diagramId}/elements", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdElement = JsonSerializer.Deserialize<DiagramElement>(responseContent, _jsonOptions);
        
        Assert.NotNull(createdElement);
        Assert.Equal(newElement.Id, createdElement.Id);
        Assert.Equal(newElement.Name, createdElement.Name);
    }

    [Fact]
    public async Task PutDiagramElement_ExistingElement_ReturnsSuccess()
    {
        // Arrange
        var diagramId = "payment-system-context";
        var elementId = "payment-system";
        var updateRequest = new UpdateElementRequest
        {
            Name = "Updated Payment System",
            Description = "Updated payment system description",
            Position = new Position { X = 200, Y = 150 }
        };

        var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/diagrams/{diagramId}/elements/{elementId}", content);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DeleteDiagramElement_ExistingElement_ReturnsNoContent()
    {
        // Arrange
        var diagramId = "payment-system-context";
        var elementId = "test-element";

        // Act
        var response = await _client.DeleteAsync($"/api/diagrams/{diagramId}/elements/{elementId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task PostDiagramRelationship_ValidRelationship_ReturnsCreated()
    {
        // Arrange
        var diagramId = "payment-system-context";
        var newRelationship = new DiagramRelationship
        {
            Id = "test-relationship",
            SourceId = "customer",
            TargetId = "payment-system",
            Description = "Makes payments using",
            Technology = "HTTPS/REST"
        };

        var json = JsonSerializer.Serialize(newRelationship, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/diagrams/{diagramId}/relationships", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetDiagramExport_PngFormat_ReturnsPngImage()
    {
        // Arrange
        var diagramId = "payment-system-context";

        // Act
        var response = await _client.GetAsync($"/api/diagrams/{diagramId}/export?format=png");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetDiagramExport_SvgFormat_ReturnsSvgImage()
    {
        // Arrange
        var diagramId = "payment-system-context";

        // Act
        var response = await _client.GetAsync($"/api/diagrams/{diagramId}/export?format=svg");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("image/svg+xml", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetDiagramExport_PdfFormat_ReturnsPdfDocument()
    {
        // Arrange
        var diagramId = "payment-system-context";

        // Act
        var response = await _client.GetAsync($"/api/diagrams/{diagramId}/export?format=pdf");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
    }
}

// Data models for contract testing - these MUST match the OpenAPI specification
public class Diagram
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? System { get; set; }
    public string? Description { get; set; }
    public List<DiagramElement> Elements { get; set; } = new();
    public List<DiagramRelationship> Relationships { get; set; } = new();
    public DiagramMetadata Metadata { get; set; } = new();
}

public class DiagramSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? System { get; set; }
    public string? Description { get; set; }
    public int ElementCount { get; set; }
    public int RelationshipCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DiagramElement
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Technology { get; set; }
    public Position? Position { get; set; }
    public Size? Size { get; set; }
    public ElementStyle? Style { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class DiagramRelationship
{
    public string Id { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Technology { get; set; }
    public RelationshipStyle? Style { get; set; }
}

public class Position
{
    public double X { get; set; }
    public double Y { get; set; }
}

public class Size
{
    public double Width { get; set; }
    public double Height { get; set; }
}

public class ElementStyle
{
    public string? BackgroundColor { get; set; }
    public string? Color { get; set; }
    public string? BorderColor { get; set; }
    public double? BorderWidth { get; set; }
    public string? Shape { get; set; }
}

public class RelationshipStyle
{
    public string? LineStyle { get; set; }
    public string? ArrowStyle { get; set; }
    public string? Color { get; set; }
    public double? Width { get; set; }
}

public class DiagramMetadata
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? Version { get; set; }
    public string? ParentDiagram { get; set; }
    public List<string> ChildDiagrams { get; set; } = new();
}

public class CreateDiagramRequest
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? System { get; set; }
    public string? Description { get; set; }
    public string? ParentDiagram { get; set; }
}

public class UpdateDiagramRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<DiagramElement>? Elements { get; set; }
    public List<DiagramRelationship>? Relationships { get; set; }
}

public class UpdateElementRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Technology { get; set; }
    public Position? Position { get; set; }
    public Size? Size { get; set; }
    public ElementStyle? Style { get; set; }
}