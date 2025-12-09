using System.Text.Json;
using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Services;

public class FileEventStore : IEventStore, IDisposable
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public FileEventStore(IHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "events.json");
    }

    public async Task<Event?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var events = await ReadEventsUnsafeAsync(cancellationToken);
            return events.FirstOrDefault(e => e.Identifier == identifier);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task AddOrUpdateAsync(Event evt, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var events = await ReadEventsUnsafeAsync(cancellationToken);
            var existingIndex = events.FindIndex(e => e.Identifier == evt.Identifier);

            if (existingIndex >= 0)
            {
                events[existingIndex] = evt;
            }
            else
            {
                events.Add(evt);
            }

            await WriteEventsUnsafeAsync(events, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task DeleteByIdAsync(string identifier, CancellationToken ct = default)
    {
        await _fileLock.WaitAsync(ct);
        try
        {
            var events = await ReadEventsUnsafeAsync(ct);
            var existingIndex = events.FindIndex(e => e.Identifier == identifier);
            if (existingIndex >= 0)
            {
                events.RemoveAt(existingIndex);
                await WriteEventsUnsafeAsync(events, ct);
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<IReadOnlyCollection<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            return await ReadEventsUnsafeAsync(cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<Event>> ReadEventsUnsafeAsync(CancellationToken ct)
    {
        if (!File.Exists(_filePath)) return new List<Event>();

        await using var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var events = await JsonSerializer.DeserializeAsync<List<Event>>(stream, _serializerOptions, ct);
        return events ?? new List<Event>();
    }

    private async Task WriteEventsUnsafeAsync(IReadOnlyCollection<Event> events, CancellationToken ct)
    {
        await using var stream = File.Open(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, events, _serializerOptions, ct);
    }

    public void Dispose() => _fileLock.Dispose();
}
