namespace C4Engineering.Web.Models.Deployment;

/// <summary>
/// Represents a deployment configuration for Docker Desktop
/// </summary>
public class DeploymentConfiguration
{
    public string Id { get; init; } = string.Empty;
    public string ServiceId { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public DeploymentStatus Status { get; init; }
    public string ContainerName { get; init; } = string.Empty;
    public string ImageName { get; init; } = string.Empty;
    public List<PortMapping> Ports { get; init; } = new();
    public Dictionary<string, string> EnvironmentVars { get; init; } = new();
    public List<VolumeMount> Volumes { get; init; } = new();
    public ResourceLimits Resources { get; init; } = new();
    public ContainerHealthCheck HealthCheck { get; init; } = new();
    public DeploymentInfo Deployment { get; init; } = new();
}

public class ContainerInstance
{
    public string Id { get; init; } = string.Empty;
    public string DeploymentId { get; init; } = string.Empty;
    public string DockerContainerId { get; init; } = string.Empty;
    public ContainerStatus Status { get; init; }
    public DateTime? StartedAt { get; init; }
    public int RestartCount { get; init; }
    public List<string> Ports { get; init; } = new();
    public ResourceUsage Resources { get; init; } = new();
    public List<ContainerLogEntry> Logs { get; init; } = new();
}

public class PortMapping
{
    public int ContainerPort { get; init; }
    public int HostPort { get; init; }
    public string Protocol { get; init; } = "tcp";
}

public class VolumeMount
{
    public string HostPath { get; init; } = string.Empty;
    public string ContainerPath { get; init; } = string.Empty;
    public bool ReadOnly { get; init; } = false;
}

public class ResourceLimits
{
    public string? CpuLimit { get; init; }
    public string? MemoryLimit { get; init; }
}

public class ResourceUsage
{
    public double CpuUsage { get; init; }
    public double MemoryUsage { get; init; }
    public NetworkIO NetworkIO { get; init; } = new();
}

public class NetworkIO
{
    public long BytesReceived { get; init; }
    public long BytesSent { get; init; }
}

public class ContainerHealthCheck
{
    public string? Url { get; init; }
    public HealthStatus Status { get; init; }
    public DateTime? LastChecked { get; init; }
}

public class DeploymentInfo
{
    public DateTime? DeployedAt { get; init; }
    public string? DeployedBy { get; init; }
    public string? ExecutionId { get; init; }
}

public class ContainerLogEntry
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Level { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public enum DeploymentStatus
{
    Pending,
    Running,
    Stopped,
    Failed,
    Unknown
}

public enum ContainerStatus
{
    Created,
    Running,
    Paused,
    Restarting,
    Removing,
    Exited,
    Dead
}

public enum HealthStatus
{
    Unknown,
    Starting,
    Healthy,
    Unhealthy
}

// DTOs for API requests and responses
public record CreateDeploymentRequest(
    string ServiceId,
    string ImageName,
    string ContainerName,
    List<PortMapping>? Ports = null,
    Dictionary<string, string>? Environment = null,
    List<VolumeMount>? Volumes = null,
    ResourceLimits? Resources = null);

public record UpdateDeploymentRequest(
    DeploymentStatus? Status = null,
    Dictionary<string, string>? Environment = null,
    ResourceLimits? Resources = null);

public class DeploymentSummary
{
    public string Id { get; init; } = string.Empty;
    public string ServiceId { get; init; } = string.Empty;
    public string ContainerName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? DeployedAt { get; init; }
    public string? HealthStatus { get; init; }
    public List<string> Ports { get; init; } = new();
}

public class ContainerDeployment
{
    public string Id { get; init; } = string.Empty;
    public string ServiceId { get; init; } = string.Empty;
    public string ContainerName { get; init; } = string.Empty;
    public string ImageName { get; init; } = string.Empty;
    public DeploymentStatus Status { get; init; }
    public ContainerStatus ContainerStatus { get; init; }
    public DateTime? DeployedAt { get; init; }
    public string? DeployedBy { get; init; }
    public List<PortMapping> Ports { get; init; } = new();
    public Dictionary<string, string> Environment { get; init; } = new();
    public List<VolumeMount> Volumes { get; init; } = new();
    public ResourceLimits Resources { get; init; } = new();
    public ContainerHealthCheck HealthCheck { get; init; } = new();
    public ResourceUsage CurrentUsage { get; init; } = new();
}

public class ContainerResourceUsage
{
    public double CpuUsagePercent { get; init; }
    public long MemoryUsageBytes { get; init; }
    public double MemoryUsagePercent { get; init; }
    public long NetworkRxBytes { get; init; }
    public long NetworkTxBytes { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public class DockerStatus
{
    public bool IsAvailable { get; init; }
    public string Version { get; init; } = string.Empty;
    public int RunningContainers { get; init; }
    public int TotalContainers { get; init; }
    public string? ErrorMessage { get; init; }
}