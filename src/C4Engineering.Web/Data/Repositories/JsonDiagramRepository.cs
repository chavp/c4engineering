using C4Engineering.Web.Models.Architecture;
using Microsoft.Extensions.Options;

namespace C4Engineering.Web.Data.Repositories;

/// <summary>
/// JSON file-based implementation of diagram repository
/// </summary>
public class JsonDiagramRepository : JsonDataStore<DiagramModel>, IDiagramRepository
{
    public JsonDiagramRepository(IOptions<JsonDataOptions> options, ILogger<JsonDiagramRepository> logger)
        : base(options, logger, "diagrams")
    {
    }

    public async Task<IEnumerable<DiagramModel>> GetAllAsync()
    {
        return await ReadAllEntitiesAsync();
    }

    public async Task<DiagramModel?> GetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Diagram ID cannot be null or empty", nameof(id));

        return await ReadEntityAsync(id);
    }

    public async Task<DiagramModel> CreateAsync(DiagramModel diagram)
    {
        if (diagram == null)
            throw new ArgumentNullException(nameof(diagram));

        if (string.IsNullOrEmpty(diagram.Id))
            throw new ArgumentException("Diagram ID cannot be null or empty", nameof(diagram));

        if (await EntityExistsAsync(diagram.Id))
            throw new InvalidOperationException($"Diagram with ID '{diagram.Id}' already exists");

        // Set metadata
        var diagramWithMetadata = diagram with
        {
            Metadata = diagram.Metadata with
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        return await WriteEntityAsync(diagram.Id, diagramWithMetadata);
    }

    public async Task<DiagramModel> UpdateAsync(DiagramModel diagram)
    {
        if (diagram == null)
            throw new ArgumentNullException(nameof(diagram));

        if (string.IsNullOrEmpty(diagram.Id))
            throw new ArgumentException("Diagram ID cannot be null or empty", nameof(diagram));

        if (!await EntityExistsAsync(diagram.Id))
            throw new InvalidOperationException($"Diagram with ID '{diagram.Id}' does not exist");

        // Update metadata
        var diagramWithMetadata = diagram with
        {
            Metadata = diagram.Metadata with
            {
                UpdatedAt = DateTime.UtcNow
            }
        };

        return await WriteEntityAsync(diagram.Id, diagramWithMetadata);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Diagram ID cannot be null or empty", nameof(id));

        return await DeleteEntityAsync(id);
    }

    public async Task<IEnumerable<DiagramModel>> FindByTypeAsync(DiagramType type)
    {
        return await FilterEntitiesAsync(diagram => diagram.Type == type);
    }

    public async Task<IEnumerable<DiagramModel>> FindBySystemAsync(string system)
    {
        if (string.IsNullOrEmpty(system))
            return Enumerable.Empty<DiagramModel>();

        return await FilterEntitiesAsync(diagram => 
            system.Equals(diagram.System, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> ExistsAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        return await EntityExistsAsync(id);
    }

    public async Task<DiagramModel> AddElementAsync(string diagramId, DiagramElement element)
    {
        var diagram = await GetByIdAsync(diagramId);
        if (diagram == null)
            throw new InvalidOperationException($"Diagram with ID '{diagramId}' does not exist");

        if (diagram.Elements.Any(e => e.Id == element.Id))
            throw new InvalidOperationException($"Element with ID '{element.Id}' already exists in diagram");

        var updatedElements = new List<DiagramElement>(diagram.Elements) { element };
        var updatedDiagram = diagram with { Elements = updatedElements };

        return await UpdateAsync(updatedDiagram);
    }

    public async Task<DiagramModel> UpdateElementAsync(string diagramId, string elementId, DiagramElement element)
    {
        var diagram = await GetByIdAsync(diagramId);
        if (diagram == null)
            throw new InvalidOperationException($"Diagram with ID '{diagramId}' does not exist");

        var elementIndex = diagram.Elements.FindIndex(e => e.Id == elementId);
        if (elementIndex == -1)
            throw new InvalidOperationException($"Element with ID '{elementId}' does not exist in diagram");

        var updatedElements = new List<DiagramElement>(diagram.Elements);
        updatedElements[elementIndex] = element with { Id = elementId };
        var updatedDiagram = diagram with { Elements = updatedElements };

        return await UpdateAsync(updatedDiagram);
    }

    public async Task<DiagramModel> RemoveElementAsync(string diagramId, string elementId)
    {
        var diagram = await GetByIdAsync(diagramId);
        if (diagram == null)
            throw new InvalidOperationException($"Diagram with ID '{diagramId}' does not exist");

        var updatedElements = diagram.Elements.Where(e => e.Id != elementId).ToList();
        if (updatedElements.Count == diagram.Elements.Count)
            throw new InvalidOperationException($"Element with ID '{elementId}' does not exist in diagram");

        // Also remove any relationships involving this element
        var updatedRelationships = diagram.Relationships
            .Where(r => r.SourceId != elementId && r.TargetId != elementId)
            .ToList();

        var updatedDiagram = diagram with 
        { 
            Elements = updatedElements,
            Relationships = updatedRelationships
        };

        return await UpdateAsync(updatedDiagram);
    }

    public async Task<DiagramModel> AddRelationshipAsync(string diagramId, DiagramRelationship relationship)
    {
        var diagram = await GetByIdAsync(diagramId);
        if (diagram == null)
            throw new InvalidOperationException($"Diagram with ID '{diagramId}' does not exist");

        if (diagram.Relationships.Any(r => r.Id == relationship.Id))
            throw new InvalidOperationException($"Relationship with ID '{relationship.Id}' already exists in diagram");

        // Verify source and target elements exist
        if (!diagram.Elements.Any(e => e.Id == relationship.SourceId))
            throw new InvalidOperationException($"Source element '{relationship.SourceId}' does not exist in diagram");

        if (!diagram.Elements.Any(e => e.Id == relationship.TargetId))
            throw new InvalidOperationException($"Target element '{relationship.TargetId}' does not exist in diagram");

        var updatedRelationships = new List<DiagramRelationship>(diagram.Relationships) { relationship };
        var updatedDiagram = diagram with { Relationships = updatedRelationships };

        return await UpdateAsync(updatedDiagram);
    }

    public async Task<DiagramModel> RemoveRelationshipAsync(string diagramId, string relationshipId)
    {
        var diagram = await GetByIdAsync(diagramId);
        if (diagram == null)
            throw new InvalidOperationException($"Diagram with ID '{diagramId}' does not exist");

        var updatedRelationships = diagram.Relationships.Where(r => r.Id != relationshipId).ToList();
        if (updatedRelationships.Count == diagram.Relationships.Count)
            throw new InvalidOperationException($"Relationship with ID '{relationshipId}' does not exist in diagram");

        var updatedDiagram = diagram with { Relationships = updatedRelationships };

        return await UpdateAsync(updatedDiagram);
    }
}