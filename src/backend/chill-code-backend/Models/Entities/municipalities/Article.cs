namespace ChillCodeBackend.Models.Entities
{
    public class Article
    {
        public string Identifier { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTime? SourceLastUpdated { get; set; }

        public string Language { get; set; } = "it";

        public string Title { get; set; } = string.Empty;
        public string Script { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;

        public string TimeToRead { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;
        public List<ArticleParagraph> Paragraphs { get; set; } = new();
        public List<string> Themes { get; set; } = new();
        public MunicipalityInfo MunicipalityData { get; set; } = new();
    }

    public class ArticleParagraph
    {
        public string Title { get; set; } = string.Empty;
        public string Script { get; set; } = string.Empty;
        public int Position { get; set; }
        public string Subtitle { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;

        public string ReferenceIdentifier { get; set; } = string.Empty;
        public string ReferenceName { get; set; } = string.Empty;
        public string ReferenceCategory { get; set; } = string.Empty;
        public string ReferenceImagePath { get; set; } = string.Empty;
        public double? ReferenceLongitude { get; set; }
        public double? ReferenceLatitude { get; set; }
    }
}
