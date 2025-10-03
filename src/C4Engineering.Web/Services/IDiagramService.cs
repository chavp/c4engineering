using C4Engineering.Web.Models.Architecture;

namespace C4Engineering.Web.Services;

/// <summary>
/// Service interface for managing C4 architecture diagrams
/// </summary>
public interface IDiagramService
{
    Task<IEnumerable<DiagramModel>> GetAllDiagramsAsync();
    Task<DiagramModel?> GetDiagramByIdAsync(string id);
    Task<DiagramModel> CreateDiagramAsync(DiagramModel diagram);
    Task<DiagramModel> UpdateDiagramAsync(DiagramModel diagram);
    Task<bool> DeleteDiagramAsync(string id);
    Task<IEnumerable<DiagramModel>> GetDiagramsByTypeAsync(DiagramType type);
    Task<IEnumerable<DiagramModel>> GetDiagramsBySystemAsync(string system);
    Task<DiagramModel> GenerateDiagramFromServiceAsync(string serviceId, DiagramType type);
    Task<DiagramElement> AddElementAsync(string diagramId, DiagramElement element);
    Task<DiagramRelationship> AddRelationshipAsync(string diagramId, DiagramRelationship relationship);
    Task<DiagramElement> UpdateElementAsync(string diagramId, DiagramElement element);
    Task<bool> DeleteElementAsync(string diagramId, string elementId);
    Task<bool> RemoveElementAsync(string diagramId, string elementId);
    Task<bool> RemoveRelationshipAsync(string diagramId, string relationshipId);
    Task<bool> ExistsAsync(string id);
}