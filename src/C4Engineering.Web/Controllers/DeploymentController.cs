using Microsoft.AspNetCore.Mvc;
using C4Engineering.Web.Services;
using C4Engineering.Web.Models.Deployment;

namespace C4Engineering.Web.Controllers;

/// <summary>
/// MVC controller for Deployment views
/// </summary>
public class DeploymentController : Controller
{
    private readonly IDockerDeploymentService _deploymentService;
    private readonly IServiceCatalogService _serviceCatalogService;
    private readonly ILogger<DeploymentController> _logger;

    public DeploymentController(
        IDockerDeploymentService deploymentService,
        IServiceCatalogService serviceCatalogService,
        ILogger<DeploymentController> logger)
    {
        _deploymentService = deploymentService;
        _serviceCatalogService = serviceCatalogService;
        _logger = logger;
    }

    /// <summary>
    /// Deployments index page
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var deployments = await _deploymentService.GetAllDeploymentsAsync();
            return View(deployments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading deployments");
            ViewBag.ErrorMessage = "Failed to load deployments. Please try again.";
            return View(Enumerable.Empty<ContainerDeployment>());
        }
    }

    /// <summary>
    /// Deployment details page
    /// </summary>
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        try
        {
            var deployment = await _deploymentService.GetDeploymentByIdAsync(id);
            if (deployment == null)
            {
                return NotFound();
            }

            return View(deployment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading deployment details for {DeploymentId}", id);
            ViewBag.ErrorMessage = "Failed to load deployment details. Please try again.";
            return View();
        }
    }

    /// <summary>
    /// Container logs page
    /// </summary>
    public async Task<IActionResult> Logs(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        try
        {
            var deployment = await _deploymentService.GetDeploymentByIdAsync(id);
            if (deployment == null)
            {
                return NotFound();
            }

            ViewBag.DeploymentId = id;
            ViewBag.ContainerName = deployment.ContainerName;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading logs page for {DeploymentId}", id);
            ViewBag.ErrorMessage = "Failed to load logs. Please try again.";
            return View();
        }
    }
}