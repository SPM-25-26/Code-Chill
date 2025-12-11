using System.Text.Json;

namespace ChillCodeBackend.Data.Api
{
    public class UnicamApiProxy : IUnicamApiProxy
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

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
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"Unicam API returned {(int)response.StatusCode} ({response.StatusCode}) for '{relativePath}'. Body: {errorBody}"
                );
            }

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        public async Task<T?> FetchAsAsync<T>(
            string relativePath,
            CancellationToken cancellationToken = default
        )
        {
            using var response = await _httpClient.GetAsync(relativePath, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"Unicam API returned {(int)response.StatusCode} ({response.StatusCode}) for '{relativePath}'. Body: {errorBody}"
                );
            }

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(
                content,
                _serializerOptions,
                cancellationToken
            );
        }
    }
}
