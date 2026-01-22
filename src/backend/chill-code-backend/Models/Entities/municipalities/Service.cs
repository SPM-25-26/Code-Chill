using System;
using System.Collections.Generic;

namespace ChillCodeBackend.Models.Entities
{
    public class Service
    {
        public string Identifier { get; set; } = string.Empty;  // PRIMARY KEY

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTime? SourceLastUpdated { get; set; }
        public string Language { get; set; } = "it";

        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Typology { get; set; } = string.Empty;

        // Parking-specific fields
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

        // Complex structures
        public ServiceOpeningHours? OpeningHours { get; set; }
        public ServiceTemporaryClosure? TemporaryClosure { get; set; }
        public ServiceBooking? Booking { get; set; }
        public List<LocationSummary> Locations { get; set; } = new();
        public List<AssociatedServiceSummary> AssociatedServices { get; set; } = new();

        public MunicipalityInfo MunicipalityData { get; set; } = new();
    }

    public class ServiceOpeningHours
    {
        public TimeOnly? Opens { get; set; }
        public TimeOnly? Closes { get; set; }
        public string Description { get; set; } = string.Empty;
        public AdmissionTypeInfo? AdmissionType { get; set; }
        public TimeIntervalInfo? TimeInterval { get; set; }
        public string Day { get; set; } = string.Empty;
    }

    public class ServiceTemporaryClosure
    {
        public string ReasonForClosure { get; set; } = string.Empty;
        public TimeOnly? Opens { get; set; }
        public TimeOnly? Closes { get; set; }
        public string Description { get; set; } = string.Empty;
        public TimeIntervalInfo? TimeInterval { get; set; }
        public string Day { get; set; } = string.Empty;
    }

    public class ServiceBooking
    {
        public TimeIntervalInfo? TimeInterval { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class AdmissionTypeInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class TimeIntervalInfo
    {
        public DateTime? Date { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class LocationSummary
    {
        public string Identifier { get; set; } = string.Empty;
        public string OfficialName { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class AssociatedServiceSummary
    {
        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
    }
}
