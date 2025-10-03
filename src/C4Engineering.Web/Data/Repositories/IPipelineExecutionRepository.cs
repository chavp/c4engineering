using C4Engineering.Web.Models.Pipeline;

namespace C4Engineering.Web.Data.Repositories;

/// <summary>
/// Repository interface for pipeline execution operations
/// </summary>
public interface IPipelineExecutionRepository
{
    Task<IEnumerable<PipelineExecution>> GetAllAsync();
    Task<PipelineExecution?> GetByIdAsync(string id);
    Task<PipelineExecution> CreateAsync(PipelineExecution execution);
    Task<PipelineExecution> UpdateAsync(PipelineExecution execution);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<PipelineExecution>> GetByPipelineIdAsync(string pipelineId);
    Task<IEnumerable<PipelineExecution>> GetByStatusAsync(ExecutionStatus status);
    Task<IEnumerable<LogEntry>> GetExecutionLogsAsync(string executionId);
    Task<IEnumerable<LogEntry>> GetExecutionLogsByStageAsync(string executionId, string stageName);
    Task<PipelineExecution> AddLogEntryAsync(string executionId, LogEntry logEntry);
    Task<bool> ExistsAsync(string id);
}