using C4Engineering.Web.Models.Pipeline;

namespace C4Engineering.Web.Data.Repositories;

/// <summary>
/// Repository interface for pipeline operations
/// </summary>
public interface IPipelineRepository
{
    Task<IEnumerable<PipelineConfiguration>> GetAllAsync();
    Task<PipelineConfiguration?> GetByIdAsync(string id);
    Task<PipelineConfiguration> CreateAsync(PipelineConfiguration pipeline);
    Task<PipelineConfiguration> UpdateAsync(PipelineConfiguration pipeline);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<PipelineConfiguration>> FindByServiceIdAsync(string serviceId);
    Task<bool> ExistsAsync(string id);
}