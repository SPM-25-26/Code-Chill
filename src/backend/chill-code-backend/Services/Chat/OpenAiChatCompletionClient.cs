using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChillCodeBackend.Models.Options;
using Microsoft.Extensions.Options;

namespace ChillCodeBackend.Services.Chat
{
    public class OpenAiChatCompletionClient : IChatCompletionClient
    {
        private readonly HttpClient _httpClient;
        private readonly LlmOptions _options;

        public OpenAiChatCompletionClient(
            HttpClient httpClient,
            IOptions<LlmOptions> options
        )
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string> GetCompletionAsync(
            string prompt,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrWhiteSpace(_options.BaseUrl) || string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException("LLM configuration is missing.");
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(
                    new ChatCompletionRequest
                    {
                        Model = _options.Model,
                        Messages = new List<ChatMessage>
                        {
                            new ChatMessage { Role = "user", Content = prompt },
                        },
                        Temperature = 0.4,
                        MaxTokens = 400,
                    },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                ),
                Encoding.UTF8,
                "application/json"
            );

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"LLM request failed: {(int)response.StatusCode} {responseBody}"
                );
            }

            var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return completion?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
        }

        private class ChatCompletionRequest
        {
            public string Model { get; set; } = string.Empty;
            public List<ChatMessage> Messages { get; set; } = new();
            public double Temperature { get; set; }
            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; }
        }

        private class ChatMessage
        {
            public string Role { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }

        private class ChatCompletionResponse
        {
            public List<Choice> Choices { get; set; } = new();
        }

        private class Choice
        {
            public ChatMessage Message { get; set; } = new();
        }
    }
}
