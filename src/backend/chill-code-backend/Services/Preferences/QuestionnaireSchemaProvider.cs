using ChillCodeBackend.Models.Api;

namespace ChillCodeBackend.Services.Preferences
{
    public interface IQuestionnaireSchemaProvider
    {
        QuestionnaireSchemaDto GetSchema();
        TravelPreferenceDto MapAnswersToPreferences(Dictionary<string, object> answers);
    }

    public class QuestionnaireSchemaProvider : IQuestionnaireSchemaProvider
    {
        private readonly QuestionnaireSchemaDto _schema = new()
        {
            Version = "v1",
            Questions =
            [
                new QuestionDto
                {
                    Id = "travelVibe",
                    Title = "What's your travel style?",
                    Type = "binary",
                    Required = true,
                    Options =
                    [
                        new OptionDto { Value = "chill", Label = "Chill & Relax" },
                        new OptionDto { Value = "adventure", Label = "Adventure & Active" },
                    ],
                },
                new QuestionDto
                {
                    Id = "activityLevel",
                    Title = "How active do you want to be?",
                    Type = "single",
                    Required = true,
                    Options =
                    [
                        new OptionDto { Value = "low", Label = "Low" },
                        new OptionDto { Value = "medium", Label = "Medium" },
                        new OptionDto { Value = "high", Label = "High" },
                    ],
                },
                new QuestionDto
                {
                    Id = "citySize",
                    Title = "Which atmosphere do you prefer?",
                    Type = "binary",
                    Required = true,
                    Options =
                    [
                        new OptionDto { Value = "small", Label = "Small & Cozy" },
                        new OptionDto { Value = "big", Label = "Big & Bustling" },
                    ],
                },
                new QuestionDto
                {
                    Id = "interests",
                    Title = "What interests you?",
                    Type = "multi",
                    Required = false,
                    MinSelections = 0,
                    MaxSelections = 8,
                    Options =
                    [
                        new OptionDto
                        {
                            Value = "Cultural & Historical Sites",
                            Label = "Cultural & Historical Sites",
                        },
                        new OptionDto
                        {
                            Value = "Nature & Outdoor Activities",
                            Label = "Nature & Outdoor Activities",
                        },
                        new OptionDto
                        {
                            Value = "Food & Wine Experiences",
                            Label = "Food & Wine Experiences",
                        },
                        new OptionDto
                        {
                            Value = "Festivals & Events",
                            Label = "Festivals & Events",
                        },
                        new OptionDto { Value = "Art & Museums", Label = "Art & Museums" },
                        new OptionDto { Value = "Architecture", Label = "Architecture" },
                        new OptionDto { Value = "Local Traditions", Label = "Local Traditions" },
                        new OptionDto
                        {
                            Value = "Sports & Adventure",
                            Label = "Sports & Adventure",
                        },
                        new OptionDto { Value = "Religious Sites", Label = "Religious Sites" },
                        new OptionDto
                        {
                            Value = "Shopping & Markets",
                            Label = "Shopping & Markets",
                        },
                    ],
                },
            ],
        };

        public QuestionnaireSchemaDto GetSchema() => _schema;

        public TravelPreferenceDto MapAnswersToPreferences(Dictionary<string, object> answers)
        {
            var travelVibe = answers.TryGetValue("travelVibe", out var vibeObj)
                ? ToStringSafe(vibeObj)
                : string.Empty;
            var activityLevel = answers.TryGetValue("activityLevel", out var activityObj)
                ? ToStringSafe(activityObj)
                : string.Empty;
            var citySize = answers.TryGetValue("citySize", out var cityObj)
                ? ToStringSafe(cityObj)
                : string.Empty;

            var interests = new List<string>();
            if (
                answers.TryGetValue("interests", out var interestsObj)
                && interestsObj is IEnumerable<object> enumerable
            )
            {
                interests = enumerable
                    .Select(i => ToStringSafe(i))
                    .Where(s => s != string.Empty)
                    .ToList();
            }
            else if (
                answers.TryGetValue("interests", out interestsObj)
                && interestsObj is System.Text.Json.JsonElement element
                && element.ValueKind == System.Text.Json.JsonValueKind.Array
            )
            {
                interests = element
                    .EnumerateArray()
                    .Select(e => e.ToString())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
            }
            else if (
                answers.TryGetValue("interests", out interestsObj)
                && interestsObj is string singleInterest
            )
            {
                interests = singleInterest
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            var dto = new TravelPreferenceDto
            {
                TravelVibe = travelVibe,
                ActivityLevel = activityLevel,
                CitySize = citySize,
                Interests = interests,
            };

            var (isValid, error) = PreferenceValidation.Validate(dto);
            if (!isValid)
            {
                throw new ArgumentException(error ?? "Invalid preferences derived from answers.");
            }

            return dto;
        }

        private static string ToStringSafe(object value)
        {
            return value switch
            {
                System.Text.Json.JsonElement element => element.ToString(),
                _ => value?.ToString() ?? string.Empty,
            };
        }
    }
}
