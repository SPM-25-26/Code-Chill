using System.Text.Json;
using ChillCodeBackend.Models.Entities;
using Microsoft.Extensions.Hosting;

namespace ChillCodeBackend.Services;

public class FileUserStore : IUserStore, IDisposable
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public FileUserStore(IHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "users.json");
    }

    public async Task<User?> GetByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default
    )
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var users = await ReadUsersUnsafeAsync(cancellationToken);
            return users.FirstOrDefault(u =>
                string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase)
            );
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var users = await ReadUsersUnsafeAsync(cancellationToken);
            return users.FirstOrDefault(u =>
                string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)
            );
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task AddOrUpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var users = await ReadUsersUnsafeAsync(cancellationToken);
            var existingIndex = users.FindIndex(u =>
                u.Id == user.Id
                || string.Equals(u.Email, user.Email, StringComparison.OrdinalIgnoreCase)
            );

            if (existingIndex >= 0)
            {
                user.ChangedAt = DateTimeOffset.UtcNow;
                if (user.CreatedAt == default)
                {
                    user.CreatedAt = DateTimeOffset.UtcNow;
                }
                users[existingIndex] = user;
            }
            else
            {
                user.Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id;
                var now = DateTimeOffset.UtcNow;
                user.CreatedAt = user.CreatedAt == default ? now : user.CreatedAt;
                user.ChangedAt = now;
                users.Add(user);
            }

            await WriteUsersUnsafeAsync(users, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<IReadOnlyCollection<User>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var users = await ReadUsersUnsafeAsync(cancellationToken);
            return users;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<User>> ReadUsersUnsafeAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return new List<User>();
        }

        await using var stream = File.Open(
            _filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );
        var users = await JsonSerializer.DeserializeAsync<List<User>>(
            stream,
            _serializerOptions,
            cancellationToken
        );
        return users ?? new List<User>();
    }

    private async Task WriteUsersUnsafeAsync(
        IReadOnlyCollection<User> users,
        CancellationToken cancellationToken
    )
    {
        await using var stream = File.Open(
            _filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );
        await JsonSerializer.SerializeAsync(stream, users, _serializerOptions, cancellationToken);
    }

    public void Dispose()
    {
        _fileLock.Dispose();
    }
}
