namespace ChillCodeBackend.Models.Api
{
    public class IngestionRequestDto
    {
        public string Municipality { get; set; } = string.Empty;
        public string? Language { get; set; } = "it";
    }
}
