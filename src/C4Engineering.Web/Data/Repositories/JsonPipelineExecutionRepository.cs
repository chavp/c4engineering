using C4Engineering.Web.Models.Pipeline;
using Microsoft.Extensions.Options;

namespace C4Engineering.Web.Data.Repositories;

/// <summary>
/// JSON file-based implementation of pipeline execution repository
/// </summary>
public class JsonPipelineExecutionRepository : JsonDataStore<PipelineExecution>, IPipelineExecutionRepository
{
    public JsonPipelineExecutionRepository(IOptions<JsonDataOptions> options, ILogger<JsonPipelineExecutionRepository> logger)
        : base(options, logger, "pipeline-executions")
    {
    }

    public async Task<IEnumerable<PipelineExecution>> GetAllAsync()
    {
        return await ReadAllEntitiesAsync();
    }

    public async Task<PipelineExecution?> GetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Pipeline execution ID cannot be null or empty", nameof(id));

        return await ReadEntityAsync(id);
    }

    public async Task<PipelineExecution> CreateAsync(PipelineExecution execution)
    {
        if (execution == null)
            throw new ArgumentNullException(nameof(execution));

        if (string.IsNullOrEmpty(execution.Id))
            throw new ArgumentException("Pipeline execution ID cannot be null or empty", nameof(execution));

        if (await EntityExistsAsync(execution.Id))
            throw new InvalidOperationException($"Pipeline execution with ID '{execution.Id}' already exists");

        return await WriteEntityAsync(execution.Id, execution);
    }

    public async Task<PipelineExecution> UpdateAsync(PipelineExecution execution)
    {
        if (execution == null)
            throw new ArgumentNullException(nameof(execution));

        if (string.IsNullOrEmpty(execution.Id))
            throw new ArgumentException("Pipeline execution ID cannot be null or empty", nameof(execution));

        if (!await EntityExistsAsync(execution.Id))
            throw new InvalidOperationException($"Pipeline execution with ID '{execution.Id}' does not exist");

        return await WriteEntityAsync(execution.Id, execution);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Pipeline execution ID cannot be null or empty", nameof(id));

        return await DeleteEntityAsync(id);
    }

    public async Task<IEnumerable<PipelineExecution>> GetByPipelineIdAsync(string pipelineId)
    {
        if (string.IsNullOrEmpty(pipelineId))
            return Enumerable.Empty<PipelineExecution>();

        return await FilterEntitiesAsync(execution => 
            execution.PipelineId.Equals(pipelineId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<PipelineExecution>> GetByStatusAsync(ExecutionStatus status)
    {
        return await FilterEntitiesAsync(execution => execution.Status == status);
    }

    public async Task<IEnumerable<LogEntry>> GetExecutionLogsAsync(string executionId)
    {
        var execution = await GetByIdAsync(executionId);
        return execution?.Logs?.Select(log => new LogEntry
        {
            Timestamp = log.Timestamp,
            Level = log.Level,
            Message = log.Message,
            StageId = log.StageId,
            Stage = log.Stage
        }) ?? Enumerable.Empty<LogEntry>();
    }

    public async Task<IEnumerable<LogEntry>> GetExecutionLogsByStageAsync(string executionId, string stageName)
    {
        var logs = await GetExecutionLogsAsync(executionId);
        return logs.Where(log => stageName.Equals(log.Stage, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<PipelineExecution> AddLogEntryAsync(string executionId, LogEntry logEntry)
    {
        var execution = await GetByIdAsync(executionId);
        if (execution == null)
            throw new InvalidOperationException($"Execution with ID '{executionId}' does not exist");

        var updatedLogs = new List<ExecutionLogEntry>(execution.Logs) 
        { 
            new ExecutionLogEntry
            {
                Timestamp = logEntry.Timestamp,
                Level = logEntry.Level,
                Message = logEntry.Message,
                StageId = logEntry.StageId,
                Stage = logEntry.Stage
            }
        };
        var updatedExecution = execution with { Logs = updatedLogs };

        return await UpdateAsync(updatedExecution);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        return await EntityExistsAsync(id);
    }
}