using System.Linq;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Services.Ingestion.Mappers;
using FluentAssertions;
using NUnit.Framework;

namespace ChillCodeBackend.Tests.Services.Ingestion.Mappers;

[TestFixture]
public class PointOfInterestMapperTests
{
    [Test]
    public void MapPointsOfInterest_MapsOwnerAndMunicipalityFallback()
    {
        var json = """
            [
              {
                "entityId": "poi-1",
                "title": "Poi Title",
                "officialName": "Poi Official",
                "latitude": 42.1,
                "lon": 13.4,
                "gallery": [" /g.png ", ""],
                "virtualTours": ["tour-1"],
                "owner": { "taxCode": "TX-1", "legalName": "Owner Name" },
                "municipality": { "name": "City", "logoPath": "/logo.png" },
                "openingHours": [
                  { "dayOfWeek": "Mon", "opens": "08:00", "closes": "18:00", "description": "Open" }
                ],
                "prices": [
                  { "description": "Adult", "minPrice": 5.5, "maxValue": 10.0, "currency": "EUR" }
                ],
                "services": ["wifi"]
              }
            ]
            """;

        var mapper = new PointOfInterestMapper();
        var pois = mapper
            .MapPointsOfInterest(new[] { TestJson.Parse(json).EnumerateArray().First() }, MobileCategory.Services)
            .ToList();

        pois.Should().ContainSingle();
        var poi = pois[0];
        poi.Identifier.Should().Be("poi-1");
        poi.Category.Should().Be(MobileCategory.Services);
        poi.Title.Should().Be("Poi Title");
        poi.OfficialName.Should().Be("Poi Official");
        poi.Longitude.Should().Be(13.4);
        poi.Gallery.Should().ContainSingle().Which.Should().Be("/g.png");
        poi.VirtualTours.Should().ContainSingle().Which.Should().Be("tour-1");
        poi.OrganizationTaxCode.Should().Be("TX-1");
        poi.OrganizationLegalName.Should().Be("Owner Name");
        poi.MunicipalityData.Name.Should().Be("City");
        poi.MunicipalityData.LogoPath.Should().Be("/logo.png");

        poi.OpeningHours.Should().ContainSingle();
        poi.OpeningHours[0].DayOfWeek.Should().Be("Mon");
        poi.OpeningHours[0].Opens.Should().Be("08:00");

        poi.Prices.Should().ContainSingle();
        poi.Prices[0].MinPrice.Should().Be(5.5m);
        poi.Prices[0].MaxPrice.Should().Be(10.0m);
        poi.Prices[0].Currency.Should().Be("EUR");
    }

    [Test]
    public void MapPointsOfInterest_SkipsWhenIdentifierMissing()
    {
        var json = """
            [
              { "title": "No id" }
            ]
            """;

        var mapper = new PointOfInterestMapper();
        var pois = mapper
            .MapPointsOfInterest(new[] { TestJson.Parse(json).EnumerateArray().First() }, MobileCategory.Services)
            .ToList();

        pois.Should().BeEmpty();
    }
}
