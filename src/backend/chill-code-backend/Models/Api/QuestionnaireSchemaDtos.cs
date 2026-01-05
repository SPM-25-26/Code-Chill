namespace ChillCodeBackend.Models.Api
{
    public record QuestionnaireSchemaDto
    {
        public string Version { get; init; } = "v1";
        public IReadOnlyCollection<QuestionDto> Questions { get; init; } =
            Array.Empty<QuestionDto>();
    }

    public record QuestionDto
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty; // binary | single | multi
        public bool Required { get; init; }
        public int? MinSelections { get; init; }
        public int? MaxSelections { get; init; }
        public IReadOnlyCollection<OptionDto> Options { get; init; } = Array.Empty<OptionDto>();
    }

    public record OptionDto
    {
        public string Value { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public string? HelperText { get; init; }
    }
}
