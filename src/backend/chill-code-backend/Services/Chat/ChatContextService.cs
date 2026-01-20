using ChillCodeBackend.Data.Db;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChillCodeBackend.Services.Chat
{
    public class ChatContextService
    {
        private const int MaxPerType = 80;
        private readonly ChillCodeDbContext _dbContext;

        public ChatContextService(ChillCodeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<TourismContextItem>> BuildContextAsync(
            ChatContextRequest request,
            CancellationToken cancellationToken
        )
        {
            var language = string.IsNullOrWhiteSpace(request.Language) ? "it" : request.Language;
            var municipality = string.IsNullOrWhiteSpace(request.Municipality)
                ? null
                : request.Municipality.Trim();

            var items = new List<TourismContextItem>();

            var poiQuery = _dbContext.PointsOfInterest.AsNoTracking().Where(p => p.Language == language);
            if (!string.IsNullOrWhiteSpace(municipality))
            {
                poiQuery = poiQuery.Where(p => p.MunicipalityData.Name == municipality);
            }

            var pois = await poiQuery.Take(MaxPerType).ToListAsync(cancellationToken);
            items.AddRange(
                pois.Select(p => new TourismContextItem
                {
                    Type = "poi",
                    Id = p.Identifier,
                    Name = string.IsNullOrWhiteSpace(p.Title) ? p.OfficialName : p.Title,
                    Category = p.Category.ToString(),
                    Municipality = p.MunicipalityData?.Name ?? string.Empty,
                    Address = p.Address,
                    Availability = FormatOpeningHours(p.OpeningHours),
                    Notes = p.Typology,
                    DistanceKm = CalculateDistanceKm(
                        request.Latitude,
                        request.Longitude,
                        p.Latitude,
                        p.Longitude
                    )
                })
            );

            var eventQuery = _dbContext.Events.AsNoTracking().Where(e => e.Language == language);
            if (!string.IsNullOrWhiteSpace(municipality))
            {
                eventQuery = eventQuery.Where(e => e.MunicipalityData.Name == municipality);
            }

            var events = await eventQuery.Take(MaxPerType).ToListAsync(cancellationToken);
            items.AddRange(
                events.Select(e => new TourismContextItem
                {
                    Type = "event",
                    Id = e.Identifier,
                    Name = e.Title,
                    Category = e.Typology,
                    Municipality = e.MunicipalityData?.Name ?? string.Empty,
                    Address = e.Address,
                    Availability = FormatEventDates(e),
                    Notes = e.Audience,
                    DistanceKm = CalculateDistanceKm(
                        request.Latitude,
                        request.Longitude,
                        e.Latitude,
                        e.Longitude
                    )
                })
            );

            var routeQuery = _dbContext.Routes.AsNoTracking().Where(r => r.Language == language);
            if (!string.IsNullOrWhiteSpace(municipality))
            {
                routeQuery = routeQuery.Where(r => r.MunicipalityData.Name == municipality);
            }

            var routes = await routeQuery.Take(MaxPerType).ToListAsync(cancellationToken);
            items.AddRange(
                routes.Select(r => new TourismContextItem
                {
                    Type = "route",
                    Id = string.IsNullOrWhiteSpace(r.ExternalId) ? r.Name : r.ExternalId,
                    Name = r.Name,
                    Category = r.PathTheme,
                    Municipality = r.MunicipalityData?.Name ?? string.Empty,
                    Address = r.StartingPoint?.Address ?? string.Empty,
                    Availability = FormatDateRange(r.StartDate, r.EndDate),
                    Notes = r.TravellingMethod,
                    DistanceKm = CalculateDistanceKm(
                        request.Latitude,
                        request.Longitude,
                        r.StartingPoint?.Latitude,
                        r.StartingPoint?.Longitude
                    )
                })
            );

            var orgQuery = _dbContext.Organizations.AsNoTracking().Where(o => o.Language == language);
            if (!string.IsNullOrWhiteSpace(municipality))
            {
                orgQuery = orgQuery.Where(o => o.MunicipalityData.Name == municipality);
            }

            var organizations = await orgQuery.Take(MaxPerType).ToListAsync(cancellationToken);
            items.AddRange(
                organizations.Select(o => new TourismContextItem
                {
                    Type = "organization",
                    Id = o.TaxCode,
                    Name = o.LegalName,
                    Category = o.Type,
                    Municipality = o.MunicipalityData?.Name ?? string.Empty,
                    Address = o.Address,
                    Notes = o.MainFunction,
                    DistanceKm = CalculateDistanceKm(
                        request.Latitude,
                        request.Longitude,
                        o.Latitude,
                        o.Longitude
                    )
                })
            );

            var ordered = OrderItems(items, request.Latitude, request.Longitude);
            var maxItems = request.MaxItems <= 0 ? 40 : request.MaxItems;
            return ordered.Take(maxItems).ToList();
        }

        private static IEnumerable<TourismContextItem> OrderItems(
            IEnumerable<TourismContextItem> items,
            double? latitude,
            double? longitude
        )
        {
            if (latitude.HasValue && longitude.HasValue)
            {
                return items.OrderBy(i => i.DistanceKm ?? double.MaxValue).ThenBy(i => i.Name);
            }

            return items.OrderBy(i => i.Type).ThenBy(i => i.Name);
        }

        private static string FormatEventDates(Event ev)
        {
            if (ev.StartDate.HasValue || ev.EndDate.HasValue)
            {
                return FormatDateRange(ev.StartDate, ev.EndDate);
            }

            return ev.Date.HasValue ? ev.Date.Value.ToString("yyyy-MM-dd") : string.Empty;
        }

        private static string FormatDateRange(DateTime? start, DateTime? end)
        {
            if (start.HasValue && end.HasValue)
            {
                return $"{start:yyyy-MM-dd} to {end:yyyy-MM-dd}";
            }

            if (start.HasValue)
            {
                return $"from {start:yyyy-MM-dd}";
            }

            if (end.HasValue)
            {
                return $"until {end:yyyy-MM-dd}";
            }

            return string.Empty;
        }

        private static string FormatOpeningHours(IEnumerable<OpeningHoursPeriod> hours)
        {
            if (hours == null)
            {
                return string.Empty;
            }

            var segments = hours
                .Where(h => !string.IsNullOrWhiteSpace(h.DayOfWeek))
                .Take(3)
                .Select(h => $"{h.DayOfWeek} {h.Opens}-{h.Closes}")
                .ToList();

            return segments.Count == 0 ? string.Empty : string.Join("; ", segments);
        }

        private static double? CalculateDistanceKm(
            double? fromLat,
            double? fromLng,
            double? toLat,
            double? toLng
        )
        {
            if (!fromLat.HasValue || !fromLng.HasValue || !toLat.HasValue || !toLng.HasValue)
            {
                return null;
            }

            const double radius = 6371;
            var dLat = DegreesToRadians(toLat.Value - fromLat.Value);
            var dLng = DegreesToRadians(toLng.Value - fromLng.Value);
            var lat1 = DegreesToRadians(fromLat.Value);
            var lat2 = DegreesToRadians(toLat.Value);

            var h =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h)));
            return radius * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
