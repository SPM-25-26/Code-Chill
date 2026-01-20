using System.Collections.Generic;

namespace ChillCodeBackend.Models.Api
{
    public class ChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatMessageDto> Conversation { get; set; } = new();
        public ChatLocationDto? Location { get; set; }
        public string? Municipality { get; set; }
        public string Language { get; set; } = "it";
        public int? MaxContextItems { get; set; }
    }

    public class ChatMessageDto
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
    }

    public class ChatLocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class ChatResponseDto
    {
        public string Reply { get; set; } = string.Empty;
        public int ContextItemsUsed { get; set; }
    }
}
