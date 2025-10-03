using System.ComponentModel.DataAnnotations;
using C4Engineering.Web.Models.ServiceCatalog;

namespace C4Engineering.Web.Models.Requests;

/// <summary>
/// Request model for creating a new service
/// </summary>
public record CreateServiceRequest
{
    public string? Id { get; init; }
    
    [Required]
    public string Name { get; init; } = string.Empty;
    
    public string? Description { get; init; }
    
    [Required]
    public string Type { get; init; } = string.Empty;
    
    [Required]
    public string Owner { get; init; } = string.Empty;
    
    public string? Repository { get; init; }
    public string? Documentation { get; init; }
    public string? ApiSpec { get; init; }
    public List<string>? Tags { get; init; }
    
    [Required]
    public string Lifecycle { get; init; } = string.Empty;
    
    public string? System { get; init; }
    public List<string>? DependsOn { get; init; }
    public List<string>? ProvidesApis { get; init; }
    public List<string>? ConsumesApis { get; init; }
}

/// <summary>
/// Request model for updating an existing service
/// </summary>
public record UpdateServiceRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Type { get; init; }
    public string? Owner { get; init; }
    public string? Repository { get; init; }
    public string? Documentation { get; init; }
    public string? ApiSpec { get; init; }
    public List<string>? Tags { get; init; }
    public string? Lifecycle { get; init; }
    public string? System { get; init; }
    public List<string>? DependsOn { get; init; }
    public List<string>? ProvidesApis { get; init; }
    public List<string>? ConsumesApis { get; init; }
}