using Microsoft.AspNetCore.SignalR;
using C4Engineering.Web.Services;
using C4Engineering.Web.Models.Pipeline;

namespace C4Engineering.Web.Hubs;

/// <summary>
/// SignalR hub for real-time pipeline status updates
/// </summary>
public class PipelineStatusHub : Hub
{
    private readonly IPipelineService _pipelineService;
    private readonly ILogger<PipelineStatusHub> _logger;

    public PipelineStatusHub(IPipelineService pipelineService, ILogger<PipelineStatusHub> logger)
    {
        _pipelineService = pipelineService;
        _logger = logger;
    }

    public async Task JoinPipeline(string pipelineId)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"pipeline-{pipelineId}");
            
            _logger.LogDebug("User {UserId} joined pipeline {PipelineId}", Context.UserIdentifier, pipelineId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining pipeline {PipelineId}", pipelineId);
            throw;
        }
    }

    public async Task LeavePipeline(string pipelineId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"pipeline-{pipelineId}");
            
            _logger.LogDebug("User {UserId} left pipeline {PipelineId}", Context.UserIdentifier, pipelineId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving pipeline {PipelineId}", pipelineId);
            throw;
        }
    }

    public async Task JoinExecution(string executionId)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"execution-{executionId}");
            
            _logger.LogDebug("User {UserId} joined execution {ExecutionId}", Context.UserIdentifier, executionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining execution {ExecutionId}", executionId);
            throw;
        }
    }

    public async Task LeaveExecution(string executionId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"execution-{executionId}");
            
            _logger.LogDebug("User {UserId} left execution {ExecutionId}", Context.UserIdentifier, executionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving execution {ExecutionId}", executionId);
            throw;
        }
    }

    // Methods to be called by the pipeline service to broadcast updates
    public async Task NotifyExecutionStatusChanged(string executionId, ExecutionStatus status)
    {
        try
        {
            await Clients.Group($"execution-{executionId}")
                .SendAsync("ExecutionStatusChanged", executionId, status.ToString());
            
            _logger.LogDebug("Execution {ExecutionId} status changed to {Status}", executionId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying execution status change for {ExecutionId}", executionId);
            throw;
        }
    }

    public async Task NotifyStageStatusChanged(string executionId, string stageId, ExecutionStatus status)
    {
        try
        {
            await Clients.Group($"execution-{executionId}")
                .SendAsync("StageStatusChanged", executionId, stageId, status.ToString());
            
            _logger.LogDebug("Stage {StageId} in execution {ExecutionId} status changed to {Status}", stageId, executionId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying stage status change for {StageId} in {ExecutionId}", stageId, executionId);
            throw;
        }
    }

    public async Task NotifyLogEntry(string executionId, string stageId, ExecutionLogEntry logEntry)
    {
        try
        {
            await Clients.Group($"execution-{executionId}")
                .SendAsync("LogEntry", executionId, stageId, logEntry);
            
            _logger.LogDebug("Log entry added to stage {StageId} in execution {ExecutionId}", stageId, executionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying log entry for {StageId} in {ExecutionId}", stageId, executionId);
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogDebug("User {UserId} disconnected", Context.UserIdentifier);
        await base.OnDisconnectedAsync(exception);
    }
}