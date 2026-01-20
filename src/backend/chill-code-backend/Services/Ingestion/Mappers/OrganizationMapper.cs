using System.Text.Json;
using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Services.Ingestion.Mappers
{
    public class OrganizationMapper : JsonMapperBase
    {
        public IEnumerable<Organization> MapOrganizations(IEnumerable<JsonElement> items)
        {
            foreach (var item in items)
            {
                if (!TryGetString(item, out var taxCode, "taxCode"))
                {
                    continue;
                }

                yield return new Organization
                {
                    TaxCode = taxCode,
                    SourceLastUpdated = GetDateTime(item, "updatedAt", "lastUpdatedAt"),
                    LegalName = GetString(item, "legalName"),
                    PrimaryImagePath = GetString(item, "primaryImagePath", "imagePath"),
                    Type = GetString(item, "type"),
                    Address = GetString(item, "address"),
                    Description = GetString(item, "description"),
                    MainFunction = GetString(item, "mainFunction"),
                    FoundationDate = GetDateTime(item, "foundationDate"),
                    LegalStatus = GetString(item, "legalStatus"),
                    Gallery = GetStringList(item, "gallery", "images"),
                    Email = GetString(item, "email"),
                    Telephone = GetString(item, "telephone", "phone"),
                    Website = GetString(item, "website"),
                    Instagram = GetString(item, "instagram"),
                    Facebook = GetString(item, "facebook"),
                    Latitude = GetDouble(item, "latitude", "lat"),
                    Longitude = GetDouble(item, "longitude", "lng"),
                    Neighbors = MapNeighbors(item),
                    NearestCarPark = MapCarPark(item),
                    OwnedPoi = MapOwnedPoi(item),
                    Offers = MapOffers(item),
                    Events = MapOrganizationEvents(item),
                    MunicipalityData = MapMunicipalityInfo(item),
                };
            }
        }

        private MunicipalityInfo MapMunicipalityInfo(JsonElement root)
        {
            if (
                root.TryGetProperty("municipalityData", out var muni)
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
                !root.TryGetProperty("nearestCarPark", out var carPark)
                || carPark.ValueKind == JsonValueKind.Null
            )
            {
                return null;
            }

            return new CarParkInfo
            {
                Latitude = GetDouble(carPark, "latitude", "lat"),
                Longitude = GetDouble(carPark, "longitude", "lng"),
                Address = GetString(carPark, "address"),
                Distance = GetDouble(carPark, "distance"),
            };
        }

        private List<OwnedPoiSummary> MapOwnedPoi(JsonElement root)
        {
            if (
                !root.TryGetProperty("ownedPoi", out var ownedPoi)
                || ownedPoi.ValueKind != JsonValueKind.Array
            )
            {
                return new List<OwnedPoiSummary>();
            }

            var result = new List<OwnedPoiSummary>();
            foreach (var poi in ownedPoi.EnumerateArray())
            {
                result.Add(
                    new OwnedPoiSummary
                    {
                        Identifier = GetString(poi, "identifier", "id"),
                        OfficialName = GetString(poi, "officialName", "name"),
                        ImagePath = GetString(poi, "imagePath"),
                        Category = GetString(poi, "category"),
                    }
                );
            }

            return result;
        }

        private List<OfferInfo> MapOffers(JsonElement root)
        {
            if (
                !root.TryGetProperty("offers", out var offers)
                || offers.ValueKind != JsonValueKind.Array
            )
            {
                return new List<OfferInfo>();
            }

            var result = new List<OfferInfo>();
            foreach (var offer in offers.EnumerateArray())
            {
                result.Add(
                    new OfferInfo
                    {
                        Description = GetString(offer, "description"),
                        PriceSpecificationCurrencyValue = GetDecimal(
                            offer,
                            "priceSpecificationCurrencyValue",
                            "price"
                        ),
                        Currency = GetString(offer, "currency"),
                        ValidityDescription = GetString(offer, "validityDescription"),
                        ValidityStartDate = GetDateTime(offer, "validityStartDate"),
                        ValidityEndDate = GetDateTime(offer, "validityEndDate"),
                        UserTypeName = GetString(offer, "userTypeName"),
                        UserTypeDescription = GetString(offer, "userTypeDescription"),
                        TicketDescription = GetString(offer, "ticketDescription"),
                    }
                );
            }

            return result;
        }

        private List<OrganizationEventCard> MapOrganizationEvents(JsonElement root)
        {
            if (
                !root.TryGetProperty("events", out var events)
                || events.ValueKind != JsonValueKind.Array
            )
            {
                return new List<OrganizationEventCard>();
            }

            var result = new List<OrganizationEventCard>();
            foreach (var ev in events.EnumerateArray())
            {
                var evCard = new OrganizationEventCard
                {
                    EntityId = GetString(ev, "entityId", "id"),
                    EntityName = GetString(ev, "entityName", "name"),
                    ImagePath = GetString(ev, "imagePath"),
                    BadgeText = GetString(ev, "badgeText"),
                    Address = GetString(ev, "address"),
                    Date = GetString(ev, "date"),
                };

                if (
                    ev.TryGetProperty("municipalityData", out var muni)
                    && muni.ValueKind != JsonValueKind.Null
                )
                {
                    evCard.MunicipalityData = new MunicipalityInfo
                    {
                        Name = GetString(muni, "name"),
                        LogoPath = GetString(muni, "logoPath"),
                    };
                }

                result.Add(evCard);
            }

            return result;
        }
    }
}
