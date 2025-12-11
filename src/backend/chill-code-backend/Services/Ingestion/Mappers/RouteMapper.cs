using System.Text.Json;
using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Services.Ingestion.Mappers
{
    /// <summary>
    /// Maps Unicam route detail payloads into Route entities.
    /// </summary>
    public class RouteMapper : JsonMapperBase
    {
        public IEnumerable<Route> MapRoutes(IEnumerable<JsonElement> items)
        {
            foreach (var item in items)
            {
                if (!TryGetString(item, out var name, "name", "title"))
                {
                    continue;
                }

                yield return new Route
                {
                    Name = name,
                    ImagePath = GetString(item, "imagePath", "primaryImage"),
                    Number = GetString(item, "number"),
                    Description = GetString(item, "description"),
                    PathTheme = GetString(item, "pathTheme", "theme"),
                    TravellingMethod = GetString(item, "travellingMethod", "travelingMethod"),
                    ShortName = GetString(item, "shortName"),
                    OrganizationWebsite = GetString(item, "organizationWebsite"),
                    OrganizationEmail = GetString(item, "organizationEmail"),
                    OrganizationFacebook = GetString(item, "organizationFacebook"),
                    OrganizationInstragram = GetString(
                        item,
                        "organizationInstragram",
                        "organizationInstagram"
                    ),
                    OrganizationTelephone = GetString(item, "organizationTelephone"),
                    Website = GetString(item, "website"),
                    Duration = GetString(item, "duration"),
                    RouteLength = GetString(item, "routeLength", "length"),
                    StartDate = GetDateTime(item, "startDate") ?? default,
                    EndDate = GetDateTime(item, "endDate") ?? default,
                    MunicipalityData = MapMunicipality(item),
                };
            }
        }

        private Municipality MapMunicipality(JsonElement root)
        {
            if (
                root.TryGetProperty("municipality", out var muni)
                && muni.ValueKind != JsonValueKind.Null
            )
            {
                return new Municipality
                {
                    Name = GetString(muni, "name"),
                    LegalName = GetString(muni, "legalName"),
                    LogoPath = GetString(muni, "logoPath"),
                };
            }

            return new Municipality();
        }
    }
}
