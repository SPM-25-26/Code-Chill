using System.Text.Json;
using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Services.Ingestion.Mappers
{
    public class ServiceMapper : JsonMapperBase
    {
        public IEnumerable<Service> MapServices(IEnumerable<JsonElement> items)
        {
            foreach (var item in items)
            {
                if (!TryGetString(item, out var identifier, "identifier", "id"))
                {
                    continue;
                }

                yield return new Service
                {
                    Identifier = identifier,
                    SourceLastUpdated = GetDateTime(item, "updatedAt", "lastUpdatedAt"),
                    Name = GetString(item, "name", "title"),
                    Address = GetString(item, "address"),
                    Description = GetString(item, "description"),
                    Typology = GetString(item, "typology", "type"),
                    SpacesForDisabled = GetInt(item, "spacesForDisabled"),
                    PayingParkingSpaces = GetInt(item, "payingParkingSpaces"),
                    AvailableParkingSpaces = GetInt(item, "availableParkingSpaces"),
                    PostiAutoVenduti = GetInt(item, "postiAutoVenduti"),
                    TotalNumberOfCarParkSpaces = GetInt(item, "totalNumberOfCarParkSpaces"),
                    Latitude = GetDouble(item, "latitude", "lat"),
                    Longitude = GetDouble(item, "longitude", "lng"),
                    PrimaryImage = GetString(item, "primaryImage", "imagePath"),
                    Gallery = GetStringList(item, "gallery", "images"),
                    Email = GetString(item, "email"),
                    Telephone = GetString(item, "telephone", "phone"),
                    Website = GetString(item, "website"),
                    Instagram = GetString(item, "instagram"),
                    Facebook = GetString(item, "facebook"),
                    Price = GetString(item, "price"),
                    ReservationUrl = GetString(item, "reservationUrl"),
                    Neighbors = MapNeighbors(item),
                    NearestCarPark = MapCarPark(item),
                    OpeningHours = MapOpeningHours(item),
                    TemporaryClosure = MapTemporaryClosure(item),
                    Booking = MapBooking(item),
                    Locations = MapLocations(item),
                    AssociatedServices = MapAssociatedServices(item),
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

        private ServiceOpeningHours? MapOpeningHours(JsonElement root)
        {
            if (
                !root.TryGetProperty("openingHours", out var oh)
                || oh.ValueKind == JsonValueKind.Null
            )
            {
                return null;
            }

            return new ServiceOpeningHours
            {
                Opens = ParseTimeOnly(GetString(oh, "opens")),
                Closes = ParseTimeOnly(GetString(oh, "closes")),
                Description = GetString(oh, "description"),
                Day = GetString(oh, "day"),
                AdmissionType = MapAdmissionType(oh),
                TimeInterval = MapTimeInterval(oh),
            };
        }

        private ServiceTemporaryClosure? MapTemporaryClosure(JsonElement root)
        {
            if (
                !root.TryGetProperty("temporaryClosure", out var tc)
                || tc.ValueKind == JsonValueKind.Null
            )
            {
                return null;
            }

            return new ServiceTemporaryClosure
            {
                ReasonForClosure = GetString(tc, "reasonForClosure"),
                Opens = ParseTimeOnly(GetString(tc, "opens")),
                Closes = ParseTimeOnly(GetString(tc, "closes")),
                Description = GetString(tc, "description"),
                Day = GetString(tc, "day"),
                TimeInterval = MapTimeInterval(tc),
            };
        }

        private ServiceBooking? MapBooking(JsonElement root)
        {
            if (
                !root.TryGetProperty("booking", out var booking)
                || booking.ValueKind == JsonValueKind.Null
            )
            {
                return null;
            }

            return new ServiceBooking
            {
                Name = GetString(booking, "name"),
                Description = GetString(booking, "description"),
                TimeInterval = MapTimeInterval(booking),
            };
        }

        private AdmissionTypeInfo? MapAdmissionType(JsonElement root)
        {
            if (
                !root.TryGetProperty("admissionType", out var at)
                || at.ValueKind == JsonValueKind.Null
            )
            {
                return null;
            }

            return new AdmissionTypeInfo
            {
                Name = GetString(at, "name"),
                Description = GetString(at, "description"),
            };
        }

        private TimeIntervalInfo? MapTimeInterval(JsonElement root)
        {
            if (
                !root.TryGetProperty("timeInterval", out var ti)
                || ti.ValueKind == JsonValueKind.Null
            )
            {
                return null;
            }

            return new TimeIntervalInfo
            {
                Date = GetDateTime(ti, "date"),
                StartDate = GetDateTime(ti, "startDate"),
                EndDate = GetDateTime(ti, "endDate"),
            };
        }

        private List<LocationSummary> MapLocations(JsonElement root)
        {
            if (
                !root.TryGetProperty("locations", out var locations)
                || locations.ValueKind != JsonValueKind.Array
            )
            {
                return new List<LocationSummary>();
            }

            var result = new List<LocationSummary>();
            foreach (var loc in locations.EnumerateArray())
            {
                result.Add(
                    new LocationSummary
                    {
                        Identifier = GetString(loc, "identifier", "id"),
                        OfficialName = GetString(loc, "officialName", "name"),
                        ImagePath = GetString(loc, "imagePath"),
                        Category = GetString(loc, "category"),
                    }
                );
            }

            return result;
        }

        private List<AssociatedServiceSummary> MapAssociatedServices(JsonElement root)
        {
            if (
                !root.TryGetProperty("associatedServices", out var services)
                || services.ValueKind != JsonValueKind.Array
            )
            {
                return new List<AssociatedServiceSummary>();
            }

            var result = new List<AssociatedServiceSummary>();
            foreach (var svc in services.EnumerateArray())
            {
                result.Add(
                    new AssociatedServiceSummary
                    {
                        Identifier = GetString(svc, "identifier", "id"),
                        Name = GetString(svc, "name"),
                        ImagePath = GetString(svc, "imagePath"),
                    }
                );
            }

            return result;
        }

        private static TimeOnly? ParseTimeOnly(string? timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
            {
                return null;
            }

            if (TimeOnly.TryParse(timeString, out var time))
            {
                return time;
            }

            if (DateTime.TryParse(timeString, out var dateTime))
            {
                return TimeOnly.FromDateTime(dateTime);
            }

            return null;
        }
    }
}
