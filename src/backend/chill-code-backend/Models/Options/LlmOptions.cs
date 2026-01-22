namespace ChillCodeBackend.Models.Options
{
    public class LlmOptions
    {
        public const string SectionName = "Llm";

        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4o-mini";
        public int TimeoutSeconds { get; set; } = 30;
    }
}
