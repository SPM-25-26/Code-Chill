using System.Text.Json;

namespace ChillCodeBackend.Tests;

public static class TestJson
{
    public static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        // Clone so the caller can use the element after the document is disposed.
        return document.RootElement.Clone();
    }
}
