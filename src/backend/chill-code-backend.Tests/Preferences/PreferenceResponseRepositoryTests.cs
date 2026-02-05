using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Models.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ChillCodeBackend.Tests.Preferences
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

        [Test]
        public async Task GetLatestAsync_ReturnsMostRecentlyUpdated()
        {
            var options = new DbContextOptionsBuilder<ChillCodeDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var db = new ChillCodeDbContext(options);
            var repo = new PreferenceResponseRepository(db);
            var userId = Guid.NewGuid();

            db.PreferenceResponses.AddRange(
                new PreferenceResponse
                {
                    UserId = userId,
                    QuestionnaireVersion = "v1",
                    Answers = new Dictionary<string, object> { { "travelVibe", "chill" } },
                    DerivedPreferences = new ChillCodeBackend.Models.Api.TravelPreferenceDto
                    {
                        TravelVibe = "chill",
                        ActivityLevel = "low",
                        CitySize = "small",
                    },
                    UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
                },
                new PreferenceResponse
                {
                    UserId = userId,
                    QuestionnaireVersion = "v1",
                    Answers = new Dictionary<string, object> { { "travelVibe", "adventure" } },
                    DerivedPreferences = new ChillCodeBackend.Models.Api.TravelPreferenceDto
                    {
                        TravelVibe = "adventure",
                        ActivityLevel = "high",
                        CitySize = "big",
                    },
                    UpdatedAt = DateTimeOffset.UtcNow,
                }
            );
            await db.SaveChangesAsync();

            var latest = await repo.GetLatestAsync(userId, CancellationToken.None);

            latest.Should().NotBeNull();
            latest!.DerivedPreferences.TravelVibe.Should().Be("adventure");
        }

        [Test]
        public async Task Upsert_UpdatesExistingResponse()
        {
            var options = new DbContextOptionsBuilder<ChillCodeDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var db = new ChillCodeDbContext(options);
            var repo = new PreferenceResponseRepository(db);
            var userId = Guid.NewGuid();

            await repo.UpsertAsync(
                userId,
                new PreferenceResponse
                {
                    QuestionnaireVersion = "v1",
                    Answers = new Dictionary<string, object> { { "travelVibe", "chill" } },
                    DerivedPreferences = new ChillCodeBackend.Models.Api.TravelPreferenceDto
                    {
                        TravelVibe = "chill",
                        ActivityLevel = "low",
                        CitySize = "small",
                    },
                },
                CancellationToken.None
            );

            var existing = await db.PreferenceResponses.FirstAsync(r => r.UserId == userId);
            existing.UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-10);
            await db.SaveChangesAsync();

            var updated = await repo.UpsertAsync(
                userId,
                new PreferenceResponse
                {
                    QuestionnaireVersion = "v2",
                    Answers = new Dictionary<string, object> { { "travelVibe", "adventure" } },
                    DerivedPreferences = new ChillCodeBackend.Models.Api.TravelPreferenceDto
                    {
                        TravelVibe = "adventure",
                        ActivityLevel = "high",
                        CitySize = "big",
                    },
                },
                CancellationToken.None
            );

            updated.QuestionnaireVersion.Should().Be("v2");
            updated.DerivedPreferences.TravelVibe.Should().Be("adventure");
            updated.Answers["travelVibe"].ToString().Should().Be("adventure");
            updated.UpdatedAt.Should().BeOnOrAfter(existing.UpdatedAt);
        }
    }
}
