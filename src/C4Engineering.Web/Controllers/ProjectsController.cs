using Microsoft.AspNetCore.Mvc;
using C4Engineering.Web.Services;
using C4Engineering.Web.Models.Project;

namespace C4Engineering.Web.Controllers;

/// <summary>
/// MVC controller for Project views
/// </summary>
public class ProjectsController : Controller
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(
        IProjectService projectService,
        ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Projects index page
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var projects = await _projectService.GetAllProjectSummariesAsync();
            return View(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading projects");
            ViewBag.ErrorMessage = "Failed to load projects. Please try again.";
            return View(Enumerable.Empty<ProjectSummary>());
        }
    }

    /// <summary>
    /// Project details page
    /// </summary>
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        try
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading project details for {ProjectId}", id);
            ViewBag.ErrorMessage = "Failed to load project details. Please try again.";
            return View();
        }
    }

    /// <summary>
    /// Create new project page
    /// </summary>
    public async Task<IActionResult> Create()
    {
        try
        {
            var templates = await _projectService.GetProjectTemplatesAsync();
            ViewBag.Templates = templates;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading project templates");
            ViewBag.ErrorMessage = "Failed to load project templates.";
            return View();
        }
    }

    /// <summary>
    /// Handle project creation
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProjectRequest request)
    {
        if (!ModelState.IsValid)
        {
            var templates = await _projectService.GetProjectTemplatesAsync();
            ViewBag.Templates = templates;
            return View(request);
        }

        try
        {
            var project = await _projectService.CreateProjectAsync(request);
            TempData["SuccessMessage"] = $"Project '{project.Name}' created successfully!";
            return RedirectToAction(nameof(Details), new { id = project.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project {ProjectId}", request.Id);
            ModelState.AddModelError("", ex.Message);
            
            var templates = await _projectService.GetProjectTemplatesAsync();
            ViewBag.Templates = templates;
            return View(request);
        }
    }

    /// <summary>
    /// Create project from template
    /// </summary>
    public async Task<IActionResult> CreateFromTemplate(string templateId)
    {
        if (string.IsNullOrEmpty(templateId))
        {
            return NotFound();
        }

        try
        {
            var templates = await _projectService.GetProjectTemplatesAsync();
            var template = templates.FirstOrDefault(t => t.Id == templateId);
            
            if (template == null)
            {
                return NotFound();
            }

            ViewBag.Template = template;
            ViewBag.Templates = templates;
            
            // Pre-populate with template defaults
            var model = new CreateProjectRequest(
                Id: "",
                Name: "",
                Description: template.Description,
                Owner: "Current User", // TODO: Get from authentication
                Type: template.DefaultType.ToString(),
                Tags: template.DefaultTags,
                Settings: template.DefaultSettings
            );

            return View("CreateFromTemplate", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading template {TemplateId}", templateId);
            return RedirectToAction(nameof(Create));
        }
    }

    /// <summary>
    /// Handle project creation from template
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromTemplate(string templateId, CreateProjectRequest request)
    {
        if (!ModelState.IsValid)
        {
            var templates = await _projectService.GetProjectTemplatesAsync();
            var template = templates.FirstOrDefault(t => t.Id == templateId);
            ViewBag.Template = template;
            ViewBag.Templates = templates;
            return View(request);
        }

        try
        {
            var project = await _projectService.CreateProjectFromTemplateAsync(templateId, request);
            TempData["SuccessMessage"] = $"Project '{project.Name}' created successfully from template!";
            return RedirectToAction(nameof(Details), new { id = project.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project from template {TemplateId}", templateId);
            ModelState.AddModelError("", ex.Message);
            
            var templates = await _projectService.GetProjectTemplatesAsync();
            var template = templates.FirstOrDefault(t => t.Id == templateId);
            ViewBag.Template = template;
            ViewBag.Templates = templates;
            return View(request);
        }
    }
}