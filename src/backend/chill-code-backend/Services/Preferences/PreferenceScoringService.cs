using System.Text.RegularExpressions;
using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Api;
using ChillCodeBackend.Models.Entities;
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

        private static readonly Dictionary<string, string[]> TypologyInterestMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Sagra"] = new[] { "Food & Wine Experiences" },
                ["Fiera / Salone"] = new[] { "Food & Wine Experiences", "Shopping & Markets" },
                ["Festival"] = new[] { "Festivals & Events" },
                ["Show"] = new[] { "Festivals & Events" },
                ["Patronal Day/Feast of Saints"] = new[]
                {
                    "Festivals & Events",
                    "Local Traditions",
                },
                ["Musical event"] = new[] { "Festivals & Events" },
                ["Theatrical performance"] = new[] { "Festivals & Events" },
                ["Sports meeting"] = new[] { "Sports & Adventure", "Festivals & Events" },
            };

        private static readonly Dictionary<string, string[]> PathThemeInterestMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Archeologico"] = new[] { "Architecture" },
                ["Culturale"] = new[] { "Local Traditions" },
                ["Storico"] = new[] { "Local Traditions" },
                ["Naturalistico"] = new[] { "Nature & Outdoor Activities" },
                ["Religiosi"] = new[] { "Religious Sites" },
            };

        private static readonly Dictionary<string, string[]> ReferenceCategoryInterestMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Nature"] = new[] { "Nature & Outdoor Activities" },
                ["Events"] = new[] { "Festivals & Events" },
                ["ArtCulture"] = new[] { "Art & Museums" },
                ["EntertainmentLeisure"] = new[]
                {
                    "Festivals & Events",
                    "Local Traditions",
                },
            };

        private static readonly Dictionary<MobileCategory, string[]> PoiCategoryInterestMap = new()
        {
            [MobileCategory.ArtCulture] = new[]
            {
                "Cultural & Historical Sites",
                "Art & Museums",
                "Architecture",
            },
            [MobileCategory.Nature] = new[] { "Nature & Outdoor Activities" },
            [MobileCategory.EatAndDrink] = new[] { "Food & Wine Experiences" },
            [MobileCategory.Shopping] = new[] { "Shopping & Markets" },
            [MobileCategory.EntertainmentLeisure] = new[] { "Festivals & Events" },
            [MobileCategory.TypicalProducts] = new[]
            {
                "Food & Wine Experiences",
                "Shopping & Markets",
                "Local Traditions",
            },
            [MobileCategory.Sleep] = Array.Empty<string>(),
        };

        private static readonly Dictionary<string, string[]> PoiTypologyInterestMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Museo"] = new[] { "Art & Museums", "Cultural & Historical Sites" },
                ["Monumento"] = new[] { "Cultural & Historical Sites", "Architecture" },
                ["Torre"] = new[] { "Cultural & Historical Sites", "Architecture" },
                ["Chiesa"] = new[] { "Religious Sites", "Architecture" },
                ["Santuario"] = new[] { "Religious Sites" },
                ["Belvedere"] = new[] { "Nature & Outdoor Activities" },
                ["Montagna"] = new[] { "Nature & Outdoor Activities", "Sports & Adventure" },
                ["Caffetteria"] = new[] { "Food & Wine Experiences" },
                ["Ristorante Tradizionale"] = new[] { "Food & Wine Experiences" },
                ["Cantina Produttiva"] = new[]
                {
                    "Food & Wine Experiences",
                    "Shopping & Markets",
                },
                ["Alloggio in ambito di attività agrituristica"] = new[]
                {
                    "Food & Wine Experiences",
                    "Nature & Outdoor Activities",
                    "Local Traditions",
                },
            };

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
            ["Food & Wine Experiences"] = new[] { "food", "wine", "restaurant", "cuisine", "sagra" },
            ["Festivals & Events"] = new[] { "festival", "event", "celebration", "show" },
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
            var pois = await _dbContext.PointsOfInterest
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new PreferencePreviewDto
            {
                Articles = ScoreArticles(preference, normalizedInterests, articles),
                Events = ScoreEvents(preference, normalizedInterests, events),
                Routes = ScoreRoutes(preference, normalizedInterests, routes),
                Pois = ScorePois(preference, normalizedInterests, pois),
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

                    var paragraphInterests = article.Paragraphs
                        .Select(p => p.ReferenceCategory)
                        .Where(rc => !string.IsNullOrWhiteSpace(rc))
                        .SelectMany(rc => MapReferenceCategoryToInterests(rc))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    foreach (var interest in paragraphInterests)
                    {
                        if (normalizedInterests.Contains(interest))
                        {
                            score += 1.0;
                        }
                    }

                    if (preference.TravelVibe.Equals("chill", StringComparison.OrdinalIgnoreCase))
                    {
                        score += 0.3; // small lean so articles still rank with vibe
                    }

                    // fallback keyword-style match on themes/title
                    score += ScoreByKeywords(
                        normalizedInterests,
                        article.Title,
                        string.Join(' ', article.Themes)
                    );

                    // Adventure/active users should see articles as light suggestions only.
                    if (
                        preference.TravelVibe.Equals("adventure", StringComparison.OrdinalIgnoreCase)
                        || preference.ActivityLevel.Equals("high", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        score = Math.Min(score, 1.0);
                    }

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
                    var score = 0.0;

                    // family-friendly bias but never filter out
                    if (preference.FamilyFriendly == true)
                    {
                        score += IsFamilyFriendly(ev.Audience) ? 0.3 : 0.0;
                    }
                    else if (preference.FamilyFriendly == false && IsFamilyFriendly(ev.Audience))
                    {
                        // User asked for non-family focus: gently push family events down.
                        score -= 0.2;
                    }

                    // vibe leaning
                    if (preference.TravelVibe.Equals("chill", StringComparison.OrdinalIgnoreCase))
                    {
                        if (IsChillTypology(ev.Typology))
                        {
                            score += 1.0;
                        }
                    }
                    else if (
                        preference.TravelVibe.Equals("adventure", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        if (IsAdventureTypology(ev.Typology))
                        {
                            score += 1.0;
                        }
                    }

                    // interests from typology
                    foreach (var interest in MapTypologyToInterests(ev.Typology))
                    {
                        if (normalizedInterests.Contains(interest))
                        {
                            score += 1.0;
                        }
                    }

                    // keyword fallback
                    score += ScoreByKeywords(normalizedInterests, ev.Title, ev.Description);
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
                    var score = 0.0;

                    var routeDifficulty = DeriveRouteActivityLevel(route);
                    score += ScoreActivityMatch(preference.ActivityLevel, routeDifficulty);

                    // vibe: chill prefers low effort, adventure prefers higher or biking
                    if (preference.TravelVibe.Equals("adventure", StringComparison.OrdinalIgnoreCase))
                    {
                        if (
                            routeDifficulty.Equals("high", StringComparison.OrdinalIgnoreCase)
                            || route.TravellingMethod?.Contains("cicl", StringComparison.OrdinalIgnoreCase)
                                == true
                        )
                        {
                            score += 1.0;
                        }
                    }
                    else if (
                        preference.TravelVibe.Equals("chill", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        if (routeDifficulty.Equals("low", StringComparison.OrdinalIgnoreCase))
                        {
                            score += 1.0;
                        }
                    }

                    foreach (var interest in MapPathThemeToInterests(route.PathTheme))
                    {
                        if (normalizedInterests.Contains(interest))
                        {
                            score += 1.0;
                        }
                    }

                    if (
                        route.TravellingMethod?.Contains("cicl", StringComparison.OrdinalIgnoreCase)
                            == true
                        && normalizedInterests.Contains("Sports & Adventure")
                    )
                    {
                        score += 1.0;
                    }

                    if (normalizedInterests.Contains("Nature & Outdoor Activities"))
                    {
                        score += 0.3; // small universal outdoor lean
                    }

                    score += ScoreByKeywords(
                        normalizedInterests,
                        route.Name,
                        route.Description,
                        route.PathTheme
                    );

                    return new ScoredRouteDto
                    {
                        Id = route.ExternalId ?? route.Name ?? string.Empty,
                        Name = route.Name ?? string.Empty,
                        Description = route.Description ?? string.Empty,
                        PathTheme = route.PathTheme ?? string.Empty,
                        TravellingMethod = route.TravellingMethod ?? string.Empty,
                        Municipality = route.MunicipalityData.Name ?? string.Empty,
                        RouteLength = route.RouteLength ?? string.Empty,
                        Duration = route.Duration ?? string.Empty,
                        ImagePath = route.ImagePath,
                        Score = Math.Round(score, 3),
                    };
                })
                .OrderByDescending(r => r.Score)
                .ToList();
        }

        private static IReadOnlyCollection<ScoredPoiDto> ScorePois(
            TravelPreferenceDto preference,
            HashSet<string> normalizedInterests,
            IEnumerable<PointOfInterest> pois
        )
        {
            return pois
                .Select(poi =>
                {
                    var score = 0.0;

                    if (preference.TravelVibe.Equals("chill", StringComparison.OrdinalIgnoreCase))
                    {
                        if (IsChillPoi(poi))
                        {
                            score += 1.0;
                        }
                    }
                    else if (
                        preference.TravelVibe.Equals("adventure", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        if (IsAdventurePoi(poi))
                        {
                            score += 1.0;
                        }
                    }

                    // Boost Sleep POIs significantly when user wants overnight stay
                    if (preference.OvernightStay == true && poi.Category == MobileCategory.Sleep)
                    {
                        score += 2.0;
                    }

                    var mappedInterests = MapPoiCategoryToInterests(poi.Category)
                        .Concat(MapPoiTypologyToInterests(poi.Typology))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    foreach (var interest in mappedInterests)
                    {
                        if (normalizedInterests.Contains(interest))
                        {
                            score += 1.0;
                        }
                    }

                    score += ScoreByKeywords(
                        normalizedInterests,
                        poi.Title,
                        poi.Description,
                        poi.Typology
                    );

                    return new ScoredPoiDto
                    {
                        Id = poi.Identifier,
                        Title = !string.IsNullOrWhiteSpace(poi.Title)
                            ? poi.Title
                            : poi.OfficialName,
                        Category = poi.Category.ToString(),
                        Typology = poi.Typology ?? string.Empty,
                        Municipality = poi.MunicipalityData?.Name ?? string.Empty,
                        PrimaryImage = string.IsNullOrWhiteSpace(poi.PrimaryImage)
                            ? null
                            : poi.PrimaryImage,
                        Score = Math.Round(score, 3),
                    };
                })
                .OrderByDescending(p => p.Score)
                .ToList();
        }

        private static double ScoreByKeywords(
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

        private static IEnumerable<string> MapTypologyToInterests(string? typology)
        {
            if (string.IsNullOrWhiteSpace(typology))
            {
                return Array.Empty<string>();
            }

            return TypologyInterestMap.TryGetValue(typology, out var interests)
                ? interests
                : Array.Empty<string>();
        }

        private static IEnumerable<string> MapPathThemeToInterests(string? pathTheme)
        {
            if (string.IsNullOrWhiteSpace(pathTheme))
            {
                return Array.Empty<string>();
            }

            return PathThemeInterestMap.TryGetValue(pathTheme, out var interests)
                ? interests
                : Array.Empty<string>();
        }

        private static IEnumerable<string> MapPoiCategoryToInterests(MobileCategory category)
        {
            return PoiCategoryInterestMap.TryGetValue(category, out var interests)
                ? interests
                : Array.Empty<string>();
        }

        private static IEnumerable<string> MapPoiTypologyToInterests(string? typology)
        {
            if (string.IsNullOrWhiteSpace(typology))
            {
                return Array.Empty<string>();
            }

            return PoiTypologyInterestMap.TryGetValue(typology, out var interests)
                ? interests
                : Array.Empty<string>();
        }

        private static IEnumerable<string> MapReferenceCategoryToInterests(string category)
        {
            return ReferenceCategoryInterestMap.TryGetValue(category, out var interests)
                ? interests
                : Array.Empty<string>();
        }

        private static bool IsFamilyFriendly(string? audience)
        {
            if (string.IsNullOrWhiteSpace(audience))
            {
                return false;
            }

            var lower = audience.ToLowerInvariant();
            return lower.Contains("bambini") || lower.Contains("children");
        }

        private static bool IsChillTypology(string? typology)
        {
            if (string.IsNullOrWhiteSpace(typology))
            {
                return false;
            }

            return typology.Equals("Musical event", StringComparison.OrdinalIgnoreCase)
                || typology.Equals("Theatrical performance", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAdventureTypology(string? typology)
        {
            if (string.IsNullOrWhiteSpace(typology))
            {
                return false;
            }

            return !IsChillTypology(typology);
        }

        private static bool IsChillPoi(PointOfInterest poi)
        {
            return poi.Category == MobileCategory.ArtCulture
                || poi.Category == MobileCategory.EatAndDrink
                || poi.Category == MobileCategory.Sleep
                || poi.Category == MobileCategory.TypicalProducts;
        }

        private static bool IsAdventurePoi(PointOfInterest poi)
        {
            if (
                poi.Category == MobileCategory.Nature
                || poi.Category == MobileCategory.Shopping
                || poi.Category == MobileCategory.EntertainmentLeisure
            )
            {
                return true;
            }

            return MapPoiTypologyToInterests(poi.Typology)
                .Any(i => i.Equals("Sports & Adventure", StringComparison.OrdinalIgnoreCase));
        }

        private static string DeriveRouteActivityLevel(Models.Entities.Route route)
        {
            var minutes = ParseDurationMinutes(route.Duration);
            if (minutes is not null)
            {
                if (minutes <= 60)
                {
                    return "low";
                }

                if (minutes <= 240)
                {
                    return "medium";
                }

                return "high";
            }

            var lengthKm = ParseLength(route.RouteLength);
            if (lengthKm is not null)
            {
                if (lengthKm <= 5)
                {
                    return "low";
                }

                if (lengthKm <= 12)
                {
                    return "medium";
                }

                return "high";
            }

            // fallback: assume medium if unknown
            return "medium";
        }

        private static int? ParseDurationMinutes(string? duration)
        {
            if (string.IsNullOrWhiteSpace(duration))
            {
                return null;
            }

            var lower = duration.ToLowerInvariant().Trim();
            if (lower.Contains("metà giornata"))
            {
                return 240;
            }

            if (lower.Contains("una giornata"))
            {
                return 480;
            }

            // pattern: "X ora", "X ore", "X ora e Y minuti"
            var hours = 0;
            var minutes = 0;
            var hourMatch = Regex.Match(lower, @"(\d+)\s*or");
            if (hourMatch.Success && int.TryParse(hourMatch.Groups[1].Value, out var h))
            {
                hours = h;
            }

            var minuteMatch = Regex.Match(lower, @"(\d+)\s*min");
            if (minuteMatch.Success && int.TryParse(minuteMatch.Groups[1].Value, out var m))
            {
                minutes = m;
            }

            if (hours == 0 && minutes == 0)
            {
                return null;
            }

            return (hours * 60) + minutes;
        }

        private static double? ParseLength(string? length)
        {
            if (string.IsNullOrWhiteSpace(length))
            {
                return null;
            }

            return double.TryParse(length, out var value) ? value : null;
        }

        private static double ScoreActivityMatch(string preferredLevel, string routeLevel)
        {
            var pref = preferredLevel.ToLowerInvariant();
            var route = routeLevel.ToLowerInvariant();

            if (pref == route)
            {
                return 1.0;
            }

            // adjacent levels still get some credit
            if (
                (pref == "low" && route == "medium")
                || (pref == "medium" && (route == "low" || route == "high"))
                || (pref == "high" && route == "medium")
            )
            {
                return 0.5;
            }

            return 0.0;
        }
    }
}
