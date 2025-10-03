using Microsoft.AspNetCore.Mvc;
using C4Engineering.Web.Services;
using C4Engineering.Web.Models.ServiceCatalog;

namespace C4Engineering.Web.Controllers;

/// <summary>
/// MVC controller for Service Catalog views
/// </summary>
public class ServiceCatalogController : Controller
{
    private readonly IServiceCatalogService _serviceCatalogService;
    private readonly ILogger<ServiceCatalogController> _logger;

    public ServiceCatalogController(
        IServiceCatalogService serviceCatalogService,
        ILogger<ServiceCatalogController> logger)
    {
        _serviceCatalogService = serviceCatalogService;
        _logger = logger;
    }

    /// <summary>
    /// Service catalog index page
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var services = await _serviceCatalogService.GetAllServicesAsync();
            var serviceSummaries = services.Select(service => new ServiceSummary
            {
                Id = service.Id,
                Name = service.Name,
                Type = service.Type.ToString(),
                Owner = service.Owner,
                Description = service.Description,
                Status = "Active", // TODO: Get actual status
                LastUpdated = DateTime.UtcNow, // TODO: Get actual last updated
                DependencyCount = service.DependsOn?.Count ?? 0,
                Tags = service.Tags ?? new List<string>()
            });

            return View(serviceSummaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading service catalog");
            ViewBag.ErrorMessage = "Failed to load services. Please try again.";
            return View(Enumerable.Empty<ServiceSummary>());
        }
    }

    /// <summary>
    /// Service details page
    /// </summary>
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        try
        {
            var service = await _serviceCatalogService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading service details for {ServiceId}", id);
            ViewBag.ErrorMessage = "Failed to load service details. Please try again.";
            return View();
        }
    }
}