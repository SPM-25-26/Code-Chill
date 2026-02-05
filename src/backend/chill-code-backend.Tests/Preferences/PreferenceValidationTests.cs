using ChillCodeBackend.Models.Api;
using FluentAssertions;
using NUnit.Framework;

namespace ChillCodeBackend.Tests.Preferences
{
    public class PreferenceValidationTests
    {
        [Test]
        public void Validate_ReturnsError_WhenInterestLimitExceeded()
        {
            var dto = new TravelPreferenceDto
            {
                TravelVibe = "chill",
                ActivityLevel = "low",
                CitySize = "small",
                Interests = Enumerable.Repeat("Art & Museums", 9).ToList(),
            };

            var (isValid, error) = PreferenceValidation.Validate(dto);
            isValid.Should().BeFalse();
            error.Should().Contain("more than 8");
        }

        [Test]
        public void Validate_Succeeds_WithAllowedValues()
        {
            var dto = new TravelPreferenceDto
            {
                TravelVibe = "adventure",
                ActivityLevel = "high",
                CitySize = "big",
                Interests = new[] { "Sports & Adventure", "Nature & Outdoor Activities" },
            };

            var (isValid, error) = PreferenceValidation.Validate(dto);
            isValid.Should().BeTrue();
            error.Should().BeNull();
        }

        [Test]
        public void Validate_ReturnsError_WhenTravelVibeInvalid()
        {
            var dto = new TravelPreferenceDto
            {
                TravelVibe = "relaxed",
                ActivityLevel = "low",
                CitySize = "small",
            };

            var (isValid, error) = PreferenceValidation.Validate(dto);
            isValid.Should().BeFalse();
            error.Should().Contain("TravelVibe");
        }

        [Test]
        public void Validate_ReturnsError_WhenActivityLevelInvalid()
        {
            var dto = new TravelPreferenceDto
            {
                TravelVibe = "chill",
                ActivityLevel = "super-high",
                CitySize = "small",
            };

            var (isValid, error) = PreferenceValidation.Validate(dto);
            isValid.Should().BeFalse();
            error.Should().Contain("ActivityLevel");
        }

        [Test]
        public void Validate_ReturnsError_WhenCitySizeInvalid()
        {
            var dto = new TravelPreferenceDto
            {
                TravelVibe = "chill",
                ActivityLevel = "low",
                CitySize = "medium",
            };

            var (isValid, error) = PreferenceValidation.Validate(dto);
            isValid.Should().BeFalse();
            error.Should().Contain("CitySize");
        }

        [Test]
        public void Validate_ReturnsError_WhenInterestUnsupported()
        {
            var dto = new TravelPreferenceDto
            {
                TravelVibe = "chill",
                ActivityLevel = "low",
                CitySize = "small",
                Interests = new[] { "Unknown Interest" },
            };

            var (isValid, error) = PreferenceValidation.Validate(dto);
            isValid.Should().BeFalse();
            error.Should().Contain("Interests");
        }
    }
}
