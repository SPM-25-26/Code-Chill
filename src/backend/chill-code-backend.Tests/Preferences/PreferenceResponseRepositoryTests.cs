using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Models.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace chill_code_backend.Tests.Preferences
{
    public class PreferenceResponseRepositoryTests
    {
        [Test]
        public async Task Upsert_StoresAnswersAndDerivedPrefs()
        {
            var options = new DbContextOptionsBuilder<ChillCodeDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var db = new ChillCodeDbContext(options);
            var repo = new PreferenceResponseRepository(db);
            var userId = Guid.NewGuid();

            var response = new PreferenceResponse
            {
                QuestionnaireVersion = "v1",
                Answers = new Dictionary<string, object> { { "travelVibe", "chill" } },
                DerivedPreferences = new ChillCodeBackend.Models.Api.TravelPreferenceDto
                {
                    TravelVibe = "chill",
                    ActivityLevel = "low",
                    CitySize = "small",
                },
            };

            await repo.UpsertAsync(userId, response, CancellationToken.None);

            var latest = await repo.GetLatestAsync(userId, CancellationToken.None);
            latest.Should().NotBeNull();
            latest!.Answers["travelVibe"].ToString().Should().Be("chill");
            latest.DerivedPreferences.TravelVibe.Should().Be("chill");
        }
    }
}
