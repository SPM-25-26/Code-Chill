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
            preview.Routes.First().Score.Should().BeGreaterThan(preview.Routes.Last().Score);
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
