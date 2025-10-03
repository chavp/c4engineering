using C4Engineering.Web.Models.Deployment;

namespace C4Engineering.Web.Services;

/// <summary>
/// Service interface for managing Docker Desktop deployments
/// </summary>
public interface IDockerDeploymentService
{
    Task<IEnumerable<ContainerDeployment>> GetAllDeploymentsAsync();
    Task<ContainerDeployment?> GetDeploymentByIdAsync(string id);
    Task<ContainerDeployment> CreateDeploymentAsync(ContainerDeployment deployment);
    Task<ContainerDeployment> UpdateDeploymentAsync(ContainerDeployment deployment);
    Task<bool> DeleteDeploymentAsync(string id);
    Task<ContainerDeployment> DeployServiceAsync(string serviceId, DeploymentConfiguration config);
    Task<bool> StartContainerAsync(string deploymentId);
    Task<bool> StopContainerAsync(string deploymentId);
    Task<bool> RestartContainerAsync(string deploymentId);
    Task<bool> RemoveContainerAsync(string deploymentId);
    Task<ContainerStatus> GetContainerStatusAsync(string deploymentId);
    Task<IEnumerable<string>> GetContainerLogsAsync(string deploymentId, int tailLines = 100);
    Task<ContainerResourceUsage> GetResourceUsageAsync(string deploymentId);
    Task<DockerStatus> GetDockerStatusAsync();
    Task<bool> IsDockerAvailableAsync();
    Task<bool> ExistsAsync(string id);
}