namespace C4Engineering.Web.Models.ServiceCatalog;

/// <summary>
/// Represents a service in the service catalog following Backstage entity model
/// </summary>
public record ServiceModel
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ServiceType Type { get; init; }
    public string Owner { get; init; } = string.Empty;
    public string? Repository { get; init; }
    public string? Documentation { get; init; }
    public string? ApiSpec { get; init; }
    public List<string> Tags { get; init; } = new();
    public ServiceLifecycle Lifecycle { get; init; }
    public string? System { get; init; }
    public List<string> DependsOn { get; init; } = new();
    public List<string> ProvidesApis { get; init; } = new();
    public List<string> ConsumesApis { get; init; } = new();
    public ServiceMetadata Metadata { get; init; } = new();
}

public record ServiceMetadata
{
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
    public string? Version { get; init; }
    public string? HealthCheckUrl { get; init; }
}

public class TeamModel
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Email { get; init; }
    public string? Slack { get; init; }
    public List<TeamMember> Members { get; init; } = new();
    public List<string> Services { get; init; } = new();
    public string? OncallSchedule { get; init; }
}

public class TeamMember
{
    public string Name { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string? Email { get; init; }
}

public enum ServiceType
{
    Service,
    Website,
    Library
}

public enum ServiceLifecycle
{
    Experimental,
    Development,
    Production,
    Deprecated
}

// DTOs for API requests and responses
public record CreateServiceRequest(
    string Id,
    string Name,
    string? Description,
    string Type,
    string Owner,
    string? Repository = null,
    string? Documentation = null,
    List<string>? Tags = null,
    string? Lifecycle = null,
    string? System = null);

public record UpdateServiceRequest(
    string? Name = null,
    string? Description = null,
    string? Owner = null,
    string? Repository = null,
    string? Documentation = null,
    List<string>? Tags = null,
    string? Lifecycle = null);

public class ServiceDependencies
{
    public string ServiceId { get; init; } = string.Empty;
    public List<ServiceModel> Dependencies { get; init; } = new();
    public List<ServiceModel> Dependents { get; init; } = new();
}

public class ServiceSummary
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Owner { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime LastUpdated { get; init; }
    public int DependencyCount { get; init; }
    public List<string> Tags { get; init; } = new();
}