using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Models.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace chill_code_backend.Tests.Preferences
{
    public class PreferenceRepositoryTests
    {
        [Test]
        public async Task Upsert_Then_GetByUserId_RoundTripsData()
        {
            var options = new DbContextOptionsBuilder<ChillCodeDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var db = new ChillCodeDbContext(options);
            var repo = new PreferenceRepository(db);
            var userId = Guid.NewGuid();

            var upserted = await repo.UpsertAsync(
                userId,
                new UserPreference
                {
                    TravelVibe = "chill",
                    ActivityLevel = "medium",
                    CitySize = "small",
                    Interests = new List<string> { "Art & Museums" },
                },
                CancellationToken.None
            );

            var loaded = await repo.GetByUserIdAsync(userId, CancellationToken.None);

            upserted.Should().NotBeNull();
            loaded.Should().NotBeNull();
            loaded!.TravelVibe.Should().Be("chill");
            loaded.Interests.Should().ContainSingle(i => i == "Art & Museums");
        }
    }
}
