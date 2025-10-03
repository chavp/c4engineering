using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace C4Engineering.Tests.Contract;

/// <summary>
/// Contract tests for Pipeline API endpoints based on OpenAPI specification
/// These tests MUST FAIL initially to enforce TDD methodology
/// </summary>
public class PipelineApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public PipelineApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task GetPipelines_ReturnsSuccessStatusCode()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/pipelines");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetPipelines_WithServiceIdFilter_ReturnsFilteredResults()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/pipelines?serviceId=payment-service");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pipelines = JsonSerializer.Deserialize<PipelineSummary[]>(content, _jsonOptions);
        
        Assert.NotNull(pipelines);
        Assert.All(pipelines, pipeline => Assert.Equal("payment-service", pipeline.ServiceId));
    }

    [Fact]
    public async Task GetPipelineById_ExistingPipeline_ReturnsPipeline()
    {
        // Arrange
        var pipelineId = "payment-service-pipeline";

        // Act
        var response = await _client.GetAsync($"/api/pipelines/{pipelineId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pipeline = JsonSerializer.Deserialize<PipelineConfiguration>(content, _jsonOptions);
        
        Assert.NotNull(pipeline);
        Assert.Equal(pipelineId, pipeline.Id);
        Assert.NotNull(pipeline.Stages);
    }

    [Fact]
    public async Task PostPipeline_ValidPipeline_ReturnsCreatedPipeline()
    {
        // Arrange
        var newPipeline = new CreatePipelineRequest
        {
            ServiceId = "test-service",
            Name = "Test Service Pipeline",
            Description = "A test pipeline for contract testing",
            Stages = new List<PipelineStage>
            {
                new PipelineStage
                {
                    Id = "build",
                    Name = "Build",
                    Type = "build",
                    Commands = new List<string> { "dotnet build" },
                    Timeout = 300
                }
            }
        };

        var json = JsonSerializer.Serialize(newPipeline, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/pipelines", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdPipeline = JsonSerializer.Deserialize<PipelineConfiguration>(responseContent, _jsonOptions);
        
        Assert.NotNull(createdPipeline);
        Assert.Equal(newPipeline.ServiceId, createdPipeline.ServiceId);
        Assert.Equal(newPipeline.Name, createdPipeline.Name);
    }

    [Fact]
    public async Task PutPipeline_ExistingPipeline_ReturnsSuccess()
    {
        // Arrange
        var pipelineId = "payment-service-pipeline";
        var updateRequest = new UpdatePipelineRequest
        {
            Name = "Updated Payment Service Pipeline",
            Description = "Updated pipeline description"
        };

        var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/pipelines/{pipelineId}", content);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task PostPipelineExecution_ValidPipeline_ReturnsAccepted()
    {
        // Arrange
        var pipelineId = "payment-service-pipeline";
        var executeRequest = new ExecutePipelineRequest
        {
            TriggeredBy = "test-user",
            Parameters = new Dictionary<string, string>
            {
                { "BUILD_VERSION", "1.0.0" }
            }
        };

        var json = JsonSerializer.Serialize(executeRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/pipelines/{pipelineId}/execute", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Accepted, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var execution = JsonSerializer.Deserialize<PipelineExecution>(responseContent, _jsonOptions);
        
        Assert.NotNull(execution);
        Assert.Equal(pipelineId, execution.PipelineId);
        Assert.Equal("queued", execution.Status);
    }

    [Fact]
    public async Task GetExecutions_ReturnsSuccessStatusCode()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/executions");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetExecutions_WithPipelineIdFilter_ReturnsFilteredResults()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/executions?pipelineId=payment-service-pipeline");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var executions = JsonSerializer.Deserialize<PipelineExecution[]>(content, _jsonOptions);
        
        Assert.NotNull(executions);
        Assert.All(executions, execution => Assert.Equal("payment-service-pipeline", execution.PipelineId));
    }

    [Fact]
    public async Task GetExecutions_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/executions?status=success");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var executions = JsonSerializer.Deserialize<PipelineExecution[]>(content, _jsonOptions);
        
        Assert.NotNull(executions);
        Assert.All(executions, execution => Assert.Equal("success", execution.Status));
    }

    [Fact]
    public async Task GetExecutionById_ExistingExecution_ReturnsExecution()
    {
        // Arrange
        var executionId = "exec-payment-service-20250110-001";

        // Act
        var response = await _client.GetAsync($"/api/executions/{executionId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var execution = JsonSerializer.Deserialize<PipelineExecution>(content, _jsonOptions);
        
        Assert.NotNull(execution);
        Assert.Equal(executionId, execution.Id);
        Assert.NotNull(execution.Stages);
    }

    [Fact]
    public async Task GetExecutionLogs_ExistingExecution_ReturnsLogs()
    {
        // Arrange
        var executionId = "exec-payment-service-20250110-001";

        // Act
        var response = await _client.GetAsync($"/api/executions/{executionId}/logs");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var logs = JsonSerializer.Deserialize<LogEntry[]>(content, _jsonOptions);
        
        Assert.NotNull(logs);
    }

    [Fact]
    public async Task GetExecutionLogs_WithStageFilter_ReturnsFilteredLogs()
    {
        // Arrange
        var executionId = "exec-payment-service-20250110-001";

        // Act
        var response = await _client.GetAsync($"/api/executions/{executionId}/logs?stageId=build");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var logs = JsonSerializer.Deserialize<LogEntry[]>(content, _jsonOptions);
        
        Assert.NotNull(logs);
        Assert.All(logs, log => Assert.Equal("build", log.StageId));
    }

    [Fact]
    public async Task PostExecutionCancel_RunningExecution_ReturnsSuccess()
    {
        // Arrange
        var executionId = "exec-payment-service-running-001";

        // Act
        var response = await _client.PostAsync($"/api/executions/{executionId}/cancel", null);

        // Assert
        response.EnsureSuccessStatusCode();
    }
}

// Data models for contract testing - these MUST match the OpenAPI specification
public class PipelineConfiguration
{
    public string Id { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<PipelineStage> Stages { get; set; } = new();
    public PipelineTriggers? Triggers { get; set; }
    public PipelineMetadata? Metadata { get; set; }
}

public class PipelineSummary
{
    public string Id { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int StageCount { get; set; }
    public ExecutionSummary? LastExecution { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PipelineStage
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<string> Commands { get; set; } = new();
    public string? WorkingDirectory { get; set; }
    public string? DockerFile { get; set; }
    public string? ImageName { get; set; }
    public string? ImageTag { get; set; }
    public string? ContainerName { get; set; }
    public List<string> Ports { get; set; } = new();
    public Dictionary<string, string> Environment { get; set; } = new();
    public List<string> Volumes { get; set; } = new();
    public Dictionary<string, string> BuildArgs { get; set; } = new();
    public HealthCheck? HealthCheck { get; set; }
    public int Timeout { get; set; } = 300;
    public int RetryCount { get; set; } = 0;
}

public class PipelineTriggers
{
    public bool Manual { get; set; } = true;
    public bool OnDiagramChange { get; set; } = false;
    public bool OnRepositoryChange { get; set; } = false;
    public string? Scheduled { get; set; }
}

public class PipelineExecution
{
    public string Id { get; set; } = string.Empty;
    public string PipelineId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? Duration { get; set; }
    public string? TriggeredBy { get; set; }
    public int BuildNumber { get; set; }
    public List<StageExecution> Stages { get; set; } = new();
}

public class StageExecution
{
    public string StageId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? Duration { get; set; }
    public List<string> Logs { get; set; } = new();
    public List<string> Artifacts { get; set; } = new();
    public string? DeploymentUrl { get; set; }
}

public class ExecutionSummary
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public int? Duration { get; set; }
    public int BuildNumber { get; set; }
}

public class HealthCheck
{
    public string? Url { get; set; }
    public int Interval { get; set; } = 10;
    public int Timeout { get; set; } = 5;
    public int Retries { get; set; } = 3;
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StageId { get; set; }
}

public class PipelineMetadata
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class CreatePipelineRequest
{
    public string ServiceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<PipelineStage> Stages { get; set; } = new();
    public PipelineTriggers? Triggers { get; set; }
}

public class UpdatePipelineRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<PipelineStage>? Stages { get; set; }
    public PipelineTriggers? Triggers { get; set; }
}

public class ExecutePipelineRequest
{
    public string? TriggeredBy { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
}