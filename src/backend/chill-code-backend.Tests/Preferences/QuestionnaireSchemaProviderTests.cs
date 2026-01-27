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

        [Test]
        public void MapAnswersToPreferences_ParsesCommaSeparatedInterests()
        {
            var provider = new QuestionnaireSchemaProvider();
            var answers = new Dictionary<string, object>
            {
                { "travelVibe", "adventure" },
                { "activityLevel", "high" },
                { "citySize", "big" },
                { "interests", "Art & Museums,Architecture" },
            };

            var dto = provider.MapAnswersToPreferences(answers);

            dto.Interests.Should().Contain(new[] { "Art & Museums", "Architecture" });
        }

        [Test]
        public void MapAnswersToPreferences_ParsesBooleansFromStrings()
        {
            var provider = new QuestionnaireSchemaProvider();
            var answers = new Dictionary<string, object>
            {
                { "travelVibe", "chill" },
                { "activityLevel", "low" },
                { "citySize", "small" },
                { "familyFriendly", "yes" },
                { "overnightStay", "no" },
            };

            var dto = provider.MapAnswersToPreferences(answers);

            dto.FamilyFriendly.Should().BeTrue();
            dto.OvernightStay.Should().BeFalse();
        }

        [Test]
        public void MapAnswersToPreferences_Throws_WhenPreferencesInvalid()
        {
            var provider = new QuestionnaireSchemaProvider();
            var answers = new Dictionary<string, object>
            {
                { "travelVibe", "invalid" },
                { "activityLevel", "low" },
                { "citySize", "small" },
            };

            var act = () => provider.MapAnswersToPreferences(answers);

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void MapAnswersToPreferences_ParsesEnumerableInterests()
        {
            var provider = new QuestionnaireSchemaProvider();
            var answers = new Dictionary<string, object>
            {
                { "travelVibe", "chill" },
                { "activityLevel", "low" },
                { "citySize", "small" },
                { "interests", new List<object> { "Art & Museums", "Local Traditions" } },
            };

            var dto = provider.MapAnswersToPreferences(answers);

            dto.Interests.Should().Contain(new[] { "Art & Museums", "Local Traditions" });
        }

        [Test]
        public void MapAnswersToPreferences_ParsesAnyAsNullFamilyFriendly()
        {
            var provider = new QuestionnaireSchemaProvider();
            var answers = new Dictionary<string, object>
            {
                { "travelVibe", "chill" },
                { "activityLevel", "low" },
                { "citySize", "small" },
                { "familyFriendly", "any" },
            };

            var dto = provider.MapAnswersToPreferences(answers);

            dto.FamilyFriendly.Should().BeNull();
        }
    }
}
