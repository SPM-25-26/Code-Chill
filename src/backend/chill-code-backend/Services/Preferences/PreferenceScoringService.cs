using System.Text.RegularExpressions;
using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Api;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Services.Preferences
{
    public interface IPreferenceScoringService
    {
        Task<PreferencePreviewDto> BuildPreviewAsync(
            TravelPreferenceDto preference,
            CancellationToken cancellationToken
        );
    }

    public class PreferenceScoringService : IPreferenceScoringService
    {
        private readonly ChillCodeDbContext _dbContext;
        private static readonly Regex WordSplitter = new(@"[^\p{L}\p{N}]+", RegexOptions.Compiled);

        private static readonly Dictionary<string, string[]> InterestKeywords = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            ["Cultural & Historical Sites"] = new[]
            {
                "museum",
                "culture",
                "history",
                "heritage",
                "castle",
            },
            ["Nature & Outdoor Activities"] = new[] { "nature", "park", "trail", "outdoor" },
            ["Food & Wine Experiences"] = new[] { "food", "wine", "restaurant", "cuisine" },
            ["Festivals & Events"] = new[] { "festival", "event", "celebration" },
            ["Art & Museums"] = new[] { "art", "gallery", "museum", "exhibition" },
            ["Architecture"] = new[] { "architecture", "building", "design" },
            ["Local Traditions"] = new[] { "tradition", "local", "folklore" },
            ["Sports & Adventure"] = new[] { "sport", "hike", "bike", "climb", "adventure" },
            ["Religious Sites"] = new[] { "church", "cathedral", "religious", "sanctuary" },
            ["Shopping & Markets"] = new[] { "shop", "market", "boutique" },
        };

        public PreferenceScoringService(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PreferencePreviewDto> BuildPreviewAsync(
            TravelPreferenceDto preference,
            CancellationToken cancellationToken
        )
        {
            var normalizedInterests = preference
                .Interests.Select(i => i.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var articles = await _dbContext.Articles.AsNoTracking().ToListAsync(cancellationToken);
            var events = await _dbContext.Events.AsNoTracking().ToListAsync(cancellationToken);
            var routes = await _dbContext.Routes.AsNoTracking().ToListAsync(cancellationToken);

            return new PreferencePreviewDto
            {
                Articles = ScoreArticles(preference, normalizedInterests, articles),
                Events = ScoreEvents(preference, normalizedInterests, events),
                Routes = ScoreRoutes(preference, normalizedInterests, routes),
            };
        }

        private static IReadOnlyCollection<ScoredArticleDto> ScoreArticles(
            TravelPreferenceDto preference,
            HashSet<string> normalizedInterests,
            IEnumerable<Models.Entities.Article> articles
        )
        {
            return articles
                .Select(article =>
                {
                    var score = 0.0;
                    score += preference.TravelVibe.Equals(
                        "chill",
                        StringComparison.OrdinalIgnoreCase
                    )
                        ? 0.2
                        : 0.05;

                    score += ScoreByInterests(
                        normalizedInterests,
                        article.Title,
                        string.Join(' ', article.Themes)
                    );
                    return new ScoredArticleDto
                    {
                        Id = article.Identifier,
                        Title = article.Title,
                        Subtitle = article.Subtitle,
                        Region = article.Region,
                        Municipality = article.MunicipalityData.Name,
                        ImagePath = article.ImagePath,
                        Score = Math.Round(score, 3),
                    };
                })
                .OrderByDescending(a => a.Score)
                .Take(10)
                .ToList();
        }

        private static IReadOnlyCollection<ScoredEventDto> ScoreEvents(
            TravelPreferenceDto preference,
            HashSet<string> normalizedInterests,
            IEnumerable<Models.Entities.Event> events
        )
        {
            return events
                .Select(ev =>
                {
                    var score = preference.TravelVibe.Equals(
                        "chill",
                        StringComparison.OrdinalIgnoreCase
                    )
                        ? 0.1
                        : 0.15;
                    score += ScoreByInterests(normalizedInterests, ev.Title, ev.Description);
                    return new ScoredEventDto
                    {
                        Id = ev.Identifier,
                        Title = ev.Title,
                        Address = ev.Address,
                        Municipality = ev.MunicipalityData.Name,
                        PrimaryImage = ev.PrimaryImage,
                        StartDate = ev.StartDate,
                        EndDate = ev.EndDate,
                        Score = Math.Round(score, 3),
                    };
                })
                .OrderByDescending(e => e.Score)
                .Take(10)
                .ToList();
        }

        private static IReadOnlyCollection<ScoredRouteDto> ScoreRoutes(
            TravelPreferenceDto preference,
            HashSet<string> normalizedInterests,
            IEnumerable<Models.Entities.Route> routes
        )
        {
            return routes
                .Select(route =>
                {
                    var score = preference.TravelVibe.Equals(
                        "adventure",
                        StringComparison.OrdinalIgnoreCase
                    )
                        ? 0.25
                        : 0.1;

                    score += ScoreActivityLevel(preference.ActivityLevel, route);
                    score += ScoreByInterests(
                        normalizedInterests,
                        route.Name,
                        route.Description,
                        route.PathTheme
                    );

                    return new ScoredRouteDto
                    {
                        Id = route.ExternalId ?? route.Name,
                        Name = route.Name,
                        Description = route.Description,
                        PathTheme = route.PathTheme,
                        TravellingMethod = route.TravellingMethod,
                        Municipality = route.MunicipalityData.Name,
                        RouteLength = route.RouteLength ?? string.Empty,
                        Duration = route.Duration ?? string.Empty,
                        ImagePath = route.ImagePath,
                        Score = Math.Round(score, 3),
                    };
                })
                .OrderByDescending(r => r.Score)
                .Take(10)
                .ToList();
        }

        private static double ScoreActivityLevel(string level, Models.Entities.Route route)
        {
            var lowerTheme = route.PathTheme?.ToLowerInvariant() ?? string.Empty;
            var lowerMethod = route.TravellingMethod?.ToLowerInvariant() ?? string.Empty;
            return level.ToLowerInvariant() switch
            {
                "low" => (lowerTheme.Contains("walk") || lowerTheme.Contains("easy")) ? 0.15 : 0.05,
                "medium" => (lowerTheme.Contains("hike") || lowerMethod.Contains("bike"))
                    ? 0.2
                    : 0.1,
                "high" => (lowerTheme.Contains("mountain") || lowerTheme.Contains("climb"))
                    ? 0.25
                    : 0.12,
                _ => 0,
            };
        }

        private static double ScoreByInterests(
            HashSet<string> normalizedInterests,
            params string?[] fields
        )
        {
            if (normalizedInterests.Count == 0)
            {
                return 0;
            }

            var text = string.Join(" ", fields.Where(f => !string.IsNullOrWhiteSpace(f)));
            var tokens = WordSplitter
                .Split(text.ToLowerInvariant())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToHashSet();

            var score = 0.0;
            foreach (var interest in normalizedInterests)
            {
                if (!InterestKeywords.TryGetValue(interest, out var keywords))
                {
                    continue;
                }

                if (keywords.Any(tokens.Contains))
                {
                    score += 0.3;
                }
            }

            return score;
        }
    }
}
