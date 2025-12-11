using System.Text.Json;
using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Services.Ingestion.Mappers
{
    /// <summary>
    /// Maps Unicam event detail payloads into Event entities.
    /// </summary>
    public class EventMapper : JsonMapperBase
    {
        public IEnumerable<Event> MapEvents(IEnumerable<JsonElement> items)
        {
            foreach (var item in items)
            {
                if (!TryGetString(item, out var identifier, "id", "identifier"))
                {
                    continue;
                }

                yield return new Event
                {
                    Identifier = identifier,
                    Title = GetString(item, "title"),
                    Address = GetString(item, "address"),
                    Description = GetString(item, "description"),
                    Typology = GetString(item, "typology", "type"),
                    PrimaryImage = GetString(item, "primaryImage", "imagePath"),
                    Gallery = GetStringList(item, "gallery", "images"),
                    VirtualTours = GetStringList(item, "virtualTours"),
                    Audience = GetString(item, "audience"),
                    Email = GetString(item, "email"),
                    Telephone = GetString(item, "telephone", "phone"),
                    Website = GetString(item, "website"),
                    Facebook = GetString(item, "facebook"),
                    Instagram = GetString(item, "instagram"),
                    Latitude = GetDouble(item, "latitude", "lat"),
                    Longitude = GetDouble(item, "longitude", "lng", "lon"),
                    Neighbors = MapNeighbors(item),
                    NearestCarPark = MapCarPark(item),
                    Date = GetDateTime(item, "date"),
                    StartDate = GetDateTime(item, "startDate"),
                    EndDate = GetDateTime(item, "endDate"),
                    Organizer = MapOrganizer(item),
                    TicketsAndCosts = MapTickets(item),
                    MunicipalityData = MapMunicipalityInfo(item),
                };
            }
        }

        private MunicipalityInfo MapMunicipalityInfo(JsonElement root)
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

        private EventOrganizer? MapOrganizer(JsonElement root)
        {
            if (
                !root.TryGetProperty("organizer", out var organizer)
                || organizer.ValueKind != JsonValueKind.Object
            )
            {
                return null;
            }

            if (!TryGetString(organizer, out var taxCode, "taxCode", "identifier"))
            {
                return null;
            }

            return new EventOrganizer
            {
                TaxCode = taxCode,
                LegalName = GetString(organizer, "legalName", "name"),
                Website = GetString(organizer, "website"),
            };
        }

        private List<TicketInfo> MapTickets(JsonElement root)
        {
            if (
                !root.TryGetProperty("ticketsAndCosts", out var tickets)
                || tickets.ValueKind != JsonValueKind.Array
            )
            {
                return new List<TicketInfo>();
            }

            var result = new List<TicketInfo>();
            foreach (var ticket in tickets.EnumerateArray())
            {
                result.Add(
                    new TicketInfo
                    {
                        Description = GetString(ticket, "description"),
                        PriceSpecificationCurrencyValue = GetDecimal(
                            ticket,
                            "priceSpecificationCurrencyValue",
                            "price"
                        ),
                        Currency = GetString(ticket, "currency"),
                        ValidityDescription = GetString(ticket, "validityDescription"),
                        ValidityStartDate = GetDateTime(ticket, "validityStartDate"),
                        ValidityEndDate = GetDateTime(ticket, "validityEndDate"),
                        UserTypeName = GetString(ticket, "userTypeName"),
                        UserTypeDescription = GetString(ticket, "userTypeDescription"),
                        TicketDescription = GetString(ticket, "ticketDescription"),
                    }
                );
            }

            return result;
        }
    }
}
