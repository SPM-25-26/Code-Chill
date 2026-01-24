using System.Text;
using ChillCodeBackend.Models.Api;

namespace ChillCodeBackend.Services.Chat
{
    public class ChatPromptBuilder
    {
        public string BuildPrompt(ChatPromptInput input)
        {
            var sb = new StringBuilder();

            sb.AppendLine("SYSTEM:");
            sb.AppendLine("You are ChillCode, a travel assistant.");
            sb.AppendLine(
                "Use ONLY the tourism data provided below. If data is missing, say you don't know."
            );
            sb.AppendLine("Keep answers concise and practical.");
            sb.AppendLine("Format responses for readability:");
            sb.AppendLine("- Use short paragraphs.");
            sb.AppendLine("- When listing options, use a bulleted list with one item per line.");
            sb.AppendLine("- Each bullet should include the name, availability (dates/times), and a short reason.");
            sb.AppendLine(
                "Always respond in the same language as the USER_MESSAGE. If unclear, use LANGUAGE_HINT."
            );
            sb.AppendLine();

            sb.AppendLine("USER_PROFILE:");
            sb.AppendLine(string.IsNullOrWhiteSpace(input.PreferenceSummary) ? "-" : input.PreferenceSummary);
            sb.AppendLine();

            sb.AppendLine("LOCATION:");
            sb.AppendLine(string.IsNullOrWhiteSpace(input.LocationSummary) ? "-" : input.LocationSummary);
            sb.AppendLine();

            sb.AppendLine("CONVERSATION:");
            AppendConversation(sb, input.Conversation);
            sb.AppendLine();

            sb.AppendLine("USER_MESSAGE:");
            sb.AppendLine(input.UserMessage.Trim());
            sb.AppendLine();

            sb.AppendLine("LANGUAGE_HINT:");
            sb.AppendLine(string.IsNullOrWhiteSpace(input.LanguageHint) ? "-" : input.LanguageHint.Trim());
            sb.AppendLine();

            sb.AppendLine("TOURISM_DATA:");
            AppendItems(sb, input.Items);

            return sb.ToString().Trim();
        }

        private static void AppendConversation(
            StringBuilder sb,
            IReadOnlyCollection<ChatMessageDto> conversation
        )
        {
            if (conversation == null || conversation.Count == 0)
            {
                sb.AppendLine("-");
                return;
            }

            foreach (var message in conversation.Take(10))
            {
                var role = string.IsNullOrWhiteSpace(message.Role)
                    ? "user"
                    : message.Role.Trim().ToLowerInvariant();
                var content = string.IsNullOrWhiteSpace(message.Content)
                    ? "-"
                    : message.Content.Trim();
                sb.AppendLine($"{role}: {content}");
            }
        }

        private static void AppendItems(
            StringBuilder sb,
            IReadOnlyCollection<TourismContextItem> items
        )
        {
            if (items == null || items.Count == 0)
            {
                sb.AppendLine("-");
                return;
            }

            foreach (var item in items)
            {
                sb.AppendLine(
                    $"- [{item.Type}] {item.Name} | id={item.Id} | category={Safe(item.Category)} | municipality={Safe(item.Municipality)} | address={Safe(item.Address)} | distance_km={FormatDistance(item.DistanceKm)} | availability={Safe(item.Availability)} | notes={Safe(item.Notes)}"
                );
            }
        }

        private static string Safe(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
        }

        private static string FormatDistance(double? distanceKm)
        {
            return distanceKm.HasValue ? distanceKm.Value.ToString("0.0") : "-";
        }
    }
}
