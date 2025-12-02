namespace ChillCodeBackend.Data.Api;

public interface IUnicamApiProxy
{
    Task<string> FetchRawAsync(string relativePath, CancellationToken cancellationToken = default);
}
