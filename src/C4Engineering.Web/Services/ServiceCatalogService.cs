using C4Engineering.Web.Models.ServiceCatalog;
using C4Engineering.Web.Data.Repositories;

namespace C4Engineering.Web.Services;

/// <summary>
/// Service catalog operations implementation using repository pattern
/// </summary>
public class ServiceCatalogService : IServiceCatalogService
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<ServiceCatalogService> _logger;

    public ServiceCatalogService(IServiceRepository serviceRepository, ILogger<ServiceCatalogService> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ServiceModel>> GetAllServicesAsync()
    {
        try
        {
            return await _serviceRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all services");
            throw;
        }
    }

    public async Task<ServiceModel?> GetServiceByIdAsync(string id)
    {
        try
        {
            return await _serviceRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service {ServiceId}", id);
            throw;
        }
    }

    public async Task<ServiceModel> CreateServiceAsync(ServiceModel service)
    {
        try
        {
            return await _serviceRepository.CreateAsync(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service {ServiceId}", service.Id);
            throw;
        }
    }

    public async Task<ServiceModel> UpdateServiceAsync(ServiceModel service)
    {
        try
        {
            return await _serviceRepository.UpdateAsync(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service {ServiceId}", service.Id);
            throw;
        }
    }

    public async Task<bool> DeleteServiceAsync(string id)
    {
        try
        {
            return await _serviceRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service {ServiceId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetTeamsAsync()
    {
        try
        {
            var services = await _serviceRepository.GetAllAsync();
            return services
                .Where(s => !string.IsNullOrEmpty(s.Owner))
                .Select(s => s.Owner)
                .Distinct()
                .OrderBy(owner => owner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teams");
            throw;
        }
    }

    public async Task<ServiceDependencies> GetServiceDependenciesAsync(string serviceId)
    {
        try
        {
            var allServices = (await _serviceRepository.GetAllAsync()).ToList();
            var targetService = allServices.FirstOrDefault(s => s.Id == serviceId);
            
            if (targetService == null)
            {
                return new ServiceDependencies { ServiceId = serviceId };
            }

            var dependencies = allServices
                .Where(s => targetService.DependsOn.Contains(s.Id))
                .ToList();

            var dependents = allServices
                .Where(s => s.DependsOn.Contains(serviceId))
                .ToList();

            return new ServiceDependencies
            {
                ServiceId = serviceId,
                Dependencies = dependencies,
                Dependents = dependents
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dependencies for service {ServiceId}", serviceId);
            throw;
        }
    }

    public async Task<IEnumerable<ServiceModel>> SearchServicesAsync(string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await _serviceRepository.GetAllAsync();
            }

            var allServices = await _serviceRepository.GetAllAsync();
            var searchTerm = query.ToLowerInvariant();

            return allServices.Where(service =>
                service.Name.ToLowerInvariant().Contains(searchTerm) ||
                (service.Description?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                service.Tags.Any(tag => tag.ToLowerInvariant().Contains(searchTerm)) ||
                service.Owner.ToLowerInvariant().Contains(searchTerm));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching services with query {Query}", query);
            throw;
        }
    }

    public async Task<IEnumerable<ServiceModel>> GetServicesByOwnerAsync(string owner)
    {
        try
        {
            return await _serviceRepository.FindByOwnerAsync(owner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving services for owner {Owner}", owner);
            throw;
        }
    }

    public async Task<IEnumerable<ServiceModel>> GetServicesByTypeAsync(ServiceType type)
    {
        try
        {
            var allServices = await _serviceRepository.GetAllAsync();
            return allServices.Where(s => s.Type == type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving services for type {Type}", type);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        try
        {
            return await _serviceRepository.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if service {ServiceId} exists", id);
            throw;
        }
    }
}