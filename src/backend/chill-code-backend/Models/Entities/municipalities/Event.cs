using System.ComponentModel.DataAnnotations;

namespace ChillCodeBackend.Models.Entities
{
    public class Event
    {
        [Key]
        public string Identifier { get; set; } = default!;

        public string Title { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Typology { get; set; } = string.Empty;

        public string PrimaryImage { get; set; } = string.Empty;

        public List<string> Gallery { get; set; } = [];
        public List<string> VirtualTours { get; set; } = [];

        public string Audience { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Facebook { get; set; } = string.Empty;
        public string Instagram { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public List<NeighborSummary> Neighbors { get; set; } = new();

        public CarParkInfo? NearestCarPark { get; set; }

        public DateTime? Date { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public EventOrganizer? Organizer { get; set; }

        public List<TicketInfo> TicketsAndCosts { get; set; } = new();

        public MunicipalityInfo MunicipalityData { get; set; } = new();
    }

    public class NeighborSummary
    {
        public string EntityId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        // Sleep, Eat, Service, Event, etc.
        public string Category { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;
        public string ExtraInfo { get; set; } = string.Empty;
    }

    public class CarParkInfo
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public double? Distance { get; set; }
    }

    public class EventOrganizer
    {
        public string TaxCode { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
    }

    public class TicketInfo
    {
        public string Description { get; set; } = string.Empty;
        public decimal? PriceSpecificationCurrencyValue { get; set; }
        public string Currency { get; set; } = "EUR";
        public string ValidityDescription { get; set; } = string.Empty;

        public DateTime? ValidityStartDate { get; set; }
        public DateTime? ValidityEndDate { get; set; }

        public string UserTypeName { get; set; } = string.Empty;
        public string UserTypeDescription { get; set; } = string.Empty;
        public string TicketDescription { get; set; } = string.Empty;
    }
}
