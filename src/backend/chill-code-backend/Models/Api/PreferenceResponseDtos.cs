namespace ChillCodeBackend.Models.Api
{
    public record PreferenceResponseDto
    {
        public string QuestionnaireVersion { get; init; } = "v1";
        public Dictionary<string, object> Answers { get; init; } = new();
        public TravelPreferenceDto DerivedPreferences { get; init; } = new();
        public PreferencePreviewDto? Preview { get; init; }
    }
}
