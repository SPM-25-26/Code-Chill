namespace ChillCodeBackend.Services.Chat
{
    public interface IChatCompletionClient
    {
        Task<string> GetCompletionAsync(string prompt, CancellationToken cancellationToken);
    }
}
