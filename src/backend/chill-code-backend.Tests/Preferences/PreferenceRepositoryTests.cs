using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Data.Repositories;
using ChillCodeBackend.Models.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ChillCodeBackend.Tests.Preferences
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

        [Test]
        public async Task GetByUserId_ReturnsNull_WhenMissing()
        {
            var options = new DbContextOptionsBuilder<ChillCodeDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var db = new ChillCodeDbContext(options);
            var repo = new PreferenceRepository(db);

            var loaded = await repo.GetByUserIdAsync(Guid.NewGuid(), CancellationToken.None);

            loaded.Should().BeNull();
        }

        [Test]
        public async Task Upsert_UpdatesExistingPreference()
        {
            var options = new DbContextOptionsBuilder<ChillCodeDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var db = new ChillCodeDbContext(options);
            var repo = new PreferenceRepository(db);
            var userId = Guid.NewGuid();

            await repo.UpsertAsync(
                userId,
                new UserPreference
                {
                    TravelVibe = "chill",
                    ActivityLevel = "low",
                    CitySize = "small",
                    Interests = new List<string> { "Art & Museums" },
                },
                CancellationToken.None
            );

            var existing = await db.UserPreferences.FirstAsync(p => p.UserId == userId);
            existing.CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10);
            existing.UpdatedAt = existing.CreatedAt;
            await db.SaveChangesAsync();

            var updated = await repo.UpsertAsync(
                userId,
                new UserPreference
                {
                    TravelVibe = "adventure",
                    ActivityLevel = "high",
                    CitySize = "big",
                    Interests = new List<string> { "Sports & Adventure" },
                    FamilyFriendly = true,
                },
                CancellationToken.None
            );

            updated.UserId.Should().Be(userId);
            updated.TravelVibe.Should().Be("adventure");
            updated.ActivityLevel.Should().Be("high");
            updated.CitySize.Should().Be("big");
            updated.Interests.Should().ContainSingle(i => i == "Sports & Adventure");
            updated.FamilyFriendly.Should().BeTrue();
            updated.CreatedAt.Should().Be(existing.CreatedAt);
            updated.UpdatedAt.Should().BeOnOrAfter(existing.UpdatedAt);
        }
    }
}
