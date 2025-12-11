using System.Text.Json;
using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Services.Ingestion.Mappers
{
    /// <summary>
    /// Maps raw Unicam article JSON payloads into Article entities.
    /// </summary>
    public class ArticleMapper : JsonMapperBase
    {
        public IEnumerable<Article> MapArticles(IEnumerable<JsonElement> items)
        {
            foreach (var item in items)
            {
                if (!TryGetString(item, out var identifier, "id", "identifier"))
                {
                    continue;
                }

                yield return new Article
                {
                    Identifier = identifier,
                    Title = GetString(item, "title"),
                    Subtitle = GetString(item, "subtitle"),
                    Script = GetString(item, "script"),
                    Region = GetString(item, "region"),
                    TimeToRead = GetString(item, "timeToRead", "readTime", "readingTime"),
                    ImagePath = GetString(item, "imagePath", "coverImage"),
                    Themes = GetStringList(item, "themes"),
                    MunicipalityData = MapMunicipalityInfo(item),
                    Paragraphs = MapParagraphs(item),
                };
            }
        }

        private MunicipalityInfo? MapMunicipalityInfo(JsonElement root)
        {
            if (
                !root.TryGetProperty("municipality", out var muni)
                || muni.ValueKind == JsonValueKind.Null
            )
            {
                return null;
            }

            return new MunicipalityInfo
            {
                Name = GetString(muni, "name"),
                LogoPath = GetString(muni, "logoPath"),
            };
        }

        private List<ArticleParagraph>? MapParagraphs(JsonElement root)
        {
            if (
                !root.TryGetProperty("paragraphs", out var paragraphsElement)
                || paragraphsElement.ValueKind != JsonValueKind.Array
            )
            {
                return null;
            }

            var paragraphs = new List<ArticleParagraph>();
            var index = 0;
            foreach (var paragraphElement in paragraphsElement.EnumerateArray())
            {
                paragraphs.Add(
                    new ArticleParagraph
                    {
                        Title = GetString(paragraphElement, "title"),
                        Script = GetString(paragraphElement, "script"),
                        Position = GetInt(paragraphElement, "position") ?? index,
                        Subtitle = GetString(paragraphElement, "subtitle"),
                        Region = GetString(paragraphElement, "region"),
                        ReferenceIdentifier = GetString(
                            paragraphElement,
                            "referenceIdentifier",
                            "referenceId",
                            "reference"
                        ),
                        ReferenceName = GetString(paragraphElement, "referenceName"),
                        ReferenceCategory = GetString(
                            paragraphElement,
                            "referenceCategory",
                            "category"
                        ),
                        ReferenceImagePath = GetString(
                            paragraphElement,
                            "referenceImagePath",
                            "imagePath"
                        ),
                        ReferenceLongitude = GetDouble(
                            paragraphElement,
                            "referenceLongitude",
                            "longitude"
                        ),
                        ReferenceLatitude = GetDouble(
                            paragraphElement,
                            "referenceLatitude",
                            "latitude"
                        ),
                    }
                );

                index++;
            }

            return paragraphs;
        }
    }
}
