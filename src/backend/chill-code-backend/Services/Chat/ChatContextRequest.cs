namespace ChillCodeBackend.Services.Chat
{
    public class ChatContextRequest
    {
        public string Language { get; set; } = "it";
        public string? Municipality { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int MaxItems { get; set; } = 40;
    }
}
