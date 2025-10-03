using Microsoft.OpenApi.Models;
using C4Engineering.Web.Data.Repositories;
using C4Engineering.Web.Services;
using C4Engineering.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "C4Engineering Platform API", 
        Version = "v1",
        Description = "Platform Engineering MVP with Backstage integration and C4 model visualization"
    });
});

// Configure JSON data directory
builder.Services.Configure<JsonDataOptions>(options =>
{
    options.DataDirectory = Path.Combine(builder.Environment.WebRootPath ?? "wwwroot", "data");
});

// Register repositories
builder.Services.AddScoped<IServiceRepository, JsonServiceRepository>();
builder.Services.AddScoped<IDiagramRepository, JsonDiagramRepository>();
builder.Services.AddScoped<IPipelineRepository, JsonPipelineRepository>();
builder.Services.AddScoped<IPipelineExecutionRepository, JsonPipelineExecutionRepository>();

// Register services
builder.Services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
builder.Services.AddScoped<IDiagramService, DiagramService>();
builder.Services.AddScoped<IPipelineService, PipelineService>();
builder.Services.AddScoped<IDockerDeploymentService, DockerDeploymentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "C4Engineering Platform API v1");
        c.RoutePrefix = "api-docs";
    });
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map SignalR hubs
app.MapHub<DiagramCollaborationHub>("/hubs/diagrams");
app.MapHub<PipelineStatusHub>("/hubs/pipelines");

app.Run();

// Configuration class for JSON data storage
public class JsonDataOptions
{
    public string DataDirectory { get; set; } = string.Empty;
}

// Make Program class accessible for testing
public partial class Program { }
