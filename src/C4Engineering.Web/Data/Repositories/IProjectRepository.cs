using C4Engineering.Web.Models.Project;

namespace C4Engineering.Web.Data.Repositories;

/// <summary>
/// Repository interface for project data operations
/// </summary>
public interface IProjectRepository
{
    Task<IEnumerable<ProjectModel>> GetAllAsync();
    Task<ProjectModel?> GetByIdAsync(string id);
    Task<ProjectModel> CreateAsync(ProjectModel project);
    Task<ProjectModel> UpdateAsync(ProjectModel project);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<IEnumerable<ProjectModel>> FindByOwnerAsync(string owner);
    Task<IEnumerable<ProjectModel>> FindByTypeAsync(ProjectType type);
    Task<IEnumerable<ProjectModel>> FindByStatusAsync(ProjectStatus status);
    Task<IEnumerable<ProjectModel>> SearchAsync(string searchTerm);
    Task<ProjectModel> AddServiceAsync(string projectId, string serviceId);
    Task<ProjectModel> RemoveServiceAsync(string projectId, string serviceId);
    Task<ProjectModel> AddDiagramAsync(string projectId, string diagramId);
    Task<ProjectModel> RemoveDiagramAsync(string projectId, string diagramId);
    Task<ProjectModel> AddTeamMemberAsync(string projectId, ProjectTeamMember member);
    Task<ProjectModel> RemoveTeamMemberAsync(string projectId, string memberEmail);
}