using System.Collections.Generic;
using ChillCodeBackend.Models.Api;

namespace ChillCodeBackend.Services.Chat
{
    public class ChatPromptInput
    {
        public string UserMessage { get; set; } = string.Empty;
        public IReadOnlyCollection<ChatMessageDto> Conversation { get; set; } =
            new List<ChatMessageDto>();
        public string PreferenceSummary { get; set; } = string.Empty;
        public string LocationSummary { get; set; } = string.Empty;
        public string LanguageHint { get; set; } = string.Empty;
        public IReadOnlyCollection<TourismContextItem> Items { get; set; } =
            new List<TourismContextItem>();
    }
}
