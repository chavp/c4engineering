using C4Engineering.Web.Models.Project;

namespace C4Engineering.Web.Services;

/// <summary>
/// Service interface for project business logic
/// </summary>
public interface IProjectService
{
    Task<IEnumerable<ProjectSummary>> GetAllProjectSummariesAsync();
    Task<ProjectModel?> GetProjectByIdAsync(string id);
    Task<ProjectModel> CreateProjectAsync(CreateProjectRequest request);
    Task<ProjectModel> UpdateProjectAsync(string id, UpdateProjectRequest request);
    Task<bool> DeleteProjectAsync(string id);
    Task<IEnumerable<ProjectSummary>> SearchProjectsAsync(string searchTerm);
    Task<IEnumerable<ProjectTemplate>> GetProjectTemplatesAsync();
    Task<ProjectModel> CreateProjectFromTemplateAsync(string templateId, CreateProjectRequest request);
    Task<ProjectModel> AddServiceToProjectAsync(string projectId, string serviceId);
    Task<ProjectModel> RemoveServiceFromProjectAsync(string projectId, string serviceId);
    Task<ProjectModel> AddDiagramToProjectAsync(string projectId, string diagramId);
    Task<ProjectModel> RemoveDiagramFromProjectAsync(string projectId, string diagramId);
    Task<ProjectModel> AddTeamMemberAsync(string projectId, AddTeamMemberRequest request);
    Task<ProjectModel> RemoveTeamMemberAsync(string projectId, string memberEmail);
    Task<bool> ProjectExistsAsync(string id);
}