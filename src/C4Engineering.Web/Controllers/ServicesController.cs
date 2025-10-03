using Microsoft.AspNetCore.Mvc;
using C4Engineering.Web.Models.ServiceCatalog;
using C4Engineering.Web.Models.Requests;
using C4Engineering.Web.Data.Repositories;
using CreateServiceRequest = C4Engineering.Web.Models.Requests.CreateServiceRequest;
using UpdateServiceRequest = C4Engineering.Web.Models.Requests.UpdateServiceRequest;

namespace C4Engineering.Web.Controllers;

/// <summary>
/// REST API controller for service catalog operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ServicesController : ControllerBase
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<ServicesController> _logger;

    public ServicesController(IServiceRepository serviceRepository, ILogger<ServicesController> logger)
    {
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all services with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServiceModel>), 200)]
    public async Task<ActionResult<IEnumerable<ServiceModel>>> GetServices(
        [FromQuery] string? team = null,
        [FromQuery] string? system = null,
        [FromQuery] string? lifecycle = null)
    {
        try
        {
            var services = await _serviceRepository.GetAllAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(team))
            {
                services = services.Where(s => s.Owner.Equals(team, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(system))
            {
                services = services.Where(s => system.Equals(s.System, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(lifecycle) && Enum.TryParse<ServiceLifecycle>(lifecycle, true, out var parsedLifecycle))
            {
                services = services.Where(s => s.Lifecycle == parsedLifecycle);
            }

            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve services");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get service by ID
    /// </summary>
    [HttpGet("{serviceId}")]
    [ProducesResponseType(typeof(ServiceModel), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ServiceModel>> GetService(string serviceId)
    {
        try
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            
            if (service == null)
            {
                return NotFound();
            }

            return Ok(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve service {ServiceId}", serviceId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new service
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceModel), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<ServiceModel>> CreateService([FromBody] CreateServiceRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Parse enum values
            if (!Enum.TryParse<ServiceType>(request.Type, true, out var serviceType))
            {
                return BadRequest(new { error = "Invalid service type" });
            }

            var lifecycle = ServiceLifecycle.Development;
            if (!string.IsNullOrEmpty(request.Lifecycle) && 
                !Enum.TryParse<ServiceLifecycle>(request.Lifecycle, true, out lifecycle))
            {
                return BadRequest(new { error = "Invalid lifecycle" });
            }

            var service = new ServiceModel
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description,
                Type = serviceType,
                Owner = request.Owner,
                Repository = request.Repository,
                Documentation = request.Documentation,
                Tags = request.Tags ?? new List<string>(),
                Lifecycle = lifecycle,
                System = request.System
            };

            var createdService = await _serviceRepository.CreateAsync(service);
            
            return CreatedAtAction(nameof(GetService), new { serviceId = createdService.Id }, createdService);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create service");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update an existing service
    /// </summary>
    [HttpPut("{serviceId}")]
    [ProducesResponseType(typeof(ServiceModel), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ServiceModel>> UpdateService(string serviceId, [FromBody] UpdateServiceRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var existingService = await _serviceRepository.GetByIdAsync(serviceId);
            if (existingService == null)
            {
                return NotFound();
            }

            // Parse lifecycle if provided
            var lifecycle = existingService.Lifecycle;
            if (!string.IsNullOrEmpty(request.Lifecycle) && 
                !Enum.TryParse<ServiceLifecycle>(request.Lifecycle, true, out lifecycle))
            {
                return BadRequest(new { error = "Invalid lifecycle" });
            }

            var updatedService = existingService with
            {
                Name = request.Name ?? existingService.Name,
                Description = request.Description ?? existingService.Description,
                Owner = request.Owner ?? existingService.Owner,
                Repository = request.Repository ?? existingService.Repository,
                Documentation = request.Documentation ?? existingService.Documentation,
                Tags = request.Tags ?? existingService.Tags,
                Lifecycle = lifecycle
            };

            var result = await _serviceRepository.UpdateAsync(updatedService);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist"))
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update service {ServiceId}", serviceId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete a service
    /// </summary>
    [HttpDelete("{serviceId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteService(string serviceId)
    {
        try
        {
            var deleted = await _serviceRepository.DeleteAsync(serviceId);
            
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete service {ServiceId}", serviceId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get service dependencies
    /// </summary>
    [HttpGet("{serviceId}/dependencies")]
    [ProducesResponseType(typeof(ServiceDependencies), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ServiceDependencies>> GetServiceDependencies(string serviceId)
    {
        try
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service == null)
            {
                return NotFound();
            }

            var allServices = await _serviceRepository.GetAllAsync();
            
            // Find dependencies (services this service depends on)
            var dependencies = allServices.Where(s => service.DependsOn.Contains(s.Id)).ToList();
            
            // Find dependents (services that depend on this service)
            var dependents = allServices.Where(s => s.DependsOn.Contains(serviceId)).ToList();

            var result = new ServiceDependencies
            {
                ServiceId = serviceId,
                Dependencies = dependencies,
                Dependents = dependents
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dependencies for service {ServiceId}", serviceId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all unique teams from services
    /// </summary>
    [HttpGet("teams")]
    [ProducesResponseType(typeof(IEnumerable<string>), 200)]
    public async Task<ActionResult<IEnumerable<string>>> GetTeams()
    {
        try
        {
            var services = await _serviceRepository.GetAllAsync();
            var teams = services
                .Where(s => !string.IsNullOrEmpty(s.Owner))
                .Select(s => s.Owner)
                .Distinct()
                .OrderBy(team => team)
                .ToList();

            return Ok(teams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get teams");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}