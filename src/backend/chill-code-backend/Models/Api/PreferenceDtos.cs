using System.ComponentModel.DataAnnotations;

namespace ChillCodeBackend.Models.Api
{
    public record TravelPreferenceDto
    {
        [Required]
        public string TravelVibe { get; init; } = string.Empty; // chill | adventure

        [Required]
        public string ActivityLevel { get; init; } = string.Empty; // low | medium | high

        [Required]
        public string CitySize { get; init; } = string.Empty; // small | big

        public IReadOnlyCollection<string> Interests { get; init; } = Array.Empty<string>();

        public bool? FamilyFriendly { get; init; }

        public bool? OvernightStay { get; init; }
    }

    public static class PreferenceValidation
    {
        private static readonly HashSet<string> AllowedVibes = new(StringComparer.OrdinalIgnoreCase)
        {
            "chill",
            "adventure",
        };

        private static readonly HashSet<string> AllowedActivity = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            "low",
            "medium",
            "high",
        };

        private static readonly HashSet<string> AllowedCitySize = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            "small",
            "big",
        };

        private static readonly HashSet<string> AllowedInterests = new(
            new[]
            {
                "Cultural & Historical Sites",
                "Nature & Outdoor Activities",
                "Food & Wine Experiences",
                "Festivals & Events",
                "Art & Museums",
                "Architecture",
                "Local Traditions",
                "Sports & Adventure",
                "Religious Sites",
                "Shopping & Markets",
            },
            StringComparer.OrdinalIgnoreCase
        );

        public static (bool IsValid, string? Error) Validate(TravelPreferenceDto dto)
        {
            if (!AllowedVibes.Contains(dto.TravelVibe))
            {
                return (false, "TravelVibe must be one of: chill, adventure.");
            }

            if (!AllowedActivity.Contains(dto.ActivityLevel))
            {
                return (false, "ActivityLevel must be one of: low, medium, high.");
            }

            if (!AllowedCitySize.Contains(dto.CitySize))
            {
                return (false, "CitySize must be one of: small, big.");
            }

            if (dto.Interests.Count > 8)
            {
                return (false, "Interests cannot contain more than 8 items.");
            }

            if (dto.Interests.Any(i => !AllowedInterests.Contains(i)))
            {
                return (false, "Interests contain unsupported values.");
            }

            return (true, null);
        }
    }
}
