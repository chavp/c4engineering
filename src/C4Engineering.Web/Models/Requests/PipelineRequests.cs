using System.ComponentModel.DataAnnotations;
using C4Engineering.Web.Models.Pipeline;

namespace C4Engineering.Web.Models.Requests;

/// <summary>
/// Request model for creating a new pipeline
/// </summary>
public record CreatePipelineRequest
{
    public string? Id { get; init; }
    
    [Required]
    public string ServiceId { get; init; } = string.Empty;
    
    [Required]
    public string Name { get; init; } = string.Empty;
    
    public string? Description { get; init; }
    public List<PipelineStage>? Stages { get; init; }
    public Dictionary<string, string>? Environment { get; init; }
    public PipelineTriggers? Triggers { get; init; }
}

/// <summary>
/// Request model for updating an existing pipeline
/// </summary>
public record UpdatePipelineRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public List<PipelineStage>? Stages { get; init; }
    public Dictionary<string, string>? Environment { get; init; }
    public PipelineTriggers? Triggers { get; init; }
}

/// <summary>
/// Request model for executing a pipeline
/// </summary>
public record ExecutePipelineRequest
{
    public Dictionary<string, string>? Parameters { get; init; }
    public string? TriggeredBy { get; init; }
    public string? Reason { get; init; }
}