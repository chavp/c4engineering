namespace C4Engineering.Web.Models.Project;

/// <summary>
/// Represents a platform engineering project containing multiple services, diagrams, and pipelines
/// </summary>
public record ProjectModel
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Owner { get; init; } = string.Empty;
    public ProjectType Type { get; init; }
    public ProjectStatus Status { get; init; }
    public List<string> Tags { get; init; } = new();
    public List<string> Services { get; init; } = new();
    public List<string> Diagrams { get; init; } = new();
    public List<string> Pipelines { get; init; } = new();
    public List<ProjectTeamMember> TeamMembers { get; init; } = new();
    public ProjectSettings Settings { get; init; } = new();
    public ProjectMetadata Metadata { get; init; } = new();
}

public record ProjectTeamMember
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public ProjectRole Role { get; init; }
    public DateTime JoinedAt { get; init; } = DateTime.UtcNow;
}

public record ProjectSettings
{
    public string? Repository { get; init; }
    public string? Documentation { get; init; }
    public string? SlackChannel { get; init; }
    public string? JiraProject { get; init; }
    public bool IsPublic { get; init; } = true;
    public bool EnableNotifications { get; init; } = true;
    public List<string> AllowedDomains { get; init; } = new();
}

public record ProjectMetadata
{
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
    public string? CreatedBy { get; init; }
    public string? Version { get; init; } = "1.0";
    public string? Template { get; init; }
    public Dictionary<string, object> CustomFields { get; init; } = new();
}

public enum ProjectType
{
    WebApplication,
    MobileApplication,
    Microservices,
    DataPlatform,
    MLPlatform,
    Infrastructure,
    Library,
    Documentation
}

public enum ProjectStatus
{
    Planning,
    Active,
    Development,
    Production,
    Maintenance,
    Deprecated,
    Archived
}

public enum ProjectRole
{
    Owner,
    Maintainer,
    Developer,
    Contributor,
    Viewer
}

// DTOs for API requests and responses
public record CreateProjectRequest(
    string Id,
    string Name,
    string? Description,
    string Owner,
    string Type,
    List<string>? Tags = null,
    ProjectSettings? Settings = null);

public record UpdateProjectRequest(
    string? Name = null,
    string? Description = null,
    string? Owner = null,
    string? Status = null,
    List<string>? Tags = null,
    ProjectSettings? Settings = null);

public record AddTeamMemberRequest(
    string Name,
    string Email,
    string Role);

public class ProjectSummary
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Owner { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int ServiceCount { get; init; }
    public int DiagramCount { get; init; }
    public int PipelineCount { get; init; }
    public int TeamMemberCount { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<string> Tags { get; init; } = new();
}

public class ProjectTemplate
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Category { get; init; } = string.Empty;
    public ProjectType DefaultType { get; init; }
    public List<string> PreConfiguredServices { get; init; } = new();
    public List<string> PreConfiguredDiagrams { get; init; } = new();
    public List<string> DefaultTags { get; init; } = new();
    public ProjectSettings DefaultSettings { get; init; } = new();
}