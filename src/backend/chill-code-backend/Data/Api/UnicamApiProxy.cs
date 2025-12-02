using System.Net.Http.Json;

namespace ChillCodeBackend.Data.Api;

public class UnicamApiProxy : IUnicamApiProxy
{
    private readonly HttpClient _httpClient;

    public UnicamApiProxy(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> FetchRawAsync(
        string relativePath,
        CancellationToken cancellationToken = default
    )
    {
        using var response = await _httpClient.GetAsync(relativePath, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<T?> FetchAsAsync<T>(
        string relativePath,
        CancellationToken cancellationToken = default
    )
    {
        return await _httpClient.GetFromJsonAsync<T>(relativePath, cancellationToken);
    }
}
