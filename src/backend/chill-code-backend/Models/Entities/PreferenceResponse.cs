using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using ChillCodeBackend.Models.Api;

namespace ChillCodeBackend.Models.Entities
{
    public class PreferenceResponse
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string QuestionnaireVersion { get; set; } = "v1";
        public string AnswersJson { get; set; } = string.Empty;
        public string DerivedPreferencesJson { get; set; } = string.Empty;
        public bool Skipped { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        [NotMapped]
        public Dictionary<string, object> Answers
        {
            get =>
                string.IsNullOrWhiteSpace(AnswersJson)
                    ? new()
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(
                        AnswersJson,
                        JsonSerializerOptionsProvider.Options
                    ) ?? new();
            set =>
                AnswersJson = JsonSerializer.Serialize(
                    value,
                    JsonSerializerOptionsProvider.Options
                );
        }

        [NotMapped]
        public TravelPreferenceDto DerivedPreferences
        {
            get =>
                string.IsNullOrWhiteSpace(DerivedPreferencesJson)
                    ? new TravelPreferenceDto()
                    : JsonSerializer.Deserialize<TravelPreferenceDto>(
                        DerivedPreferencesJson,
                        JsonSerializerOptionsProvider.Options
                    ) ?? new TravelPreferenceDto();
            set =>
                DerivedPreferencesJson = JsonSerializer.Serialize(
                    value,
                    JsonSerializerOptionsProvider.Options
                );
        }
    }

    internal static class JsonSerializerOptionsProvider
    {
        public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    }
}
