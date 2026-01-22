using System.Text.RegularExpressions;

namespace ChillCodeBackend.Services.Chat
{
    public class ChatResponseNormalizer
    {
        private static readonly Regex RolePrefix = new(
            @"^\s*(assistant|assistant:|response:)\s*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public string Normalize(string? rawResponse)
        {
            if (string.IsNullOrWhiteSpace(rawResponse))
            {
                return string.Empty;
            }

            var text = rawResponse.Replace("\r\n", "\n").Trim();

            if (text.StartsWith("```") && text.EndsWith("```") && text.Length > 6)
            {
                text = text.Trim('`').Trim();
            }

            text = RolePrefix.Replace(text, string.Empty).Trim();
            return text;
        }
    }
}
