using Microsoft.AspNetCore.Mvc;
using C4Engineering.Web.Models.Architecture;
using C4Engineering.Web.Models.Requests;
using C4Engineering.Web.Data.Repositories;
using DiagramSummary = C4Engineering.Web.Models.Architecture.DiagramSummary;
using CreateDiagramRequest = C4Engineering.Web.Models.Requests.CreateDiagramRequest;
using UpdateDiagramRequest = C4Engineering.Web.Models.Requests.UpdateDiagramRequest;
using UpdateElementRequest = C4Engineering.Web.Models.Requests.UpdateElementRequest;

namespace C4Engineering.Web.Controllers;

/// <summary>
/// REST API controller for C4 diagram operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DiagramsController : ControllerBase
{
    private readonly IDiagramRepository _diagramRepository;
    private readonly ILogger<DiagramsController> _logger;

    public DiagramsController(IDiagramRepository diagramRepository, ILogger<DiagramsController> logger)
    {
        _diagramRepository = diagramRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all diagrams with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DiagramSummary>), 200)]
    public async Task<ActionResult<IEnumerable<DiagramSummary>>> GetDiagrams(
        [FromQuery] string? type = null,
        [FromQuery] string? system = null)
    {
        try
        {
            var diagrams = await _diagramRepository.GetAllAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(type) && Enum.TryParse<DiagramType>(type, true, out var parsedType))
            {
                diagrams = diagrams.Where(d => d.Type == parsedType);
            }

            if (!string.IsNullOrEmpty(system))
            {
                diagrams = diagrams.Where(d => system.Equals(d.System, StringComparison.OrdinalIgnoreCase));
            }

            // Convert to summary format
            var summaries = diagrams.Select(d => new DiagramSummary
            {
                Id = d.Id,
                Name = d.Name,
                Type = d.Type.ToString().ToLowerInvariant(),
                System = d.System,
                Description = d.Description,
                ElementCount = d.Elements.Count,
                RelationshipCount = d.Relationships.Count,
                UpdatedAt = d.Metadata.UpdatedAt
            });

            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve diagrams");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get diagram by ID
    /// </summary>
    [HttpGet("{diagramId}")]
    [ProducesResponseType(typeof(DiagramModel), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<DiagramModel>> GetDiagram(string diagramId)
    {
        try
        {
            var diagram = await _diagramRepository.GetByIdAsync(diagramId);
            
            if (diagram == null)
            {
                return NotFound();
            }

            return Ok(diagram);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve diagram {DiagramId}", diagramId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new diagram
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DiagramModel), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<DiagramModel>> CreateDiagram([FromBody] CreateDiagramRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Parse diagram type
            if (!Enum.TryParse<DiagramType>(request.Type, true, out var diagramType))
            {
                return BadRequest(new { error = "Invalid diagram type" });
            }

            // Generate ID if not provided
            var diagramId = !string.IsNullOrEmpty(request.Id) ? request.Id : 
                $"{request.System ?? "system"}-{diagramType.ToString().ToLowerInvariant()}-{Guid.NewGuid().ToString()[..8]}";

            var diagram = new DiagramModel
            {
                Id = diagramId,
                Name = request.Name,
                Type = diagramType,
                System = request.System,
                Description = request.Description,
                Elements = new List<DiagramElement>(),
                Relationships = new List<DiagramRelationship>(),
                Metadata = new DiagramMetadata
                {
                    ParentDiagram = request.ParentDiagram
                }
            };

            var createdDiagram = await _diagramRepository.CreateAsync(diagram);
            
            return CreatedAtAction(nameof(GetDiagram), new { diagramId = createdDiagram.Id }, createdDiagram);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create diagram");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update an existing diagram
    /// </summary>
    [HttpPut("{diagramId}")]
    [ProducesResponseType(typeof(DiagramModel), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<DiagramModel>> UpdateDiagram(string diagramId, [FromBody] UpdateDiagramRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var existingDiagram = await _diagramRepository.GetByIdAsync(diagramId);
            if (existingDiagram == null)
            {
                return NotFound();
            }

            var updatedDiagram = existingDiagram with
            {
                Name = request.Name ?? existingDiagram.Name,
                Description = request.Description ?? existingDiagram.Description,
                Elements = request.Elements ?? existingDiagram.Elements,
                Relationships = request.Relationships ?? existingDiagram.Relationships
            };

            var result = await _diagramRepository.UpdateAsync(updatedDiagram);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist"))
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update diagram {DiagramId}", diagramId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete a diagram
    /// </summary>
    [HttpDelete("{diagramId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteDiagram(string diagramId)
    {
        try
        {
            var deleted = await _diagramRepository.DeleteAsync(diagramId);
            
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete diagram {DiagramId}", diagramId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Add element to diagram
    /// </summary>
    [HttpPost("{diagramId}/elements")]
    [ProducesResponseType(typeof(DiagramElement), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<DiagramElement>> AddElement(string diagramId, [FromBody] DiagramElement element)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _diagramRepository.AddElementAsync(diagramId, element);
            return CreatedAtAction(nameof(GetDiagram), new { diagramId }, element);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist"))
        {
            return NotFound();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add element to diagram {DiagramId}", diagramId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update element in diagram
    /// </summary>
    [HttpPut("{diagramId}/elements/{elementId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateElement(string diagramId, string elementId, [FromBody] UpdateElementRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var diagram = await _diagramRepository.GetByIdAsync(diagramId);
            if (diagram == null)
            {
                return NotFound();
            }

            var existingElement = diagram.Elements.FirstOrDefault(e => e.Id == elementId);
            if (existingElement == null)
            {
                return NotFound();
            }

            var updatedElement = existingElement with
            {
                Name = request.Name ?? existingElement.Name,
                Description = request.Description ?? existingElement.Description,
                Technology = request.Technology ?? existingElement.Technology,
                Position = request.Position ?? existingElement.Position,
                Size = request.Size ?? existingElement.Size,
                Style = request.Style ?? existingElement.Style
            };

            await _diagramRepository.UpdateElementAsync(diagramId, elementId, updatedElement);
            return Ok();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist"))
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update element {ElementId} in diagram {DiagramId}", elementId, diagramId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete element from diagram
    /// </summary>
    [HttpDelete("{diagramId}/elements/{elementId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteElement(string diagramId, string elementId)
    {
        try
        {
            await _diagramRepository.RemoveElementAsync(diagramId, elementId);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist"))
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete element {ElementId} from diagram {DiagramId}", elementId, diagramId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Add relationship to diagram
    /// </summary>
    [HttpPost("{diagramId}/relationships")]
    [ProducesResponseType(typeof(DiagramRelationship), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<DiagramRelationship>> AddRelationship(string diagramId, [FromBody] DiagramRelationship relationship)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _diagramRepository.AddRelationshipAsync(diagramId, relationship);
            return CreatedAtAction(nameof(GetDiagram), new { diagramId }, relationship);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist"))
        {
            return NotFound();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add relationship to diagram {DiagramId}", diagramId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Export diagram in specified format
    /// </summary>
    [HttpGet("{diagramId}/export")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ExportDiagram(string diagramId, [FromQuery] string format = "png")
    {
        try
        {
            var diagram = await _diagramRepository.GetByIdAsync(diagramId);
            if (diagram == null)
            {
                return NotFound();
            }

            // For MVP, return placeholder content based on format
            var contentType = format.ToLowerInvariant() switch
            {
                "png" => "image/png",
                "svg" => "image/svg+xml",
                "pdf" => "application/pdf",
                _ => throw new ArgumentException("Unsupported format")
            };

            // Generate placeholder content (in real implementation, this would use a diagram rendering engine)
            var placeholderContent = System.Text.Encoding.UTF8.GetBytes($"Placeholder {format.ToUpperInvariant()} export for diagram: {diagram.Name}");
            
            return File(placeholderContent, contentType, $"{diagramId}.{format}");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export diagram {DiagramId}", diagramId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}