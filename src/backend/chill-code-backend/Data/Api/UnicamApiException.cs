namespace ChillCodeBackend.Data.Api
{
    public class UnicamApiException : Exception
    {
        public string? RelativePath { get; }

        public UnicamApiException(string message, string? relativePath = null)
            : base(message)
        {
            RelativePath = relativePath;
        }

        public UnicamApiException(string message, string? relativePath, Exception innerException)
            : base(message, innerException)
        {
            RelativePath = relativePath;
        }
    }
}
