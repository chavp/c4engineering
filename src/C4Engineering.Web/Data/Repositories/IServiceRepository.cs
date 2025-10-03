using C4Engineering.Web.Models.ServiceCatalog;

namespace C4Engineering.Web.Data.Repositories;

/// <summary>
/// Repository interface for service catalog operations
/// </summary>
public interface IServiceRepository
{
    Task<IEnumerable<ServiceModel>> GetAllAsync();
    Task<ServiceModel?> GetByIdAsync(string id);
    Task<ServiceModel> CreateAsync(ServiceModel service);
    Task<ServiceModel> UpdateAsync(ServiceModel service);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<ServiceModel>> FindByOwnerAsync(string owner);
    Task<IEnumerable<ServiceModel>> FindBySystemAsync(string system);
    Task<IEnumerable<ServiceModel>> FindByLifecycleAsync(ServiceLifecycle lifecycle);
    Task<bool> ExistsAsync(string id);
}