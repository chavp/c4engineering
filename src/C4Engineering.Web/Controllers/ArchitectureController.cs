using Microsoft.AspNetCore.Mvc;
using C4Engineering.Web.Services;
using C4Engineering.Web.Models.Architecture;

namespace C4Engineering.Web.Controllers;

/// <summary>
/// MVC controller for Architecture diagram views
/// </summary>
public class ArchitectureController : Controller
{
    private readonly IDiagramService _diagramService;
    private readonly IServiceCatalogService _serviceCatalogService;
    private readonly ILogger<ArchitectureController> _logger;

    public ArchitectureController(
        IDiagramService diagramService,
        IServiceCatalogService serviceCatalogService,
        ILogger<ArchitectureController> logger)
    {
        _diagramService = diagramService;
        _serviceCatalogService = serviceCatalogService;
        _logger = logger;
    }

    /// <summary>
    /// Architecture diagrams index page
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var diagrams = await _diagramService.GetAllDiagramsAsync();
            return View(diagrams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading architecture diagrams");
            ViewBag.ErrorMessage = "Failed to load diagrams. Please try again.";
            return View(Enumerable.Empty<DiagramModel>());
        }
    }

    /// <summary>
    /// Diagram editor page
    /// </summary>
    public async Task<IActionResult> Editor(string? id)
    {
        try
        {
            DiagramModel? diagram = null;
            
            if (!string.IsNullOrEmpty(id))
            {
                diagram = await _diagramService.GetDiagramByIdAsync(id);
                if (diagram == null)
                {
                    return NotFound();
                }
            }

            ViewBag.DiagramId = id;
            return View(diagram);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading diagram editor for {DiagramId}", id);
            ViewBag.ErrorMessage = "Failed to load diagram editor. Please try again.";
            return View();
        }
    }

    /// <summary>
    /// Create new diagram page
    /// </summary>
    public IActionResult Create()
    {
        return View("Editor");
    }

    /// <summary>
    /// View diagram in read-only mode
    /// </summary>
    public async Task<IActionResult> ViewDiagram(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        try
        {
            var diagram = await _diagramService.GetDiagramByIdAsync(id);
            if (diagram == null)
            {
                return NotFound();
            }

            ViewBag.ReadOnly = true;
            return View("Editor", diagram);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading diagram view for {DiagramId}", id);
            ViewBag.ErrorMessage = "Failed to load diagram. Please try again.";
            return View("Editor");
        }
    }
}