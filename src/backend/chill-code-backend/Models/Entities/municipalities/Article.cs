using System.ComponentModel.DataAnnotations;

namespace ChillCodeBackend.Models.Entities
{
    public class Article
    {
        public string Identifier { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Script { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;

        public string TimeToRead { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;
        public List<ArticleParagraph>? Paragraphs { get; set; }

        public string UpdateAt { get; set; } = string.Empty;

        public List<string> Themes { get; set; } = [];

        public MunicipalityInfo? MunicipalityData { get; set; }
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
