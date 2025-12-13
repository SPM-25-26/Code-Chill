using System.Linq;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Services.Ingestion.Mappers;
using FluentAssertions;
using NUnit.Framework;

namespace ChillCodeBackend.Tests.Services.Ingestion.Mappers;

[TestFixture]
public class ArticleMapperTests
{
    [Test]
    public void MapArticles_MapsExpectedFields_AndHandlesMissingValues()
    {
        var json = """
            [
              {
                "id": "art-1",
                "title": "Sample Title",
                "subtitle": "Sub",
                "script": "Body text",
                "region": "Marche",
                "timeToRead": "5 min",
                "imagePath": "/img.jpg",
                "themes": ["culture", "history"],
                "municipality": {
                  "name": "Castignano",
                  "logoPath": "/logo.png"
                },
                "paragraphs": [
                  { "title": "P1", "script": "Text", "position": 3 },
                  { "title": "P2" }
                ]
              }
            ]
            """;

        var mapper = new ArticleMapper();
        var articles = mapper
            .MapArticles(new[] { TestJson.Parse(json).EnumerateArray().First() })
            .ToList();

        articles.Should().HaveCount(1);
        var article = articles[0];
        article.Identifier.Should().Be("art-1");
        article.Title.Should().Be("Sample Title");
        article.Subtitle.Should().Be("Sub");
        article.Script.Should().Be("Body text");
        article.Region.Should().Be("Marche");
        article.TimeToRead.Should().Be("5 min");
        article.ImagePath.Should().Be("/img.jpg");
        article.Themes.Should().Contain(new[] { "culture", "history" });
        article.MunicipalityData.Name.Should().Be("Castignano");
        article.MunicipalityData.LogoPath.Should().Be("/logo.png");

        article.Paragraphs.Should().HaveCount(2);
        article.Paragraphs[0].Position.Should().Be(3);
        article.Paragraphs[1].Position.Should().Be(1); // falls back to index when missing
        article.Paragraphs[1].Script.Should().BeEmpty();
    }

    [Test]
    public void MapArticles_SkipsArticlesWithoutIdentifier()
    {
        var json = """
            [
              { "title": "Missing id" }
            ]
            """;

        var mapper = new ArticleMapper();
        var articles = mapper
            .MapArticles(new[] { TestJson.Parse(json).EnumerateArray().First() })
            .ToList();

        articles.Should().BeEmpty();
    }
}
