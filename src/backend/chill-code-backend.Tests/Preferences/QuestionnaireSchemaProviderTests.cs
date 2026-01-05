using ChillCodeBackend.Models.Api;
using ChillCodeBackend.Services.Preferences;
using FluentAssertions;
using NUnit.Framework;

namespace chill_code_backend.Tests.Preferences
{
    public class QuestionnaireSchemaProviderTests
    {
        [Test]
        public void MapAnswersToPreferences_ParsesJsonElementArrays()
        {
            var provider = new QuestionnaireSchemaProvider();
            var answers = new Dictionary<string, object>
            {
                { "travelVibe", "chill" },
                { "activityLevel", "low" },
                { "citySize", "small" },
                {
                    "interests",
                    System.Text.Json.JsonSerializer.Deserialize<object>(
                        "[\"Art & Museums\",\"Architecture\"]"
                    )!
                },
            };

            var dto = provider.MapAnswersToPreferences(answers);

            dto.TravelVibe.Should().Be("chill");
            dto.Interests.Should().Contain(new[] { "Art & Museums", "Architecture" });
        }

        [Test]
        public void GetSchema_ReturnsExpectedVersion()
        {
            var provider = new QuestionnaireSchemaProvider();
            var schema = provider.GetSchema();
            schema.Version.Should().Be("v1");
            schema.Questions.Should().NotBeEmpty();
        }
    }
}
