namespace C4Engineering.Web.Models.Pipeline;

/// <summary>
/// Represents a build and deployment pipeline configuration
/// </summary>
public record PipelineConfiguration
{
    public string Id { get; init; } = string.Empty;
    public string ServiceId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<PipelineStage> Stages { get; init; } = new();
    public PipelineTriggers Triggers { get; init; } = new();
    public PipelineMetadata Metadata { get; init; } = new();
    public Dictionary<string, string> Environment { get; init; } = new();
}

public class PipelineStage
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public StageType Type { get; init; }
    public List<string> Commands { get; init; } = new();
    public List<PipelineStep> Steps { get; init; } = new();
    public string? WorkingDirectory { get; init; }
    public string? DockerFile { get; init; }
    public string? ImageName { get; init; }
    public string? ImageTag { get; init; }
    public string? ContainerName { get; init; }
    public List<string> Ports { get; init; } = new();
    public Dictionary<string, string> Environment { get; init; } = new();
    public List<string> Volumes { get; init; } = new();
    public Dictionary<string, string> BuildArgs { get; init; } = new();
    public HealthCheck? HealthCheck { get; init; }
    public int Timeout { get; init; } = 300;
    public int RetryCount { get; init; } = 0;
}

public class PipelineTriggers
{
    public bool Manual { get; init; } = true;
    public bool OnDiagramChange { get; init; } = false;
    public bool OnRepositoryChange { get; init; } = false;
    public string? Scheduled { get; init; }
}

public record PipelineExecution
{
    public string Id { get; init; } = string.Empty;
    public string PipelineId { get; init; } = string.Empty;
    public ExecutionStatus Status { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int? Duration { get; init; }
    public string? TriggeredBy { get; init; }
    public int BuildNumber { get; init; }
    public List<StageExecution> Stages { get; init; } = new();
    public List<StageExecution> StageExecutions { get; init; } = new();
    public Dictionary<string, string> Parameters { get; init; } = new();
    public List<ExecutionLogEntry> Logs { get; init; } = new();
}

public class StageExecution
{
    public string StageId { get; init; } = string.Empty;
    public string StageName { get; init; } = string.Empty;
    public ExecutionStatus Status { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int? Duration { get; init; }
    public List<string> Logs { get; init; } = new();
    public List<string> Artifacts { get; init; } = new();
    public List<StepExecution> Steps { get; init; } = new();
    public string? DeploymentUrl { get; init; }
}

public class HealthCheck
{
    public string? Url { get; init; }
    public int Interval { get; init; } = 10;
    public int Timeout { get; init; } = 5;
    public int Retries { get; init; } = 3;
}

public class LogEntry
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public LogLevel Level { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? StageId { get; init; }
    public string? Stage { get; init; }
}

public class ExecutionLogEntry
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public LogLevel Level { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? StageId { get; init; }
    public string? Stage { get; init; }
}

public class PipelineStep
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Command { get; init; } = string.Empty;
    public Dictionary<string, string> Environment { get; init; } = new();
    public int Timeout { get; init; } = 300;
}

public class StepExecution
{
    public string StepId { get; init; } = string.Empty;
    public string StepName { get; init; } = string.Empty;
    public ExecutionStatus Status { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int? Duration { get; init; }
    public List<string> Logs { get; init; } = new();
}

public record PipelineMetadata
{
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
    public string? CreatedBy { get; init; }
}

public enum StageType
{
    Build,
    Test,
    DockerBuild,
    DockerDeploy,
    Custom
}

public enum ExecutionStatus
{
    Queued,
    Running,
    Success,
    Failed,
    Cancelled,
    Skipped,
    Pending
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

// DTOs for API requests and responses
public record CreatePipelineRequest(
    string ServiceId,
    string Name,
    string? Description,
    List<PipelineStage> Stages,
    PipelineTriggers? Triggers = null);

public record UpdatePipelineRequest(
    string? Name = null,
    string? Description = null,
    List<PipelineStage>? Stages = null,
    PipelineTriggers? Triggers = null,
    Dictionary<string, string>? Environment = null);

public record ExecutePipelineRequest(
    string? TriggeredBy = null,
    Dictionary<string, string>? Parameters = null);

public class PipelineSummary
{
    public string Id { get; init; } = string.Empty;
    public string ServiceId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int StageCount { get; init; }
    public ExecutionSummary? LastExecution { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public class ExecutionSummary
{
    public string Id { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? StartedAt { get; init; }
    public int? Duration { get; init; }
    public int BuildNumber { get; init; }
}