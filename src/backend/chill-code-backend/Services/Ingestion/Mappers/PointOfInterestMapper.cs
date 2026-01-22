using System.Text.Json;
using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Services.Ingestion.Mappers
{
    /// <summary>
    /// Maps Unicam POI detail payloads into PointOfInterest entities.
    /// </summary>
    public class PointOfInterestMapper : JsonMapperBase
    {
        public IEnumerable<PointOfInterest> MapPointsOfInterest(
            IEnumerable<JsonElement> items,
            MobileCategory category
        )
        {
            foreach (var item in items)
            {
                if (!TryGetString(item, out var identifier, "id", "identifier", "entityId"))
                {
                    continue;
                }

                yield return new PointOfInterest
                {
                    Identifier = identifier,
                    Category = category,
                    SourceLastUpdated = GetDateTime(
                        item,
                        "updatedAt",
                        "lastUpdatedAt",
                        "lastUpdated"
                    ),
                    Title = GetString(item, "title", "officialName"),
                    OfficialName = GetString(item, "officialName", "name"),
                    Address = GetString(item, "address"),
                    Description = GetString(item, "description"),
                    Typology = GetString(item, "typology", "type"),
                    PrimaryImage = GetString(item, "primaryImagePath", "primaryImage", "imagePath"),
                    ImageThumbPath = GetString(item, "imageThumbPath", "thumbnailPath"),
                    Gallery = GetStringList(item, "gallery", "images"),
                    VirtualTours = GetStringList(item, "virtualTours"),
                    Email = GetString(item, "email"),
                    Telephone = GetString(item, "telephone", "phone"),
                    Website = GetString(item, "website"),
                    Facebook = GetString(item, "facebook"),
                    Instagram = GetString(item, "instagram"),
                    Latitude = GetDouble(item, "latitude", "lat"),
                    Longitude = GetDouble(item, "longitude", "lng", "lon"),
                    Neighbors = MapNeighbors(item),
                    NearestCarPark = MapCarPark(item),
                    OpeningHours = MapOpeningHours(item),
                    Accessibility = GetString(item, "accessibility"),
                    Prices = MapPrices(item),
                    Services = GetStringList(item, "services"),
                    Facilities = GetStringList(item, "facilities"),
                    OrganizationTaxCode = MapOwnerTaxCode(item),
                    OrganizationLegalName = MapOwnerLegalName(item),
                    MunicipalityData = MapMunicipalityInfo(item),
                };
            }
        }

        private string MapOwnerTaxCode(JsonElement root)
        {
            if (
                root.TryGetProperty("owner", out var owner)
                && owner.ValueKind == JsonValueKind.Object
            )
            {
                return GetString(owner, "taxCode");
            }

            return GetString(root, "organizationTaxCode", "taxCode");
        }

        private string MapOwnerLegalName(JsonElement root)
        {
            if (
                root.TryGetProperty("owner", out var owner)
                && owner.ValueKind == JsonValueKind.Object
            )
            {
                return GetString(owner, "legalName");
            }

            return GetString(root, "organizationLegalName", "legalName");
        }

        private MunicipalityInfo MapMunicipalityInfo(JsonElement root)
        {
            if (
                root.TryGetProperty("municipalityData", out var muni)
                && muni.ValueKind == JsonValueKind.Object
            )
            {
                return new MunicipalityInfo
                {
                    Name = GetString(muni, "name"),
                    LogoPath = GetString(muni, "logoPath"),
                };
            }

            if (
                root.TryGetProperty("municipality", out muni)
                && muni.ValueKind == JsonValueKind.Object
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

        private List<NeighborSummary> MapNeighbors(JsonElement root)
        {
            if (
                !root.TryGetProperty("neighbors", out var neighbors)
                || neighbors.ValueKind != JsonValueKind.Array
            )
            {
                return new List<NeighborSummary>();
            }

            var result = new List<NeighborSummary>();
            foreach (var neighbor in neighbors.EnumerateArray())
            {
                result.Add(
                    new NeighborSummary
                    {
                        EntityId = GetString(neighbor, "entityId", "id"),
                        Title = GetString(neighbor, "title"),
                        Category = GetString(neighbor, "category"),
                        ImagePath = GetString(neighbor, "imagePath"),
                        ExtraInfo = GetString(neighbor, "extraInfo"),
                    }
                );
            }

            return result;
        }

        private CarParkInfo? MapCarPark(JsonElement root)
        {
            if (
                !root.TryGetProperty("nearestCarPark", out var park)
                || park.ValueKind != JsonValueKind.Object
            )
            {
                return null;
            }

            return new CarParkInfo
            {
                Latitude = GetDouble(park, "latitude", "lat"),
                Longitude = GetDouble(park, "longitude", "lng", "lon"),
                Address = GetString(park, "address"),
                Distance = GetDouble(park, "distance"),
            };
        }

        private List<OpeningHoursPeriod> MapOpeningHours(JsonElement root)
        {
            if (
                !root.TryGetProperty("openingHours", out var hours)
                || hours.ValueKind != JsonValueKind.Array
            )
            {
                return new List<OpeningHoursPeriod>();
            }

            var result = new List<OpeningHoursPeriod>();
            foreach (var hour in hours.EnumerateArray())
            {
                result.Add(
                    new OpeningHoursPeriod
                    {
                        DayOfWeek = GetString(hour, "dayOfWeek", "day"),
                        Opens = GetString(hour, "opens", "openTime"),
                        Closes = GetString(hour, "closes", "closeTime"),
                        Description = GetString(hour, "description"),
                    }
                );
            }

            return result;
        }

        private List<PriceInfo> MapPrices(JsonElement root)
        {
            if (
                !root.TryGetProperty("prices", out var prices)
                || prices.ValueKind != JsonValueKind.Array
            )
            {
                return new List<PriceInfo>();
            }

            var result = new List<PriceInfo>();
            foreach (var price in prices.EnumerateArray())
            {
                result.Add(
                    new PriceInfo
                    {
                        Description = GetString(price, "description"),
                        MinPrice = GetDecimal(price, "minPrice", "minValue"),
                        MaxPrice = GetDecimal(price, "maxPrice", "maxValue"),
                        Currency = GetString(price, "currency"),
                        PriceType = GetString(price, "priceType", "type"),
                    }
                );
            }

            return result;
        }
    }
}
