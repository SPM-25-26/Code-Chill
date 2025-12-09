using System.Text.Json;
using ChillCodeBackend.Models.Entities;
using Microsoft.Extensions.Hosting;

namespace ChillCodeBackend.Services;

public class FileRouteStore : IRouteStore, IDisposable
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public FileRouteStore(IHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "routes.json");
    }

    public async Task<Models.Entities.Route?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var routes = await ReadRoutesUnsafeAsync(cancellationToken);
            return routes.FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task AddOrUpdateAsync(Models.Entities.Route route, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var routes = await ReadRoutesUnsafeAsync(cancellationToken);
            var existingIndex = routes.FindIndex(r => string.Equals(r.Name, route.Name, StringComparison.OrdinalIgnoreCase));

            if (existingIndex >= 0)
            {
                routes[existingIndex] = route; // Aggiorna la route esistente
            }
            else
            {
                routes.Add(route); // Aggiungi nuova route
            }

            await WriteRoutesUnsafeAsync(routes, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<IReadOnlyCollection<Models.Entities.Route>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var routes = await ReadRoutesUnsafeAsync(cancellationToken);
            return routes;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<Models.Entities.Route>> ReadRoutesUnsafeAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return new List<Models.Entities.Route>();
        }

        await using var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var routes = await JsonSerializer.DeserializeAsync<List<Models.Entities.Route>>(stream, _serializerOptions, cancellationToken);
        return routes ?? new List<Models.Entities.Route>();
    }

    private async Task WriteRoutesUnsafeAsync(IReadOnlyCollection<Models.Entities.Route> routes, CancellationToken cancellationToken)
    {
        await using var stream = File.Open(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, routes, _serializerOptions, cancellationToken);
    }

    public void Dispose()
    {
        _fileLock.Dispose();
    }
}
