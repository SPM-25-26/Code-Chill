using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Api;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Services.Preferences;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace chill_code_backend.Tests.Preferences
{
    public class PreferenceScoringServiceTests
    {
        [Test]
        public async Task BuildPreviewAsync_ReturnsRankedContent()
        {
            var options = new DbContextOptionsBuilder<ChillCodeDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var db = new ChillCodeDbContext(options);
            Seed(db);

            var service = new PreferenceScoringService(db);
            var preference = new TravelPreferenceDto
            {
                TravelVibe = "adventure",
                ActivityLevel = "high",
                CitySize = "small",
                Interests = new[] { "Sports & Adventure", "Nature & Outdoor Activities" },
            };

            var preview = await service.BuildPreviewAsync(preference, CancellationToken.None);

            preview.Routes.Should().NotBeEmpty();
            preview.Routes.First().Score.Should().BeGreaterThanOrEqualTo(preview.Routes.Last().Score);
        }

        [Test]
        public async Task BuildPreviewAsync_FamilyFriendlyPreference_BoostsFamilyEvents()
        {
            await using var db = CreateDbContext();
            db.Events.AddRange(
                new Event
                {
                    Identifier = "family",
                    Title = "Family Event",
                    Audience = "children",
                    MunicipalityData = new MunicipalityInfo { Name = "Town" },
                },
                new Event
                {
                    Identifier = "adult",
                    Title = "Adult Event",
                    Audience = "adults",
                    MunicipalityData = new MunicipalityInfo { Name = "Town" },
                }
            );
            await db.SaveChangesAsync();

            var service = new PreferenceScoringService(db);
            var preference = new TravelPreferenceDto
            {
                TravelVibe = "chill",
                ActivityLevel = "low",
                CitySize = "small",
                FamilyFriendly = true,
            };

            var preview = await service.BuildPreviewAsync(preference, CancellationToken.None);

            preview.Events.Should().HaveCount(2);
            preview.Events.First().Id.Should().Be("family");
            preview.Events.First().Score.Should().BeGreaterThan(preview.Events.Last().Score);
        }

        [Test]
        public async Task BuildPreviewAsync_NonFamilyPreference_PenalizesFamilyEvents()
        {
            await using var db = CreateDbContext();
            db.Events.AddRange(
                new Event
                {
                    Identifier = "family",
                    Title = "Family Event",
                    Audience = "children",
                    MunicipalityData = new MunicipalityInfo { Name = "Town" },
                },
                new Event
                {
                    Identifier = "adult",
                    Title = "Adult Event",
                    Audience = "adults",
                    MunicipalityData = new MunicipalityInfo { Name = "Town" },
                }
            );
            await db.SaveChangesAsync();

            var service = new PreferenceScoringService(db);
            var preference = new TravelPreferenceDto
            {
                TravelVibe = "adventure",
                ActivityLevel = "low",
                CitySize = "small",
                FamilyFriendly = false,
            };

            var preview = await service.BuildPreviewAsync(preference, CancellationToken.None);

            preview.Events.Should().HaveCount(2);
            preview.Events.First().Id.Should().Be("adult");
            preview.Events.First().Score.Should().BeGreaterThan(preview.Events.Last().Score);
        }

        [Test]
        public async Task BuildPreviewAsync_OvernightStay_BoostsSleepPois()
        {
            await using var db = CreateDbContext();
            db.PointsOfInterest.AddRange(
                new PointOfInterest
                {
                    Identifier = "sleep",
                    Category = MobileCategory.Sleep,
                    Title = "Hotel",
                    MunicipalityData = new MunicipalityInfo { Name = "Town" },
                },
                new PointOfInterest
                {
                    Identifier = "art",
                    Category = MobileCategory.ArtCulture,
                    Title = "Museum",
                    MunicipalityData = new MunicipalityInfo { Name = "Town" },
                }
            );
            await db.SaveChangesAsync();

            var service = new PreferenceScoringService(db);
            var preference = new TravelPreferenceDto
            {
                TravelVibe = "adventure",
                ActivityLevel = "low",
                CitySize = "small",
                OvernightStay = true,
            };

            var preview = await service.BuildPreviewAsync(preference, CancellationToken.None);

            preview.Pois.Should().HaveCount(2);
            preview.Pois.First().Id.Should().Be("sleep");
            preview.Pois.First().Score.Should().BeGreaterThan(preview.Pois.Last().Score);
        }

        [Test]
        public async Task BuildPreviewAsync_AdventureCapsArticleScore()
        {
            await using var db = CreateDbContext();
            db.Articles.Add(
                new Article
                {
                    Identifier = "a1",
                    Title = "Festival in Nature",
                    Themes = new List<string> { "nature" },
                    MunicipalityData = new MunicipalityInfo { Name = "Valley" },
                    Paragraphs =
                    [
                        new ArticleParagraph { ReferenceCategory = "Nature" },
                        new ArticleParagraph { ReferenceCategory = "Events" },
                    ],
                }
            );
            await db.SaveChangesAsync();

            var service = new PreferenceScoringService(db);
            var preference = new TravelPreferenceDto
            {
                TravelVibe = "adventure",
                ActivityLevel = "high",
                CitySize = "small",
                Interests = new[]
                {
                    "Nature & Outdoor Activities",
                    "Festivals & Events",
                },
            };

            var preview = await service.BuildPreviewAsync(preference, CancellationToken.None);

            preview.Articles.Should().ContainSingle();
            preview.Articles.First().Score.Should().Be(1.0);
        }

        [Test]
        public async Task BuildPreviewAsync_PrefersHigherActivityRoutes()
        {
            await using var db = CreateDbContext();
            db.Routes.AddRange(
                new Route
                {
                    ExternalId = "low",
                    Name = "Short Walk",
                    Duration = "30 min",
                    MunicipalityData = new MunicipalityInfo { Name = "Town" },
                },
                new Route
                {
                    ExternalId = "high",
                    Name = "Long Hike",
                    Duration = "5 ore",
                    MunicipalityData = new MunicipalityInfo { Name = "Town" },
                }
            );
            await db.SaveChangesAsync();

            var service = new PreferenceScoringService(db);
            var preference = new TravelPreferenceDto
            {
                TravelVibe = "adventure",
                ActivityLevel = "high",
                CitySize = "small",
            };

            var preview = await service.BuildPreviewAsync(preference, CancellationToken.None);

            preview.Routes.Should().HaveCount(2);
            preview.Routes.First().Id.Should().Be("high");
            preview.Routes.First().Score.Should().BeGreaterThan(preview.Routes.Last().Score);
        }

        [Test]
        public async Task BuildPreviewAsync_ReturnsEmptyCollections_WhenNoContent()
        {
            await using var db = CreateDbContext();
            var service = new PreferenceScoringService(db);
            var preference = new TravelPreferenceDto
            {
                TravelVibe = "chill",
                ActivityLevel = "low",
                CitySize = "small",
            };

            var preview = await service.BuildPreviewAsync(preference, CancellationToken.None);

            preview.Articles.Should().BeEmpty();
            preview.Events.Should().BeEmpty();
            preview.Routes.Should().BeEmpty();
            preview.Pois.Should().BeEmpty();
        }

        [Test]
        public async Task BuildPreviewAsync_HandlesMissingFieldsWithoutThrowing()
        {
            await using var db = CreateDbContext();
            db.Articles.Add(
                new Article
                {
                    Identifier = "a1",
                    MunicipalityData = new MunicipalityInfo(),
                }
            );
            db.Events.Add(
                new Event
                {
                    Identifier = "e1",
                    MunicipalityData = new MunicipalityInfo(),
                }
            );
            db.Routes.Add(
                new Route
                {
                    ExternalId = "r1",
                    MunicipalityData = new MunicipalityInfo(),
                }
            );
            db.PointsOfInterest.Add(
                new PointOfInterest
                {
                    Identifier = "p1",
                    Category = MobileCategory.Sleep,
                    MunicipalityData = new MunicipalityInfo(),
                }
            );
            await db.SaveChangesAsync();

            var service = new PreferenceScoringService(db);
            var preference = new TravelPreferenceDto
            {
                TravelVibe = "chill",
                ActivityLevel = "low",
                CitySize = "small",
            };

            var preview = await service.BuildPreviewAsync(preference, CancellationToken.None);

            preview.Articles.Should().ContainSingle();
            preview.Events.Should().ContainSingle();
            preview.Routes.Should().ContainSingle();
            preview.Pois.Should().ContainSingle();
        }

        [Test]
        public async Task BuildPreviewAsync_WithEmptyInterests_DoesNotAddKeywordScore()
        {
            await using var db = CreateDbContext();
            db.Articles.Add(
                new Article
                {
                    Identifier = "a1",
                    Title = "Festival in Nature",
                    Themes = new List<string> { "nature" },
                    MunicipalityData = new MunicipalityInfo { Name = "Valley" },
                    Paragraphs =
                    [
                        new ArticleParagraph { ReferenceCategory = "Nature" },
                        new ArticleParagraph { ReferenceCategory = "Events" },
                    ],
                }
            );
            await db.SaveChangesAsync();

            var service = new PreferenceScoringService(db);
            var preference = new TravelPreferenceDto
            {
                TravelVibe = "chill",
                ActivityLevel = "low",
                CitySize = "small",
                Interests = Array.Empty<string>(),
            };

            var preview = await service.BuildPreviewAsync(preference, CancellationToken.None);

            preview.Articles.Should().ContainSingle();
            preview.Articles.First().Score.Should().BeGreaterThanOrEqualTo(0.3);
        }

        private static ChillCodeDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ChillCodeDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ChillCodeDbContext(options);
        }

        private static void Seed(ChillCodeDbContext db)
        {
            db.Articles.Add(
                new Article
                {
                    Identifier = "a1",
                    Title = "Mountain Culture",
                    Subtitle = "Great views",
                    Region = "Alps",
                    ImagePath = "img",
                    Themes = new List<string> { "culture" },
                    MunicipalityData = new MunicipalityInfo { Name = "Valley" },
                }
            );

            db.Events.Add(
                new Event
                {
                    Identifier = "e1",
                    Title = "Local festival",
                    Address = "square",
                    MunicipalityData = new MunicipalityInfo { Name = "Town" },
                }
            );

            db.Routes.AddRange(
                new Route
                {
                    ExternalId = "r1",
                    Name = "Easy Walk",
                    Description = "walk in the park",
                    PathTheme = "easy walk",
                    TravellingMethod = "walk",
                    MunicipalityData = new MunicipalityInfo { Name = "Valley" },
                },
                new Route
                {
                    ExternalId = "r2",
                    Name = "Mountain Challenge",
                    Description = "climb and adventure",
                    PathTheme = "mountain adventure",
                    TravellingMethod = "hike",
                    MunicipalityData = new MunicipalityInfo { Name = "Peak" },
                }
            );

            db.SaveChanges();
        }
    }
}
