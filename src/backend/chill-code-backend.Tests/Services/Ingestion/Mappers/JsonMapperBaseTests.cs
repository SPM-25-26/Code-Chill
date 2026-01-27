using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ChillCodeBackend.Services.Ingestion.Mappers;
using FluentAssertions;
using NUnit.Framework;

namespace ChillCodeBackend.Tests.Services.Ingestion.Mappers;

[TestFixture]
public class JsonMapperBaseTests
{
    private sealed class TestJsonMapper : JsonMapperBase
    {
        public string ReadString(JsonElement element, params string[] names) => GetString(element, names);
        public bool TryReadString(JsonElement element, out string value, params string[] names) =>
            TryGetString(element, out value, names);
        public int? ReadInt(JsonElement element, params string[] names) => GetInt(element, names);
        public double? ReadDouble(JsonElement element, params string[] names) => GetDouble(element, names);
        public decimal? ReadDecimal(JsonElement element, params string[] names) => GetDecimal(element, names);
        public DateTime? ReadDateTime(JsonElement element, params string[] names) => GetDateTime(element, names);
        public List<string> ReadStringList(JsonElement element, params string[] names) =>
            GetStringList(element, names);
    }

    [Test]
    public void GetString_ReturnsFirstMatchingStringOrEmpty()
    {
        var json = """
            {
              "primary": "value",
              "secondary": "fallback",
              "nullValue": null,
              "numberValue": 10
            }
            """;
        var element = TestJson.Parse(json);
        var mapper = new TestJsonMapper();

        mapper.ReadString(element, "missing", "secondary").Should().Be("fallback");
        mapper.ReadString(element, "nullValue").Should().BeEmpty();
        mapper.ReadString(element, "numberValue").Should().BeEmpty();
        mapper.ReadString(element, "primary", "secondary").Should().Be("value");
    }

    [Test]
    public void TryGetString_ReturnsFalseForWhitespaceAndEmpty()
    {
        var json = """
            { "name": "  " }
            """;
        var element = TestJson.Parse(json);
        var mapper = new TestJsonMapper();

        var result = mapper.TryReadString(element, out var value, "name");

        result.Should().BeFalse();
        value.Should().Be("  ");
    }

    [Test]
    public void GetIntDoubleDecimal_ReturnsNumbersWhenPresent()
    {
        var json = """
            { "int": 5, "double": 2.5, "decimal": 7.25 }
            """;
        var element = TestJson.Parse(json);
        var mapper = new TestJsonMapper();

        mapper.ReadInt(element, "int").Should().Be(5);
        mapper.ReadDouble(element, "double").Should().Be(2.5);
        mapper.ReadDecimal(element, "decimal").Should().Be(7.25m);
    }

    [Test]
    public void GetDateTime_ParsesIsoAndEpochMillis()
    {
        var json = """
            {
              "iso": "2025-01-02T03:04:05Z",
              "epoch": 1735787045000
            }
            """;
        var element = TestJson.Parse(json);
        var mapper = new TestJsonMapper();

        mapper.ReadDateTime(element, "iso")
            .Should()
            .Be(DateTime.Parse("2025-01-02T03:04:05Z"));
        mapper.ReadDateTime(element, "epoch")
            .Should()
            .Be(DateTimeOffset.FromUnixTimeMilliseconds(1735787045000).UtcDateTime);
    }

    [Test]
    public void GetStringList_TrimsAndSkipsInvalidItems()
    {
        var json = """
            { "items": [" a ", "", null, { "obj": true }, "b"] }
            """;
        var element = TestJson.Parse(json);
        var mapper = new TestJsonMapper();

        var result = mapper.ReadStringList(element, "items");

        result.Should().Equal(new[] { "a", "b" });
    }
}
