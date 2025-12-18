namespace ChillCodeBackend.Models.Entities
{
    public class IngestionRun
    {
        public int Id { get; set; }

        public string Municipality { get; set; } = string.Empty;
        public string Language { get; set; } = "it";

        public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? CompletedAt { get; set; }

        public int TotalPoiCount { get; set; }
        public int EventCount { get; set; }
        public int OrganizationCount { get; set; }
        public int RouteCount { get; set; }
        public int ArticleCount { get; set; }

        public List<string> Errors { get; set; } = new();
    }
}

