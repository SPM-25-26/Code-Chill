using System;
using System.Linq;
using ChillCodeBackend.Services.Ingestion.Mappers;
using FluentAssertions;
using NUnit.Framework;

namespace ChillCodeBackend.Tests.Services.Ingestion.Mappers;

[TestFixture]
public class EventMapperTests
{
    [Test]
    public void MapEvents_MapsFieldsAndDefaults()
    {
        var json = """
            [
              {
                "id": "evt-1",
                "title": "A concert",
                "address": "Main square",
                "description": "Fun",
                "typology": "Music",
                "primaryImage": "/img.png",
                "gallery": ["/g1.png", " "],
                "latitude": 10.1,
                "lng": 20.2,
                "date": "2025-01-01T12:00:00Z",
                "organizer": { "taxCode": "TX1", "legalName": "Org" },
                "neighbors": [
                  { "entityId": "n1", "title": "Neighbor", "category": "Sleep", "imagePath": "/n1.png", "extraInfo": "info" }
                ],
                "municipality": { "name": "Castignano" }
              }
            ]
            """;

        var mapper = new EventMapper();
        var events = mapper
            .MapEvents(new[] { TestJson.Parse(json).EnumerateArray().First() })
            .ToList();

        events.Should().HaveCount(1);
        var evt = events[0];
        evt.Identifier.Should().Be("evt-1");
        evt.Title.Should().Be("A concert");
        evt.Latitude.Should().Be(10.1);
        evt.Longitude.Should().Be(20.2);
        evt.Date.Should().Be(DateTime.Parse("2025-01-01T12:00:00Z"));
        evt.Organizer.Should().NotBeNull();
        evt.Organizer!.TaxCode.Should().Be("TX1");
        evt.Neighbors.Should().HaveCount(1);
        evt.MunicipalityData.Name.Should().Be("Castignano");
    }

    [Test]
    public void MapEvents_ReturnsEmptyWhenIdentifierMissing()
    {
        var json = """
            [
              { "title": "No id" }
            ]
            """;

        var mapper = new EventMapper();
        var events = mapper
            .MapEvents(new[] { TestJson.Parse(json).EnumerateArray().First() })
            .ToList();

        events.Should().BeEmpty();
    }

    [Test]
    public void MapEvents_OrganizerNullWhenTaxCodeMissing()
    {
        var json = """
            [
              { "id": "evt-2", "organizer": { "legalName": "Org without tax code" } }
            ]
            """;

        var mapper = new EventMapper();
        var events = mapper
            .MapEvents(new[] { TestJson.Parse(json).EnumerateArray().First() })
            .ToList();

        events.Should().ContainSingle();
        events[0].Organizer.Should().BeNull();
    }
}
