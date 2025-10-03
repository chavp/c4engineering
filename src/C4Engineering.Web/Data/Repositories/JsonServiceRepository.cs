using C4Engineering.Web.Models.ServiceCatalog;
using Microsoft.Extensions.Options;

namespace C4Engineering.Web.Data.Repositories;

/// <summary>
/// JSON file-based implementation of service repository
/// </summary>
public class JsonServiceRepository : JsonDataStore<ServiceModel>, IServiceRepository
{
    public JsonServiceRepository(IOptions<JsonDataOptions> options, ILogger<JsonServiceRepository> logger)
        : base(options, logger, "services")
    {
    }

    public async Task<IEnumerable<ServiceModel>> GetAllAsync()
    {
        return await ReadAllEntitiesAsync();
    }

    public async Task<ServiceModel?> GetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Service ID cannot be null or empty", nameof(id));

        return await ReadEntityAsync(id);
    }

    public async Task<ServiceModel> CreateAsync(ServiceModel service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrEmpty(service.Id))
            throw new ArgumentException("Service ID cannot be null or empty", nameof(service));

        if (await EntityExistsAsync(service.Id))
            throw new InvalidOperationException($"Service with ID '{service.Id}' already exists");

        // Set metadata
        var serviceWithMetadata = service with
        {
            Metadata = service.Metadata with
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        return await WriteEntityAsync(service.Id, serviceWithMetadata);
    }

    public async Task<ServiceModel> UpdateAsync(ServiceModel service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrEmpty(service.Id))
            throw new ArgumentException("Service ID cannot be null or empty", nameof(service));

        if (!await EntityExistsAsync(service.Id))
            throw new InvalidOperationException($"Service with ID '{service.Id}' does not exist");

        // Update metadata
        var serviceWithMetadata = service with
        {
            Metadata = service.Metadata with
            {
                UpdatedAt = DateTime.UtcNow
            }
        };

        return await WriteEntityAsync(service.Id, serviceWithMetadata);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Service ID cannot be null or empty", nameof(id));

        return await DeleteEntityAsync(id);
    }

    public async Task<IEnumerable<ServiceModel>> FindByOwnerAsync(string owner)
    {
        if (string.IsNullOrEmpty(owner))
            return Enumerable.Empty<ServiceModel>();

        return await FilterEntitiesAsync(service => 
            service.Owner.Equals(owner, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<ServiceModel>> FindBySystemAsync(string system)
    {
        if (string.IsNullOrEmpty(system))
            return Enumerable.Empty<ServiceModel>();

        return await FilterEntitiesAsync(service => 
            system.Equals(service.System, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<ServiceModel>> FindByLifecycleAsync(ServiceLifecycle lifecycle)
    {
        return await FilterEntitiesAsync(service => service.Lifecycle == lifecycle);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        return await EntityExistsAsync(id);
    }
}