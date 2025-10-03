namespace C4Engineering.Web.Models.Architecture;

/// <summary>
/// Represents a C4 model diagram with elements and relationships
/// </summary>
public record DiagramModel
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

public record DiagramElement
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

public record DiagramRelationship
{
    public string Id { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Technology { get; init; }
    public RelationshipStyle Style { get; init; } = new();
}

public record Position
{
    public double X { get; init; }
    public double Y { get; init; }
}

public record Size
{
    public double Width { get; init; } = 120;
    public double Height { get; init; } = 80;
}

public record ElementStyle
{
    public string? BackgroundColor { get; init; }
    public string? Color { get; init; }
    public string? BorderColor { get; init; }
    public double? BorderWidth { get; init; }
    public string? Shape { get; init; }
}

public record RelationshipStyle
{
    public string? LineStyle { get; init; }
    public string? ArrowStyle { get; init; }
    public string? Color { get; init; }
    public double? Width { get; init; }
}

public record DiagramMetadata
{
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
    public string? CreatedBy { get; init; }
    public string? Version { get; init; } = "1.0";
    public string? ParentDiagram { get; init; }
    public List<string> ChildDiagrams { get; init; } = new();
}

public enum DiagramType
{
    Context,
    Container,
    Component,
    Code
}

public enum ElementType
{
    Person,
    System,
    ExternalSystem,
    Container,
    Component,
    CodeElement
}

// DTOs for API requests and responses
public record CreateDiagramRequest(
    string? Id,
    string Name,
    string Type,
    string? System = null,
    string? Description = null,
    string? ParentDiagram = null);

public record UpdateDiagramRequest(
    string? Name = null,
    string? Description = null,
    List<DiagramElement>? Elements = null,
    List<DiagramRelationship>? Relationships = null);

public record UpdateElementRequest(
    string? Name = null,
    string? Description = null,
    string? Technology = null,
    Position? Position = null,
    Size? Size = null,
    ElementStyle? Style = null);

public class DiagramSummary
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string? System { get; init; }
    public string? Description { get; init; }
    public int ElementCount { get; init; }
    public int RelationshipCount { get; init; }
    public DateTime UpdatedAt { get; init; }
}