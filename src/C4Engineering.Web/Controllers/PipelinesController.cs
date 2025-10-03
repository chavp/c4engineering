using Microsoft.AspNetCore.Mvc;
using C4Engineering.Web.Models.Pipeline;
using C4Engineering.Web.Models.Requests;
using C4Engineering.Web.Data.Repositories;
using CreatePipelineRequest = C4Engineering.Web.Models.Requests.CreatePipelineRequest;
using UpdatePipelineRequest = C4Engineering.Web.Models.Requests.UpdatePipelineRequest;
using ExecutePipelineRequest = C4Engineering.Web.Models.Requests.ExecutePipelineRequest;

namespace C4Engineering.Web.Controllers;

/// <summary>
/// REST API controller for pipeline operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PipelinesController : ControllerBase
{
    private readonly IPipelineRepository _pipelineRepository;
    private readonly IPipelineExecutionRepository _executionRepository;
    private readonly ILogger<PipelinesController> _logger;

    public PipelinesController(
        IPipelineRepository pipelineRepository,
        IPipelineExecutionRepository executionRepository,
        ILogger<PipelinesController> logger)
    {
        _pipelineRepository = pipelineRepository;
        _executionRepository = executionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all pipelines with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PipelineConfiguration>), 200)]
    public async Task<ActionResult<IEnumerable<PipelineConfiguration>>> GetPipelines(
        [FromQuery] string? serviceId = null)
    {
        try
        {
            var pipelines = await _pipelineRepository.GetAllAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(serviceId))
            {
                pipelines = pipelines.Where(p => serviceId.Equals(p.ServiceId, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(pipelines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve pipelines");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get pipeline by ID
    /// </summary>
    [HttpGet("{pipelineId}")]
    [ProducesResponseType(typeof(PipelineConfiguration), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PipelineConfiguration>> GetPipeline(string pipelineId)
    {
        try
        {
            var pipeline = await _pipelineRepository.GetByIdAsync(pipelineId);
            
            if (pipeline == null)
            {
                return NotFound();
            }

            return Ok(pipeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve pipeline {PipelineId}", pipelineId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new pipeline
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PipelineConfiguration), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<PipelineConfiguration>> CreatePipeline([FromBody] CreatePipelineRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Generate ID
            var pipelineId = $"{request.ServiceId}-{request.Name.ToLowerInvariant().Replace(" ", "-")}-{Guid.NewGuid().ToString()[..8]}";

            var pipeline = new PipelineConfiguration
            {
                Id = pipelineId,
                ServiceId = request.ServiceId,
                Name = request.Name,
                Description = request.Description,
                Stages = request.Stages ?? new List<PipelineStage>(),
                Environment = new Dictionary<string, string>(),
                Triggers = request.Triggers ?? new PipelineTriggers(),
                Metadata = new PipelineMetadata()
            };

            var createdPipeline = await _pipelineRepository.CreateAsync(pipeline);
            
            return CreatedAtAction(nameof(GetPipeline), new { pipelineId = createdPipeline.Id }, createdPipeline);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create pipeline");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update an existing pipeline
    /// </summary>
    [HttpPut("{pipelineId}")]
    [ProducesResponseType(typeof(PipelineConfiguration), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<PipelineConfiguration>> UpdatePipeline(string pipelineId, [FromBody] UpdatePipelineRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var existingPipeline = await _pipelineRepository.GetByIdAsync(pipelineId);
            if (existingPipeline == null)
            {
                return NotFound();
            }

            var updatedPipeline = existingPipeline with
            {
                Name = request.Name ?? existingPipeline.Name,
                Description = request.Description ?? existingPipeline.Description,
                Stages = request.Stages ?? existingPipeline.Stages,
                Environment = request.Environment ?? existingPipeline.Environment,
                Triggers = request.Triggers ?? existingPipeline.Triggers
            };

            var result = await _pipelineRepository.UpdateAsync(updatedPipeline);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist"))
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update pipeline {PipelineId}", pipelineId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete a pipeline
    /// </summary>
    [HttpDelete("{pipelineId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeletePipeline(string pipelineId)
    {
        try
        {
            var deleted = await _pipelineRepository.DeleteAsync(pipelineId);
            
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete pipeline {PipelineId}", pipelineId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Execute a pipeline
    /// </summary>
    [HttpPost("{pipelineId}/executions")]
    [ProducesResponseType(typeof(PipelineExecution), 202)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<PipelineExecution>> ExecutePipeline(string pipelineId, [FromBody] ExecutePipelineRequest? request = null)
    {
        try
        {
            var pipeline = await _pipelineRepository.GetByIdAsync(pipelineId);
            if (pipeline == null)
            {
                return NotFound();
            }

            var executionId = $"{pipelineId}-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString()[..8]}";
            
            var execution = new PipelineExecution
            {
                Id = executionId,
                PipelineId = pipelineId,
                Status = ExecutionStatus.Queued,
                StartedAt = DateTime.UtcNow,
                Parameters = request?.Parameters ?? new Dictionary<string, string>(),
                StageExecutions = pipeline.Stages.Select(stage => new StageExecution
                {
                    StageName = stage.Name,
                    Status = ExecutionStatus.Pending,
                    Steps = stage.Steps.Select(step => new StepExecution
                    {
                        StepName = step.Name,
                        Status = ExecutionStatus.Pending
                    }).ToList()
                }).ToList(),
                Logs = new List<ExecutionLogEntry>()
            };

            var createdExecution = await _executionRepository.CreateAsync(execution);
            
            // In a real implementation, this would trigger the actual pipeline execution
            // For MVP, we'll just return the created execution
            
            return AcceptedAtAction(nameof(GetExecution), new { executionId = createdExecution.Id }, createdExecution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute pipeline {PipelineId}", pipelineId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all executions with optional filtering
    /// </summary>
    [HttpGet("executions")]
    [ProducesResponseType(typeof(IEnumerable<PipelineExecution>), 200)]
    public async Task<ActionResult<IEnumerable<PipelineExecution>>> GetExecutions(
        [FromQuery] string? pipelineId = null,
        [FromQuery] string? status = null)
    {
        try
        {
            var executions = await _executionRepository.GetAllAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(pipelineId))
            {
                executions = executions.Where(e => pipelineId.Equals(e.PipelineId, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ExecutionStatus>(status, true, out var parsedStatus))
            {
                executions = executions.Where(e => e.Status == parsedStatus);
            }

            return Ok(executions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve executions");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get execution by ID
    /// </summary>
    [HttpGet("executions/{executionId}")]
    [ProducesResponseType(typeof(PipelineExecution), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PipelineExecution>> GetExecution(string executionId)
    {
        try
        {
            var execution = await _executionRepository.GetByIdAsync(executionId);
            
            if (execution == null)
            {
                return NotFound();
            }

            return Ok(execution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve execution {ExecutionId}", executionId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Cancel a running execution
    /// </summary>
    [HttpPost("executions/{executionId}/cancel")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CancelExecution(string executionId)
    {
        try
        {
            var execution = await _executionRepository.GetByIdAsync(executionId);
            if (execution == null)
            {
                return NotFound();
            }

            if (execution.Status != ExecutionStatus.Running && execution.Status != ExecutionStatus.Queued)
            {
                return BadRequest(new { error = "Can only cancel running or queued executions" });
            }

            var cancelledExecution = execution with 
            { 
                Status = ExecutionStatus.Cancelled,
                CompletedAt = DateTime.UtcNow
            };

            await _executionRepository.UpdateAsync(cancelledExecution);
            
            return Ok(new { message = "Execution cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel execution {ExecutionId}", executionId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get execution logs
    /// </summary>
    [HttpGet("executions/{executionId}/logs")]
    [ProducesResponseType(typeof(IEnumerable<LogEntry>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<LogEntry>>> GetExecutionLogs(
        string executionId,
        [FromQuery] string? stage = null)
    {
        try
        {
            var logs = string.IsNullOrEmpty(stage) 
                ? await _executionRepository.GetExecutionLogsAsync(executionId)
                : await _executionRepository.GetExecutionLogsByStageAsync(executionId, stage);

            if (!logs.Any())
            {
                var execution = await _executionRepository.GetByIdAsync(executionId);
                if (execution == null)
                {
                    return NotFound();
                }
            }

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve logs for execution {ExecutionId}", executionId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}