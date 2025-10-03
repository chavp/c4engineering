using C4Engineering.Web.Models.Pipeline;

namespace C4Engineering.Web.Services;

/// <summary>
/// Service interface for managing build and deployment pipelines
/// </summary>
public interface IPipelineService
{
    Task<IEnumerable<PipelineConfiguration>> GetAllPipelinesAsync();
    Task<PipelineConfiguration?> GetPipelineByIdAsync(string id);
    Task<PipelineConfiguration> CreatePipelineAsync(PipelineConfiguration pipeline);
    Task<PipelineConfiguration> UpdatePipelineAsync(PipelineConfiguration pipeline);
    Task<bool> DeletePipelineAsync(string id);
    Task<IEnumerable<PipelineConfiguration>> GetPipelinesByServiceAsync(string serviceId);
    Task<PipelineExecution> ExecutePipelineAsync(string pipelineId, string? triggeredBy = null, Dictionary<string, string>? parameters = null);
    Task<PipelineExecution?> GetExecutionByIdAsync(string executionId);
    Task<IEnumerable<PipelineExecution>> GetPipelineExecutionsAsync(string pipelineId);
    Task<PipelineExecution> UpdateExecutionStatusAsync(string executionId, ExecutionStatus status);
    Task<PipelineExecution> AddLogEntryAsync(string executionId, LogEntry logEntry);
    Task<IEnumerable<LogEntry>> GetExecutionLogsAsync(string executionId);
    Task<bool> CancelExecutionAsync(string executionId);
    Task<bool> ExistsAsync(string id);
}