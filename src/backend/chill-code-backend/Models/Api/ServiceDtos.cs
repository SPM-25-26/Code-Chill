using System;
using System.Collections.Generic;
using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Models.Api
{
    public class ServiceCardDto
    {
        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Typology { get; set; } = string.Empty;
        public string PrimaryImage { get; set; } = string.Empty;
        public string MunicipalityName { get; set; } = string.Empty;
        public string MunicipalityLogo { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class ServiceDetailDto
    {
        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Typology { get; set; } = string.Empty;

        public int? SpacesForDisabled { get; set; }
        public int? PayingParkingSpaces { get; set; }
        public int? AvailableParkingSpaces { get; set; }
        public int? PostiAutoVenduti { get; set; }
        public int? TotalNumberOfCarParkSpaces { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string PrimaryImage { get; set; } = string.Empty;
        public List<string> Gallery { get; set; } = new();

        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Instagram { get; set; } = string.Empty;
        public string Facebook { get; set; } = string.Empty;

        public string Price { get; set; } = string.Empty;
        public string ReservationUrl { get; set; } = string.Empty;

        public List<NeighborSummary> Neighbors { get; set; } = new();
        public CarParkInfo? NearestCarPark { get; set; }
        public ServiceOpeningHours? OpeningHours { get; set; }
        public ServiceTemporaryClosure? TemporaryClosure { get; set; }
        public ServiceBooking? Booking { get; set; }
        public List<LocationSummary> Locations { get; set; } = new();
        public List<AssociatedServiceSummary> AssociatedServices { get; set; } = new();

        public MunicipalityInfo MunicipalityData { get; set; } = new();
        public DateTime? SourceLastUpdated { get; set; }
        public string Language { get; set; } = "it";
    }
}
