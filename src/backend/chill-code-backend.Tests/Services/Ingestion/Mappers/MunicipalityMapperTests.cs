using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Models.Unicam;
using ChillCodeBackend.Services.Ingestion.Mappers;
using FluentAssertions;
using NUnit.Framework;

namespace ChillCodeBackend.Tests.Services.Ingestion.Mappers;

[TestFixture]
public class MunicipalityMapperTests
{
    [Test]
    public void Map_SetsDefaultsWhenValuesMissing()
    {
        var visit = new UnicamMunicipalityVisit();

        var mapper = new MunicipalityMapper();
        var municipality = mapper.Map("test-municipality", visit, "it");

        municipality.ExternalId.Should().Be("test-municipality");
        municipality.Name.Should().BeEmpty();
        municipality.Description.Should().BeEmpty();
        municipality.Contacts.Email.Should().BeEmpty();
        municipality.HomeImages.Should().NotBeNull();
        municipality.Events.Should().NotBeNull();
        municipality.ArticlesAndPaths.Should().NotBeNull();
    }

    [Test]
    public void Map_MapsRelatedEntitiesAndCategories()
    {
        var visit = new UnicamMunicipalityVisit
        {
            Name = "Castignano",
            Events =
            [
                new UnicamMunicipalityVisitHighlight
                {
                    EntityId = "evt-1",
                    Category = "Events",
                    Title = "Title",
                },
                new UnicamMunicipalityVisitHighlight { EntityId = "ignored", Category = null },
            ],
            ArticlesAndPaths =
            [
                new UnicamMunicipalityVisitHighlight { EntityId = "route-1", Category = "Routes" },
                new UnicamMunicipalityVisitHighlight { EntityId = "art-1", Category = "Unknown" },
            ],
        };

        var mapper = new MunicipalityMapper();
        var municipality = mapper.Map("castignano", visit, "it");

        municipality.ExternalId.Should().Be("castignano");
        municipality.Events.Should().HaveCount(2);
        municipality.Events![0].Category.Should().Be(MobileCategory.Events);

        municipality.ArticlesAndPaths.Should().HaveCount(2);
        municipality.ArticlesAndPaths![0].Category.Should().Be(MobileCategory.Routes);
        municipality.ArticlesAndPaths![1].Category.Should().Be(MobileCategory.Articles);
    }
}
