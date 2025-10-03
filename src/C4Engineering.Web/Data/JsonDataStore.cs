using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace C4Engineering.Web.Data;

/// <summary>
/// Base class for JSON file operations providing common functionality
/// </summary>
public abstract class JsonDataStore<T> where T : class
{
    protected readonly string _dataDirectory;
    protected readonly string _entityDirectory;
    protected readonly ILogger _logger;
    protected readonly JsonSerializerOptions _jsonOptions;

    protected JsonDataStore(IOptions<JsonDataOptions> options, ILogger logger, string entityType)
    {
        _dataDirectory = options.Value.DataDirectory;
        _entityDirectory = Path.Combine(_dataDirectory, entityType);
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        // Ensure directory exists
        Directory.CreateDirectory(_entityDirectory);
    }

    protected async Task<T?> ReadEntityAsync(string id)
    {
        var filePath = GetEntityFilePath(id);
        
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read entity {Id} from {FilePath}", id, filePath);
            throw;
        }
    }

    protected async Task<T> WriteEntityAsync(string id, T entity)
    {
        var filePath = GetEntityFilePath(id);
        
        try
        {
            var json = JsonSerializer.Serialize(entity, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            
            await UpdateIndexAsync();
            
            _logger.LogDebug("Successfully wrote entity {Id} to {FilePath}", id, filePath);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write entity {Id} to {FilePath}", id, filePath);
            throw;
        }
    }

    protected async Task<bool> DeleteEntityAsync(string id)
    {
        var filePath = GetEntityFilePath(id);
        
        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            File.Delete(filePath);
            await UpdateIndexAsync();
            
            _logger.LogDebug("Successfully deleted entity {Id} from {FilePath}", id, filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete entity {Id} from {FilePath}", id, filePath);
            throw;
        }
    }

    protected async Task<IEnumerable<T>> ReadAllEntitiesAsync()
    {
        var indexPath = GetIndexFilePath();
        
        if (!File.Exists(indexPath))
        {
            return Enumerable.Empty<T>();
        }

        try
        {
            var indexJson = await File.ReadAllTextAsync(indexPath);
            var entityIds = JsonSerializer.Deserialize<List<string>>(indexJson, _jsonOptions) ?? new List<string>();
            
            var entities = new List<T>();
            
            foreach (var id in entityIds)
            {
                var entity = await ReadEntityAsync(id);
                if (entity != null)
                {
                    entities.Add(entity);
                }
            }
            
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read all entities from {Directory}", _entityDirectory);
            throw;
        }
    }

    protected async Task UpdateIndexAsync()
    {
        try
        {
            var files = Directory.GetFiles(_entityDirectory, "*.json")
                .Where(f => !Path.GetFileName(f).Equals("index.json", StringComparison.OrdinalIgnoreCase))
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            var indexPath = GetIndexFilePath();
            var json = JsonSerializer.Serialize(files, _jsonOptions);
            await File.WriteAllTextAsync(indexPath, json);
            
            _logger.LogDebug("Updated index with {Count} entities", files.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update index in {Directory}", _entityDirectory);
            throw;
        }
    }

    protected string GetEntityFilePath(string id) => Path.Combine(_entityDirectory, $"{id}.json");
    protected string GetIndexFilePath() => Path.Combine(_entityDirectory, "index.json");

    protected async Task<bool> EntityExistsAsync(string id)
    {
        var filePath = GetEntityFilePath(id);
        return File.Exists(filePath);
    }

    protected async Task<IEnumerable<T>> FilterEntitiesAsync(Func<T, bool> predicate)
    {
        var allEntities = await ReadAllEntitiesAsync();
        return allEntities.Where(predicate);
    }
}