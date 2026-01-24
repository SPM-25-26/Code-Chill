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
                "- When mentioning a specific item from TOURISM_DATA, format it as a markdown link: [Name](/login/content-detail/ID). Use the item's id."
            );
            sb.AppendLine("Domain control:");
            sb.AppendLine(
                "- You ONLY answer tourism-related questions (events, routes, POIs, services, organizations, articles)."
            );
            sb.AppendLine(
                "- If the user asks something out-of-domain, respond briefly that you can only help with local tourism and ask them to rephrase."
            );
            sb.AppendLine("Personalization and reasoning:");
            sb.AppendLine(
                "- Use USER_PROFILE preferences to rank and explain recommendations."
            );
            sb.AppendLine(
                "- Use LOCATION and distance_km to prioritize closer options when relevant."
            );
            sb.AppendLine(
                "- Use availability to avoid recommending unavailable events."
            );
            sb.AppendLine("Time-windowed queries:");
            sb.AppendLine(
                "- If the user asks for a time window (e.g., 'this week', specific dates), ONLY list items available in that window."
            );
            sb.AppendLine(
                "- If no items match the window, state that clearly and then offer a short 'Alternatives' section (non-event options) or ask for a different date range."
            );
            sb.AppendLine("Comparisons:");
            sb.AppendLine(
                "- If the user asks to compare places/events and data exists, compare 2-3 items side by side (short bullets)."
            );
            sb.AppendLine("Fallbacks:");
            sb.AppendLine(
                "- If TOURISM_DATA lacks relevant items, say you don't have enough data and suggest asking about nearby activities, events, or routes."
            );
            sb.AppendLine(
                "Always respond in the same language as the USER_MESSAGE."
            );
            sb.AppendLine(
                "If USER_MESSAGE language is unclear, use LANGUAGE_HINT."
            );
            sb.AppendLine(
                "If USER_MESSAGE language conflicts with LANGUAGE_HINT, prefer USER_MESSAGE."
            );
            sb.AppendLine(
                "If the user switches languages mid-conversation, follow the latest USER_MESSAGE language."
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
