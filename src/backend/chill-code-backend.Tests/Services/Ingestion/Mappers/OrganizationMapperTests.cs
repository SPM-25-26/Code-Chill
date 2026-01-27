using System;
using System.Linq;
using ChillCodeBackend.Services.Ingestion.Mappers;
using FluentAssertions;
using NUnit.Framework;

namespace ChillCodeBackend.Tests.Services.Ingestion.Mappers;

[TestFixture]
public class OrganizationMapperTests
{
    [Test]
    public void MapOrganizations_MapsNestedCollectionsAndFields()
    {
        var json = """
            [
              {
                "taxCode": "ORG-1",
                "updatedAt": "2025-01-02T03:04:05Z",
                "legalName": "Org Name",
                "primaryImagePath": "/primary.png",
                "gallery": [" /g1.png ", {"ignore": true}, ""],
                "latitude": 43.1,
                "longitude": 13.2,
                "neighbors": [
                  { "entityId": "n1", "title": "Neighbor", "category": "Sleep", "imagePath": "/n1.png", "extraInfo": "info" }
                ],
                "nearestCarPark": { "lat": 1.2, "lng": 3.4, "address": "Park", "distance": 0.5 },
                "ownedPoi": [
                  { "identifier": "poi-1", "officialName": "Poi One", "imagePath": "/poi.png", "category": "Services" }
                ],
                "offers": [
                  {
                    "description": "Offer",
                    "price": 9.5,
                    "currency": "EUR",
                    "validityStartDate": "2025-02-01T00:00:00Z",
                    "validityEndDate": "2025-02-02T00:00:00Z"
                  }
                ],
                "events": [
                  {
                    "entityId": "evt-1",
                    "entityName": "Event One",
                    "imagePath": "/ev.png",
                    "municipalityData": { "name": "City", "logoPath": "/logo.png" }
                  }
                ],
                "municipalityData": { "name": "Castignano", "logoPath": "/m.png" }
              }
            ]
            """;

        var mapper = new OrganizationMapper();
        var organizations = mapper
            .MapOrganizations(new[] { TestJson.Parse(json).EnumerateArray().First() })
            .ToList();

        organizations.Should().ContainSingle();
        var org = organizations[0];
        org.TaxCode.Should().Be("ORG-1");
        org.SourceLastUpdated.Should().Be(DateTime.Parse("2025-01-02T03:04:05Z"));
        org.LegalName.Should().Be("Org Name");
        org.PrimaryImagePath.Should().Be("/primary.png");
        org.Gallery.Should().ContainSingle().Which.Should().Be("/g1.png");
        org.Latitude.Should().Be(43.1);
        org.Longitude.Should().Be(13.2);

        org.Neighbors.Should().ContainSingle();
        org.Neighbors[0].EntityId.Should().Be("n1");
        org.Neighbors[0].Title.Should().Be("Neighbor");

        org.NearestCarPark.Should().NotBeNull();
        org.NearestCarPark!.Latitude.Should().Be(1.2);
        org.NearestCarPark.Longitude.Should().Be(3.4);

        org.OwnedPoi.Should().ContainSingle();
        org.OwnedPoi[0].Identifier.Should().Be("poi-1");
        org.OwnedPoi[0].OfficialName.Should().Be("Poi One");

        org.Offers.Should().ContainSingle();
        org.Offers[0].PriceSpecificationCurrencyValue.Should().Be(9.5m);
        org.Offers[0].ValidityStartDate.Should().Be(DateTime.Parse("2025-02-01T00:00:00Z"));

        org.Events.Should().ContainSingle();
        org.Events[0].EntityId.Should().Be("evt-1");
        org.Events[0].MunicipalityData.Name.Should().Be("City");

        org.MunicipalityData.Name.Should().Be("Castignano");
        org.MunicipalityData.LogoPath.Should().Be("/m.png");
    }

    [Test]
    public void MapOrganizations_SkipsWhenTaxCodeMissing()
    {
        var json = """
            [
              { "legalName": "No tax code" }
            ]
            """;

        var mapper = new OrganizationMapper();
        var organizations = mapper
            .MapOrganizations(new[] { TestJson.Parse(json).EnumerateArray().First() })
            .ToList();

        organizations.Should().BeEmpty();
    }
}
