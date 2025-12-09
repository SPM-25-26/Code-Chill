namespace ChillCodeBackend.Data.Api
{
    public interface IUnicamApiProxy
    {
        Task<string> FetchRawAsync(
            string relativePath,
            CancellationToken cancellationToken = default
        );

        Task<T?> FetchAsAsync<T>(
            string relativePath,
            CancellationToken cancellationToken = default
        );
    }
}
