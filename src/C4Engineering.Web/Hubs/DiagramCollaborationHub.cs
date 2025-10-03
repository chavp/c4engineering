using Microsoft.AspNetCore.SignalR;
using C4Engineering.Web.Services;
using C4Engineering.Web.Models.Architecture;

namespace C4Engineering.Web.Hubs;

/// <summary>
/// SignalR hub for collaborative diagram editing
/// </summary>
public class DiagramCollaborationHub : Hub
{
    private readonly IDiagramService _diagramService;
    private readonly ILogger<DiagramCollaborationHub> _logger;

    public DiagramCollaborationHub(IDiagramService diagramService, ILogger<DiagramCollaborationHub> logger)
    {
        _diagramService = diagramService;
        _logger = logger;
    }

    public async Task JoinDiagram(string diagramId)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"diagram-{diagramId}");
            await Clients.Group($"diagram-{diagramId}").SendAsync("UserJoined", Context.UserIdentifier ?? "Anonymous");
            
            _logger.LogDebug("User {UserId} joined diagram {DiagramId}", Context.UserIdentifier, diagramId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining diagram {DiagramId}", diagramId);
            throw;
        }
    }

    public async Task LeaveDiagram(string diagramId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"diagram-{diagramId}");
            await Clients.Group($"diagram-{diagramId}").SendAsync("UserLeft", Context.UserIdentifier ?? "Anonymous");
            
            _logger.LogDebug("User {UserId} left diagram {DiagramId}", Context.UserIdentifier, diagramId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving diagram {DiagramId}", diagramId);
            throw;
        }
    }

    public async Task UpdateElement(string diagramId, DiagramElement element)
    {
        try
        {
            await _diagramService.UpdateElementAsync(diagramId, element);
            await Clients.OthersInGroup($"diagram-{diagramId}")
                .SendAsync("ElementUpdated", element);
            
            _logger.LogDebug("Element {ElementId} updated in diagram {DiagramId}", element.Id, diagramId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating element {ElementId} in diagram {DiagramId}", element.Id, diagramId);
            throw;
        }
    }

    public async Task AddElement(string diagramId, DiagramElement element)
    {
        try
        {
            await _diagramService.AddElementAsync(diagramId, element);
            await Clients.OthersInGroup($"diagram-{diagramId}")
                .SendAsync("ElementAdded", element);
            
            _logger.LogDebug("Element {ElementId} added to diagram {DiagramId}", element.Id, diagramId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding element {ElementId} to diagram {DiagramId}", element.Id, diagramId);
            throw;
        }
    }

    public async Task RemoveElement(string diagramId, string elementId)
    {
        try
        {
            await _diagramService.RemoveElementAsync(diagramId, elementId);
            await Clients.OthersInGroup($"diagram-{diagramId}")
                .SendAsync("ElementRemoved", elementId);
            
            _logger.LogDebug("Element {ElementId} removed from diagram {DiagramId}", elementId, diagramId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing element {ElementId} from diagram {DiagramId}", elementId, diagramId);
            throw;
        }
    }

    public async Task AddRelationship(string diagramId, DiagramRelationship relationship)
    {
        try
        {
            await _diagramService.AddRelationshipAsync(diagramId, relationship);
            await Clients.OthersInGroup($"diagram-{diagramId}")
                .SendAsync("RelationshipAdded", relationship);
            
            _logger.LogDebug("Relationship {RelationshipId} added to diagram {DiagramId}", relationship.Id, diagramId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding relationship {RelationshipId} to diagram {DiagramId}", relationship.Id, diagramId);
            throw;
        }
    }

    public async Task RemoveRelationship(string diagramId, string relationshipId)
    {
        try
        {
            await _diagramService.RemoveRelationshipAsync(diagramId, relationshipId);
            await Clients.OthersInGroup($"diagram-{diagramId}")
                .SendAsync("RelationshipRemoved", relationshipId);
            
            _logger.LogDebug("Relationship {RelationshipId} removed from diagram {DiagramId}", relationshipId, diagramId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing relationship {RelationshipId} from diagram {DiagramId}", relationshipId, diagramId);
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogDebug("User {UserId} disconnected", Context.UserIdentifier);
        await base.OnDisconnectedAsync(exception);
    }
}