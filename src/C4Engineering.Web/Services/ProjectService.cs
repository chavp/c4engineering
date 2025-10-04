using C4Engineering.Web.Data.Repositories;
using C4Engineering.Web.Models.Project;

namespace C4Engineering.Web.Services;

/// <summary>
/// Service implementation for project business logic
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IDiagramRepository _diagramRepository;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IProjectRepository projectRepository,
        IServiceRepository serviceRepository,
        IDiagramRepository diagramRepository,
        ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _serviceRepository = serviceRepository;
        _diagramRepository = diagramRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ProjectSummary>> GetAllProjectSummariesAsync()
    {
        try
        {
            var projects = await _projectRepository.GetAllAsync();
            return projects.Select(p => new ProjectSummary
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Owner = p.Owner,
                Type = p.Type.ToString(),
                Status = p.Status.ToString(),
                ServiceCount = p.Services.Count,
                DiagramCount = p.Diagrams.Count,
                PipelineCount = p.Pipelines.Count,
                TeamMemberCount = p.TeamMembers.Count,
                UpdatedAt = p.Metadata.UpdatedAt,
                Tags = p.Tags
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project summaries");
            throw;
        }
    }

    public async Task<ProjectModel?> GetProjectByIdAsync(string id)
    {
        try
        {
            return await _projectRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project {ProjectId}", id);
            throw;
        }
    }

    public async Task<ProjectModel> CreateProjectAsync(CreateProjectRequest request)
    {
        try
        {
            // Validate request
            if (await _projectRepository.ExistsAsync(request.Id))
            {
                throw new InvalidOperationException($"Project with ID '{request.Id}' already exists");
            }

            // Parse project type
            if (!Enum.TryParse<ProjectType>(request.Type, true, out var projectType))
            {
                throw new ArgumentException($"Invalid project type: {request.Type}");
            }

            // Create project model
            var project = new ProjectModel
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description,
                Owner = request.Owner,
                Type = projectType,
                Status = ProjectStatus.Planning,
                Tags = request.Tags ?? new List<string>(),
                Settings = request.Settings ?? new ProjectSettings(),
                TeamMembers = new List<ProjectTeamMember>
                {
                    new()
                    {
                        Name = request.Owner,
                        Email = $"{request.Owner.ToLower().Replace(" ", ".")}@company.com",
                        Role = ProjectRole.Owner,
                        JoinedAt = DateTime.UtcNow
                    }
                }
            };

            var createdProject = await _projectRepository.CreateAsync(project);
            _logger.LogInformation("Created project {ProjectId} for owner {Owner}", request.Id, request.Owner);
            
            return createdProject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project {ProjectId}", request.Id);
            throw;
        }
    }

    public async Task<ProjectModel> UpdateProjectAsync(string id, UpdateProjectRequest request)
    {
        try
        {
            var existingProject = await _projectRepository.GetByIdAsync(id);
            if (existingProject == null)
            {
                throw new InvalidOperationException($"Project with ID '{id}' not found");
            }

            // Parse status if provided
            ProjectStatus? status = null;
            if (!string.IsNullOrEmpty(request.Status))
            {
                if (!Enum.TryParse<ProjectStatus>(request.Status, true, out var parsedStatus))
                {
                    throw new ArgumentException($"Invalid project status: {request.Status}");
                }
                status = parsedStatus;
            }

            // Update project with provided values
            var updatedProject = existingProject with
            {
                Name = request.Name ?? existingProject.Name,
                Description = request.Description ?? existingProject.Description,
                Owner = request.Owner ?? existingProject.Owner,
                Status = status ?? existingProject.Status,
                Tags = request.Tags ?? existingProject.Tags,
                Settings = request.Settings ?? existingProject.Settings
            };

            var result = await _projectRepository.UpdateAsync(updatedProject);
            _logger.LogInformation("Updated project {ProjectId}", id);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteProjectAsync(string id)
    {
        try
        {
            var result = await _projectRepository.DeleteAsync(id);
            if (result)
            {
                _logger.LogInformation("Deleted project {ProjectId}", id);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ProjectSummary>> SearchProjectsAsync(string searchTerm)
    {
        try
        {
            var projects = await _projectRepository.SearchAsync(searchTerm);
            return projects.Select(p => new ProjectSummary
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Owner = p.Owner,
                Type = p.Type.ToString(),
                Status = p.Status.ToString(),
                ServiceCount = p.Services.Count,
                DiagramCount = p.Diagrams.Count,
                PipelineCount = p.Pipelines.Count,
                TeamMemberCount = p.TeamMembers.Count,
                UpdatedAt = p.Metadata.UpdatedAt,
                Tags = p.Tags
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching projects with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<IEnumerable<ProjectTemplate>> GetProjectTemplatesAsync()
    {
        // Return predefined templates
        return new List<ProjectTemplate>
        {
            new()
            {
                Id = "web-app-template",
                Name = "Web Application",
                Description = "Full-stack web application with frontend, backend, and database",
                Category = "Web Development",
                DefaultType = ProjectType.WebApplication,
                PreConfiguredServices = new List<string> { "frontend-service", "backend-api", "database" },
                DefaultTags = new List<string> { "web", "full-stack", "api" },
                DefaultSettings = new ProjectSettings { IsPublic = true, EnableNotifications = true }
            },
            new()
            {
                Id = "microservices-template",
                Name = "Microservices Architecture",
                Description = "Microservices-based system with multiple services and API gateway",
                Category = "Architecture",
                DefaultType = ProjectType.Microservices,
                PreConfiguredServices = new List<string> { "api-gateway", "user-service", "notification-service" },
                DefaultTags = new List<string> { "microservices", "distributed", "api" },
                DefaultSettings = new ProjectSettings { IsPublic = true, EnableNotifications = true }
            },
            new()
            {
                Id = "data-platform-template",
                Name = "Data Platform",
                Description = "Data processing and analytics platform",
                Category = "Data & Analytics",
                DefaultType = ProjectType.DataPlatform,
                PreConfiguredServices = new List<string> { "data-ingestion", "data-processing", "analytics-api" },
                DefaultTags = new List<string> { "data", "analytics", "etl" },
                DefaultSettings = new ProjectSettings { IsPublic = false, EnableNotifications = true }  
            }
        };
    }

    public async Task<ProjectModel> CreateProjectFromTemplateAsync(string templateId, CreateProjectRequest request)
    {
        try
        {
            var templates = await GetProjectTemplatesAsync();
            var template = templates.FirstOrDefault(t => t.Id == templateId);
            if (template == null)
            {
                throw new InvalidOperationException($"Template with ID '{templateId}' not found");
            }

            // Create project with template defaults
            var templateRequest = request with
            {
                Type = template.DefaultType.ToString(),
                Tags = (request.Tags ?? new List<string>()).Concat(template.DefaultTags).Distinct().ToList(),
                Settings = request.Settings ?? template.DefaultSettings
            };

            var project = await CreateProjectAsync(templateRequest);
            
            // Update metadata with template info
            var updatedProject = project with
            {
                Metadata = project.Metadata with { Template = templateId }
            };

            return await _projectRepository.UpdateAsync(updatedProject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project from template {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<ProjectModel> AddServiceToProjectAsync(string projectId, string serviceId)
    {
        try
        {
            // Verify service exists
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service == null)
            {
                throw new InvalidOperationException($"Service with ID '{serviceId}' not found");
            }

            return await _projectRepository.AddServiceAsync(projectId, serviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding service {ServiceId} to project {ProjectId}", serviceId, projectId);
            throw;
        }
    }

    public async Task<ProjectModel> RemoveServiceFromProjectAsync(string projectId, string serviceId)
    {
        try
        {
            return await _projectRepository.RemoveServiceAsync(projectId, serviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing service {ServiceId} from project {ProjectId}", serviceId, projectId);
            throw;
        }
    }

    public async Task<ProjectModel> AddDiagramToProjectAsync(string projectId, string diagramId)
    {
        try
        {
            // Verify diagram exists
            var diagram = await _diagramRepository.GetByIdAsync(diagramId);
            if (diagram == null)
            {
                throw new InvalidOperationException($"Diagram with ID '{diagramId}' not found");
            }

            return await _projectRepository.AddDiagramAsync(projectId, diagramId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding diagram {DiagramId} to project {ProjectId}", diagramId, projectId);
            throw;
        }
    }

    public async Task<ProjectModel> RemoveDiagramFromProjectAsync(string projectId, string diagramId)
    {
        try
        {
            return await _projectRepository.RemoveDiagramAsync(projectId, diagramId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing diagram {DiagramId} from project {ProjectId}", diagramId, projectId);
            throw;
        }
    }

    public async Task<ProjectModel> AddTeamMemberAsync(string projectId, AddTeamMemberRequest request)
    {
        try
        {
            if (!Enum.TryParse<ProjectRole>(request.Role, true, out var role))
            {
                throw new ArgumentException($"Invalid project role: {request.Role}");
            }

            var member = new ProjectTeamMember
            {
                Name = request.Name,
                Email = request.Email,
                Role = role,
                JoinedAt = DateTime.UtcNow
            };

            return await _projectRepository.AddTeamMemberAsync(projectId, member);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding team member {Email} to project {ProjectId}", request.Email, projectId);
            throw;
        }
    }

    public async Task<ProjectModel> RemoveTeamMemberAsync(string projectId, string memberEmail)
    {
        try
        {
            return await _projectRepository.RemoveTeamMemberAsync(projectId, memberEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing team member {Email} from project {ProjectId}", memberEmail, projectId);
            throw;
        }
    }

    public async Task<bool> ProjectExistsAsync(string id)
    {
        try
        {
            return await _projectRepository.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if project {ProjectId} exists", id);
            throw;
        }
    }
}