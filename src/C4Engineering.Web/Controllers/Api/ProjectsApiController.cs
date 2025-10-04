using Microsoft.AspNetCore.Mvc;
using C4Engineering.Web.Services;
using C4Engineering.Web.Models.Project;

namespace C4Engineering.Web.Controllers.Api;

/// <summary>
/// API controller for project operations
/// </summary>
[ApiController]
[Route("api/projects")]
[Produces("application/json")]
public class ProjectsApiController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsApiController> _logger;

    public ProjectsApiController(
        IProjectService projectService,
        ILogger<ProjectsApiController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Get all projects
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectSummary>>> GetAllProjects()
    {
        try
        {
            var projects = await _projectService.GetAllProjectSummariesAsync();
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get project by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectModel>> GetProject(string id)
    {
        try
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound($"Project with ID '{id}' not found");
            }

            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project {ProjectId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create new project
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProjectModel>> CreateProject([FromBody] CreateProjectRequest request)
    {
        try
        {
            var project = await _projectService.CreateProjectAsync(request);
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search projects
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProjectSummary>>> SearchProjects([FromQuery] string q = "")
    {
        try
        {
            var projects = await _projectService.SearchProjectsAsync(q);
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching projects with query {Query}", q);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get project templates
    /// </summary>
    [HttpGet("templates")]
    public async Task<ActionResult<IEnumerable<ProjectTemplate>>> GetProjectTemplates()
    {
        try
        {
            var templates = await _projectService.GetProjectTemplatesAsync();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project templates");
            return StatusCode(500, "Internal server error");
        }
    }
}