using System;
using System.Linq;
using ChillCodeBackend.Services.Ingestion.Mappers;
using FluentAssertions;
using NUnit.Framework;

namespace ChillCodeBackend.Tests.Services.Ingestion.Mappers;

[TestFixture]
public class RouteMapperTests
{
    [Test]
    public void MapRoutes_LeavesMissingDatesNull()
    {
        var json = """
        [
          {
            "name": "Route A",
            "imagePath": "/img.png",
            "description": "desc"
          }
        ]
        """;

        var mapper = new RouteMapper();
        var routes = mapper.MapRoutes(new[] { TestJson.Parse(json).EnumerateArray().First() }).ToList();

        routes.Should().ContainSingle();
        routes[0].StartDate.Should().BeNull();
        routes[0].EndDate.Should().BeNull();
    }

    [Test]
    public void MapRoutes_MapsProvidedDates()
    {
        var json = """
        [
          {
            "name": "Route B",
            "startDate": "2025-02-01T00:00:00Z",
            "endDate": "2025-02-02T00:00:00Z"
          }
        ]
        """;

        var mapper = new RouteMapper();
        var routes = mapper.MapRoutes(new[] { TestJson.Parse(json).EnumerateArray().First() }).ToList();

        routes.Should().ContainSingle();
        routes[0].StartDate.Should().Be(DateTime.Parse("2025-02-01T00:00:00Z"));
        routes[0].EndDate.Should().Be(DateTime.Parse("2025-02-02T00:00:00Z"));
    }

    [Test]
    public void MapRoutes_SkipsRoutesWithoutName()
    {
        var json = """
        [
          { "description": "no name" }
        ]
        """;

        var mapper = new RouteMapper();
        var routes = mapper.MapRoutes(new[] { TestJson.Parse(json).EnumerateArray().First() }).ToList();

        routes.Should().BeEmpty();
    }
}
