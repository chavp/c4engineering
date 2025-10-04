using C4Engineering.Web.Models.Project;
using Microsoft.Extensions.Options;

namespace C4Engineering.Web.Data.Repositories;

/// <summary>
/// JSON file-based implementation of project repository
/// </summary>
public class JsonProjectRepository : JsonDataStore<ProjectModel>, IProjectRepository
{
    public JsonProjectRepository(IOptions<JsonDataOptions> options, ILogger<JsonProjectRepository> logger)
        : base(options, logger, "projects")
    {
    }

    public async Task<IEnumerable<ProjectModel>> GetAllAsync()
    {
        return await ReadAllEntitiesAsync();
    }

    public async Task<ProjectModel?> GetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Project ID cannot be null or empty", nameof(id));

        return await ReadEntityAsync(id);
    }

    public async Task<ProjectModel> CreateAsync(ProjectModel project)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));

        if (string.IsNullOrEmpty(project.Id))
            throw new ArgumentException("Project ID cannot be null or empty", nameof(project));

        if (await EntityExistsAsync(project.Id))
            throw new InvalidOperationException($"Project with ID '{project.Id}' already exists");

        // Set metadata
        var projectWithMetadata = project with
        {
            Metadata = project.Metadata with
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        return await WriteEntityAsync(project.Id, projectWithMetadata);
    }

    public async Task<ProjectModel> UpdateAsync(ProjectModel project)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));

        if (string.IsNullOrEmpty(project.Id))
            throw new ArgumentException("Project ID cannot be null or empty", nameof(project));

        if (!await EntityExistsAsync(project.Id))
            throw new InvalidOperationException($"Project with ID '{project.Id}' does not exist");

        // Update metadata
        var projectWithMetadata = project with
        {
            Metadata = project.Metadata with
            {
                UpdatedAt = DateTime.UtcNow
            }
        };

        return await WriteEntityAsync(project.Id, projectWithMetadata);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Project ID cannot be null or empty", nameof(id));

        return await DeleteEntityAsync(id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        return await EntityExistsAsync(id);
    }

    public async Task<IEnumerable<ProjectModel>> FindByOwnerAsync(string owner)
    {
        if (string.IsNullOrEmpty(owner))
            return Enumerable.Empty<ProjectModel>();

        return await FilterEntitiesAsync(project => 
            owner.Equals(project.Owner, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<ProjectModel>> FindByTypeAsync(ProjectType type)
    {
        return await FilterEntitiesAsync(project => project.Type == type);
    }

    public async Task<IEnumerable<ProjectModel>> FindByStatusAsync(ProjectStatus status)
    {
        return await FilterEntitiesAsync(project => project.Status == status);
    }

    public async Task<IEnumerable<ProjectModel>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
            return await GetAllAsync();

        var lowerSearchTerm = searchTerm.ToLowerInvariant();
        return await FilterEntitiesAsync(project =>
            project.Name.ToLowerInvariant().Contains(lowerSearchTerm) ||
            (project.Description?.ToLowerInvariant().Contains(lowerSearchTerm) ?? false) ||
            project.Owner.ToLowerInvariant().Contains(lowerSearchTerm) ||
            project.Tags.Any(tag => tag.ToLowerInvariant().Contains(lowerSearchTerm)));
    }

    public async Task<ProjectModel> AddServiceAsync(string projectId, string serviceId)
    {
        var project = await GetByIdAsync(projectId);
        if (project == null)
            throw new InvalidOperationException($"Project with ID '{projectId}' does not exist");

        if (project.Services.Contains(serviceId))
            throw new InvalidOperationException($"Service '{serviceId}' is already part of project");

        var updatedServices = new List<string>(project.Services) { serviceId };
        var updatedProject = project with { Services = updatedServices };

        return await UpdateAsync(updatedProject);
    }

    public async Task<ProjectModel> RemoveServiceAsync(string projectId, string serviceId)
    {
        var project = await GetByIdAsync(projectId);
        if (project == null)
            throw new InvalidOperationException($"Project with ID '{projectId}' does not exist");

        var updatedServices = project.Services.Where(s => s != serviceId).ToList();
        if (updatedServices.Count == project.Services.Count)
            throw new InvalidOperationException($"Service '{serviceId}' is not part of project");

        var updatedProject = project with { Services = updatedServices };
        return await UpdateAsync(updatedProject);
    }

    public async Task<ProjectModel> AddDiagramAsync(string projectId, string diagramId)
    {
        var project = await GetByIdAsync(projectId);
        if (project == null)
            throw new InvalidOperationException($"Project with ID '{projectId}' does not exist");

        if (project.Diagrams.Contains(diagramId))
            throw new InvalidOperationException($"Diagram '{diagramId}' is already part of project");

        var updatedDiagrams = new List<string>(project.Diagrams) { diagramId };
        var updatedProject = project with { Diagrams = updatedDiagrams };

        return await UpdateAsync(updatedProject);
    }

    public async Task<ProjectModel> RemoveDiagramAsync(string projectId, string diagramId)  
    {
        var project = await GetByIdAsync(projectId);
        if (project == null)
            throw new InvalidOperationException($"Project with ID '{projectId}' does not exist");

        var updatedDiagrams = project.Diagrams.Where(d => d != diagramId).ToList();
        if (updatedDiagrams.Count == project.Diagrams.Count)
            throw new InvalidOperationException($"Diagram '{diagramId}' is not part of project");

        var updatedProject = project with { Diagrams = updatedDiagrams };
        return await UpdateAsync(updatedProject);
    }

    public async Task<ProjectModel> AddTeamMemberAsync(string projectId, ProjectTeamMember member)
    {
        var project = await GetByIdAsync(projectId);
        if (project == null)
            throw new InvalidOperationException($"Project with ID '{projectId}' does not exist");

        if (project.TeamMembers.Any(m => m.Email.Equals(member.Email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Team member with email '{member.Email}' already exists in project");

        var updatedMembers = new List<ProjectTeamMember>(project.TeamMembers) { member };
        var updatedProject = project with { TeamMembers = updatedMembers };

        return await UpdateAsync(updatedProject);
    }

    public async Task<ProjectModel> RemoveTeamMemberAsync(string projectId, string memberEmail)
    {
        var project = await GetByIdAsync(projectId);
        if (project == null)
            throw new InvalidOperationException($"Project with ID '{projectId}' does not exist");

        var updatedMembers = project.TeamMembers
            .Where(m => !m.Email.Equals(memberEmail, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (updatedMembers.Count == project.TeamMembers.Count)
            throw new InvalidOperationException($"Team member with email '{memberEmail}' is not part of project");

        var updatedProject = project with { TeamMembers = updatedMembers };
        return await UpdateAsync(updatedProject);
    }
}