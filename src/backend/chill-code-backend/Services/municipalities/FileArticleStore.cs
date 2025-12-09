using System.Text.Json;
using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Services;

public class FileArticleStore : IArticleStore, IDisposable
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public FileArticleStore(IHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "articles.json");
    }

    public async Task<Article?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var articles = await ReadArticlesUnsafeAsync(cancellationToken);
            return articles.FirstOrDefault(a => string.Equals(a.Identifier, identifier, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task AddOrUpdateAsync(Article article, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var articles = await ReadArticlesUnsafeAsync(cancellationToken);
            var existingIndex = articles.FindIndex(a => string.Equals(a.Identifier, article.Identifier, StringComparison.OrdinalIgnoreCase));

            if (existingIndex >= 0)
            {
                articles[existingIndex] = article;
            }
            else
            {
                articles.Add(article);
            }

            await WriteArticlesUnsafeAsync(articles, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<IReadOnlyCollection<Article>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            return await ReadArticlesUnsafeAsync(cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<Article>> ReadArticlesUnsafeAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
            return new List<Article>();

        await using var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var articles = await JsonSerializer.DeserializeAsync<List<Article>>(stream, _serializerOptions, cancellationToken);
        return articles ?? new List<Article>();
    }

    private async Task WriteArticlesUnsafeAsync(IReadOnlyCollection<Article> articles, CancellationToken cancellationToken)
    {
        await using var stream = File.Open(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, articles, _serializerOptions, cancellationToken);
    }

    public void Dispose()
    {
        _fileLock.Dispose();
    }
}
