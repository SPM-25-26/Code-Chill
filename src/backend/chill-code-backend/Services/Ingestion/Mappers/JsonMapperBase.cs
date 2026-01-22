using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace ChillCodeBackend.Services.Ingestion.Mappers
{
    /// <summary>
    /// Base class providing convenience helpers for reading JSON values.
    /// </summary>
    public abstract class JsonMapperBase
    {
        protected string GetString(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var value))
                {
                    // Only try to get string if the value is actually a string
                    if (value.ValueKind == JsonValueKind.String)
                    {
                        return value.GetString() ?? string.Empty;
                    }
                    // If it's null, return empty string
                    if (value.ValueKind == JsonValueKind.Null)
                    {
                        return string.Empty;
                    }
                }
            }

            return string.Empty;
        }

        protected bool TryGetString(
            JsonElement element,
            out string value,
            params string[] propertyNames
        )
        {
            value = GetString(element, propertyNames);
            return !string.IsNullOrWhiteSpace(value);
        }

        protected int? GetInt(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (
                    element.TryGetProperty(propertyName, out var value)
                    && value.ValueKind == JsonValueKind.Number
                    && value.TryGetInt32(out var number)
                )
                {
                    return number;
                }
            }

            return null;
        }

        protected double? GetDouble(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (
                    element.TryGetProperty(propertyName, out var value)
                    && value.ValueKind == JsonValueKind.Number
                    && value.TryGetDouble(out var number)
                )
                {
                    return number;
                }
            }

            return null;
        }

        protected decimal? GetDecimal(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (
                    element.TryGetProperty(propertyName, out var value)
                    && value.ValueKind == JsonValueKind.Number
                    && value.TryGetDecimal(out var number)
                )
                {
                    return number;
                }
            }

            return null;
        }

        protected DateTime? GetDateTime(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var value))
                {
                    if (value.ValueKind == JsonValueKind.String)
                    {
                        var text = value.GetString();
                        if (
                            !string.IsNullOrWhiteSpace(text)
                            && DateTime.TryParse(
                                text,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.AssumeUniversal,
                                out var parsed
                            )
                        )
                        {
                            return parsed;
                        }
                    }
                    else if (
                        value.ValueKind == JsonValueKind.Number
                        && value.TryGetInt64(out var epochMillis)
                    )
                    {
                        return DateTimeOffset.FromUnixTimeMilliseconds(epochMillis).UtcDateTime;
                    }
                }
            }

            return null;
        }

        protected List<string> GetStringList(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (
                    element.TryGetProperty(propertyName, out var value)
                    && value.ValueKind == JsonValueKind.Array
                )
                {
                    return value
                        .EnumerateArray()
                        .Select(v =>
                        {
                            // Handle string elements
                            if (v.ValueKind == JsonValueKind.String)
                            {
                                return v.GetString();
                            }
                            // Handle object elements (skip them)
                            return null;
                        })
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .Select(v => v!.Trim())
                        .ToList();
                }
            }

            return new List<string>();
        }
    }
}
