using C4Engineering.Web.Models.Pipeline;
using Microsoft.Extensions.Options;

namespace C4Engineering.Web.Data.Repositories;

/// <summary>
/// JSON file-based implementation of pipeline repository
/// </summary>
public class JsonPipelineRepository : JsonDataStore<PipelineConfiguration>, IPipelineRepository
{
    public JsonPipelineRepository(IOptions<JsonDataOptions> options, ILogger<JsonPipelineRepository> logger)
        : base(options, logger, "pipelines")
    {
    }

    public async Task<IEnumerable<PipelineConfiguration>> GetAllAsync()
    {
        return await ReadAllEntitiesAsync();
    }

    public async Task<PipelineConfiguration?> GetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(id));

        return await ReadEntityAsync(id);
    }

    public async Task<PipelineConfiguration> CreateAsync(PipelineConfiguration pipeline)
    {
        if (pipeline == null)
            throw new ArgumentNullException(nameof(pipeline));

        if (string.IsNullOrEmpty(pipeline.Id))
            throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(pipeline));

        if (await EntityExistsAsync(pipeline.Id))
            throw new InvalidOperationException($"Pipeline with ID '{pipeline.Id}' already exists");

        // Set metadata
        var pipelineWithMetadata = pipeline with
        {
            Metadata = pipeline.Metadata with
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        return await WriteEntityAsync(pipeline.Id, pipelineWithMetadata);
    }

    public async Task<PipelineConfiguration> UpdateAsync(PipelineConfiguration pipeline)
    {
        if (pipeline == null)
            throw new ArgumentNullException(nameof(pipeline));

        if (string.IsNullOrEmpty(pipeline.Id))
            throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(pipeline));

        if (!await EntityExistsAsync(pipeline.Id))
            throw new InvalidOperationException($"Pipeline with ID '{pipeline.Id}' does not exist");

        // Update metadata
        var pipelineWithMetadata = pipeline with
        {
            Metadata = pipeline.Metadata with
            {
                UpdatedAt = DateTime.UtcNow
            }
        };

        return await WriteEntityAsync(pipeline.Id, pipelineWithMetadata);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(id));

        return await DeleteEntityAsync(id);
    }

    public async Task<IEnumerable<PipelineConfiguration>> FindByServiceIdAsync(string serviceId)
    {
        if (string.IsNullOrEmpty(serviceId))
            return Enumerable.Empty<PipelineConfiguration>();

        return await FilterEntitiesAsync(pipeline => 
            serviceId.Equals(pipeline.ServiceId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> ExistsAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        return await EntityExistsAsync(id);
    }
}