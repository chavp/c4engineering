using C4Engineering.Web.Models.Architecture;

namespace C4Engineering.Web.Data.Repositories;

/// <summary>
/// Repository interface for diagram operations
/// </summary>
public interface IDiagramRepository
{
    Task<IEnumerable<DiagramModel>> GetAllAsync();
    Task<DiagramModel?> GetByIdAsync(string id);
    Task<DiagramModel> CreateAsync(DiagramModel diagram);
    Task<DiagramModel> UpdateAsync(DiagramModel diagram);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<DiagramModel>> FindByTypeAsync(DiagramType type);
    Task<IEnumerable<DiagramModel>> FindBySystemAsync(string system);
    Task<bool> ExistsAsync(string id);
    Task<DiagramModel> AddElementAsync(string diagramId, DiagramElement element);
    Task<DiagramModel> UpdateElementAsync(string diagramId, string elementId, DiagramElement element);
    Task<DiagramModel> RemoveElementAsync(string diagramId, string elementId);
    Task<DiagramModel> AddRelationshipAsync(string diagramId, DiagramRelationship relationship);
    Task<DiagramModel> RemoveRelationshipAsync(string diagramId, string relationshipId);
}