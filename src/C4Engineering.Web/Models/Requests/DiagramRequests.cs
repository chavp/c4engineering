using System.ComponentModel.DataAnnotations;
using C4Engineering.Web.Models.Architecture;

namespace C4Engineering.Web.Models.Requests;

/// <summary>
/// Request model for creating a new diagram
/// </summary>
public record CreateDiagramRequest
{
    public string? Id { get; init; }
    
    [Required]
    public string Name { get; init; } = string.Empty;
    
    [Required]
    public string Type { get; init; } = string.Empty;
    
    public string? System { get; init; }
    public string? Description { get; init; }
    public string? ParentDiagram { get; init; }
}

/// <summary>
/// Request model for updating an existing diagram
/// </summary>
public record UpdateDiagramRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public List<DiagramElement>? Elements { get; init; }
    public List<DiagramRelationship>? Relationships { get; init; }
}

/// <summary>
/// Request model for updating a diagram element
/// </summary>
public record UpdateElementRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Technology { get; init; }
    public Position? Position { get; init; }
    public Size? Size { get; init; }
    public ElementStyle? Style { get; init; }
}