using Microsoft.AspNetCore.Mvc;
using C4Engineering.Web.Services;
using C4Engineering.Web.Models.Pipeline;

namespace C4Engineering.Web.Controllers;

/// <summary>
/// MVC controller for Pipeline views (distinguished from API PipelinesController)
/// </summary>
public class PipelineController : Controller
{
    private readonly IPipelineService _pipelineService;
    private readonly IServiceCatalogService _serviceCatalogService;
    private readonly ILogger<PipelineController> _logger;

    public PipelineController(
        IPipelineService pipelineService,
        IServiceCatalogService serviceCatalogService,
        ILogger<PipelineController> logger)
    {
        _pipelineService = pipelineService;
        _serviceCatalogService = serviceCatalogService;
        _logger = logger;
    }

    /// <summary>
    /// Pipelines index page
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var pipelines = await _pipelineService.GetAllPipelinesAsync();
            var pipelineSummaries = new List<PipelineSummary>();

            foreach (var pipeline in pipelines)
            {
                ExecutionSummary? lastExecutionSummary = null;
                
                // Get last execution
                var executions = await _pipelineService.GetPipelineExecutionsAsync(pipeline.Id);
                var lastExecution = executions.OrderByDescending(e => e.StartedAt).FirstOrDefault();
                
                if (lastExecution != null)
                {
                    lastExecutionSummary = new ExecutionSummary
                    {
                        Id = lastExecution.Id,
                        Status = lastExecution.Status.ToString(),
                        StartedAt = lastExecution.StartedAt,
                        Duration = lastExecution.Duration,
                        BuildNumber = lastExecution.BuildNumber
                    };
                }

                var summary = new PipelineSummary
                {
                    Id = pipeline.Id,
                    ServiceId = pipeline.ServiceId,
                    Name = pipeline.Name,
                    Description = pipeline.Description,
                    StageCount = pipeline.Stages.Count,
                    UpdatedAt = pipeline.Metadata.UpdatedAt,
                    LastExecution = lastExecutionSummary
                };

                pipelineSummaries.Add(summary);
            }

            return View(pipelineSummaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pipelines");
            ViewBag.ErrorMessage = "Failed to load pipelines. Please try again.";
            return View(Enumerable.Empty<PipelineSummary>());
        }
    }

    /// <summary>
    /// Pipeline details page
    /// </summary>
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        try
        {
            var pipeline = await _pipelineService.GetPipelineByIdAsync(id);
            if (pipeline == null)
            {
                return NotFound();
            }

            return View(pipeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pipeline details for {PipelineId}", id);
            ViewBag.ErrorMessage = "Failed to load pipeline details. Please try again.";
            return View();
        }
    }

    /// <summary>
    /// Pipeline execution details page
    /// </summary>
    public async Task<IActionResult> Execution(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        try
        {
            var execution = await _pipelineService.GetExecutionByIdAsync(id);
            if (execution == null)
            {
                return NotFound();
            }

            return View(execution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading execution details for {ExecutionId}", id);
            ViewBag.ErrorMessage = "Failed to load execution details. Please try again.";
            return View();
        }
    }
}