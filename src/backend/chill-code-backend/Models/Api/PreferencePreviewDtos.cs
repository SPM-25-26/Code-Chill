namespace ChillCodeBackend.Models.Api
{
    public record PreferencePreviewDto
    {
        public IReadOnlyCollection<ScoredArticleDto> Articles { get; init; } =
            Array.Empty<ScoredArticleDto>();
        public IReadOnlyCollection<ScoredEventDto> Events { get; init; } =
            Array.Empty<ScoredEventDto>();
        public IReadOnlyCollection<ScoredRouteDto> Routes { get; init; } =
            Array.Empty<ScoredRouteDto>();
    }

    public record ScoredArticleDto
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Subtitle { get; init; } = string.Empty;
        public string Municipality { get; init; } = string.Empty;
        public string Region { get; init; } = string.Empty;
        public string ImagePath { get; init; } = string.Empty;
        public double Score { get; init; }
    }

    public record ScoredEventDto
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string Municipality { get; init; } = string.Empty;
        public string? PrimaryImage { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public double Score { get; init; }
    }

    public record ScoredRouteDto
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string PathTheme { get; init; } = string.Empty;
        public string TravellingMethod { get; init; } = string.Empty;
        public string Municipality { get; init; } = string.Empty;
        public string RouteLength { get; init; } = string.Empty;
        public string Duration { get; init; } = string.Empty;
        public string? ImagePath { get; init; }
        public double Score { get; init; }
    }
}
