using System;
using System.Linq;
using ChillCodeBackend.Services.Ingestion.Mappers;
using FluentAssertions;
using NUnit.Framework;

namespace ChillCodeBackend.Tests.Services.Ingestion.Mappers;

[TestFixture]
public class ServiceMapperTests
{
    [Test]
    public void MapServices_MapsComplexStructures()
    {
        var json = """
            [
              {
                "id": "svc-1",
                "name": "Service One",
                "openingHours": {
                  "opens": "08:30",
                  "closes": "2025-01-01T12:00:00Z",
                  "day": "Mon",
                  "admissionType": { "name": "Free", "description": "No ticket" },
                  "timeInterval": { "startDate": "2025-02-01T00:00:00Z" }
                },
                "temporaryClosure": {
                  "reasonForClosure": "Maintenance",
                  "opens": "09:00",
                  "closes": "18:00",
                  "day": "Tue"
                },
                "booking": {
                  "name": "Book",
                  "description": "Required",
                  "timeInterval": { "date": "2025-03-01T00:00:00Z" }
                },
                "locations": [
                  { "identifier": "loc-1", "officialName": "Loc One", "imagePath": "/loc.png", "category": "Services" }
                ],
                "associatedServices": [
                  { "identifier": "assoc-1", "name": "Assoc One", "imagePath": "/a.png" }
                ],
                "neighbors": [
                  { "entityId": "n1", "title": "Neighbor", "category": "Event", "imagePath": "/n.png" }
                ],
                "municipalityData": { "name": "Castignano", "logoPath": "/logo.png" }
              }
            ]
            """;

        var mapper = new ServiceMapper();
        var services = mapper
            .MapServices(new[] { TestJson.Parse(json).EnumerateArray().First() })
            .ToList();

        services.Should().ContainSingle();
        var service = services[0];
        service.Identifier.Should().Be("svc-1");
        service.Name.Should().Be("Service One");

        service.OpeningHours.Should().NotBeNull();
        service.OpeningHours!.Opens.Should().Be(TimeOnly.Parse("08:30"));
        service.OpeningHours.Closes.Should()
            .Be(TimeOnly.FromDateTime(DateTime.Parse("2025-01-01T12:00:00Z")));
        service.OpeningHours.Day.Should().Be("Mon");
        service.OpeningHours.AdmissionType!.Name.Should().Be("Free");
        service.OpeningHours.TimeInterval!.StartDate.Should()
            .Be(DateTime.Parse("2025-02-01T00:00:00Z"));

        service.TemporaryClosure.Should().NotBeNull();
        service.TemporaryClosure!.ReasonForClosure.Should().Be("Maintenance");
        service.TemporaryClosure.Opens.Should().Be(TimeOnly.Parse("09:00"));

        service.Booking.Should().NotBeNull();
        service.Booking!.Name.Should().Be("Book");
        service.Booking.TimeInterval!.Date.Should().Be(DateTime.Parse("2025-03-01T00:00:00Z"));

        service.Locations.Should().ContainSingle();
        service.Locations[0].Identifier.Should().Be("loc-1");
        service.AssociatedServices.Should().ContainSingle();
        service.AssociatedServices[0].Identifier.Should().Be("assoc-1");

        service.Neighbors.Should().ContainSingle();
        service.Neighbors[0].EntityId.Should().Be("n1");

        service.MunicipalityData.Name.Should().Be("Castignano");
        service.MunicipalityData.LogoPath.Should().Be("/logo.png");
    }

    [Test]
    public void MapServices_SkipsWhenIdentifierMissing()
    {
        var json = """
            [
              { "name": "No id" }
            ]
            """;

        var mapper = new ServiceMapper();
        var services = mapper
            .MapServices(new[] { TestJson.Parse(json).EnumerateArray().First() })
            .ToList();

        services.Should().BeEmpty();
    }
}
