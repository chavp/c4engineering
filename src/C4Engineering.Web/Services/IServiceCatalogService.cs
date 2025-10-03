using C4Engineering.Web.Models.ServiceCatalog;

namespace C4Engineering.Web.Services;

/// <summary>
/// Service interface for managing service catalog operations
/// </summary>
public interface IServiceCatalogService
{
    Task<IEnumerable<ServiceModel>> GetAllServicesAsync();
    Task<ServiceModel?> GetServiceByIdAsync(string id);
    Task<ServiceModel> CreateServiceAsync(ServiceModel service);
    Task<ServiceModel> UpdateServiceAsync(ServiceModel service);
    Task<bool> DeleteServiceAsync(string id);
    Task<IEnumerable<string>> GetTeamsAsync();
    Task<ServiceDependencies> GetServiceDependenciesAsync(string serviceId);
    Task<IEnumerable<ServiceModel>> SearchServicesAsync(string query);
    Task<IEnumerable<ServiceModel>> GetServicesByOwnerAsync(string owner);
    Task<IEnumerable<ServiceModel>> GetServicesByTypeAsync(ServiceType type);
    Task<bool> ExistsAsync(string id);
}