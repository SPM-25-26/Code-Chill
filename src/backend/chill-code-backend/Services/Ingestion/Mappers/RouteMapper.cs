using System.Text.Json;
using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Services.Ingestion.Mappers
{
    /// <summary>
    /// Maps Unicam route detail payloads into Route entities.
    /// </summary>
    public class RouteMapper : JsonMapperBase
    {
        public IEnumerable<Models.Entities.Route> MapRoutes(IEnumerable<JsonElement> items)
        {
            foreach (var item in items)
            {
                if (!TryGetString(item, out var externalId, "id", "identifier", "entityId"))
                {
                    continue;
                }

                if (!TryGetString(item, out var name, "name", "title"))
                {
                    continue;
                }

                yield return new Models.Entities.Route
                {
                    ExternalId = externalId,
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
                    OrganizationInstagram = GetString(
                        item,
                        "organizationInstragram",
                        "organizationInstagram"
                    ),
                    OrganizationTelephone = GetString(item, "organizationTelephone"),
                    Website = GetString(item, "website"),
                    Duration = GetString(item, "duration"),
                    RouteLength = GetString(item, "routeLength", "length"),
                    StartDate = GetDateTime(item, "startDate"),
                    EndDate = GetDateTime(item, "endDate"),
                    MunicipalityData = MapMunicipality(item),
                    SourceLastUpdated = GetDateTime(
                        item,
                        "updatedAt",
                        "lastUpdatedAt",
                        "lastUpdated"
                    ),
                };
            }
        }

        private MunicipalityInfo MapMunicipality(JsonElement root)
        {
            if (
                root.TryGetProperty("municipality", out var muni)
                && muni.ValueKind != JsonValueKind.Null
            )
            {
                return new MunicipalityInfo
                {
                    Name = GetString(muni, "name"),
                    LogoPath = GetString(muni, "logoPath"),
                };
            }

            return new MunicipalityInfo();
        }
    }
}
