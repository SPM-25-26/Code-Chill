using System;
using System.Collections.Generic;

namespace ChillCodeBackend.Models.Entities
{
    public class Organization
    {
        public string TaxCode { get; set; } = string.Empty;  // PRIMARY KEY

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTime? SourceLastUpdated { get; set; }
        public string Language { get; set; } = "it";

        public string LegalName { get; set; } = string.Empty;
        public string PrimaryImagePath { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MainFunction { get; set; } = string.Empty;
        public DateTime? FoundationDate { get; set; }
        public string LegalStatus { get; set; } = string.Empty;

        public List<string> Gallery { get; set; } = new();
        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Instagram { get; set; } = string.Empty;
        public string Facebook { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public List<NeighborSummary> Neighbors { get; set; } = new();
        public CarParkInfo? NearestCarPark { get; set; }

        // Complex nested structures
        public List<OwnedPoiSummary> OwnedPoi { get; set; } = new();
        public List<OfferInfo> Offers { get; set; } = new();
        public List<OrganizationEventCard> Events { get; set; } = new();

        public MunicipalityInfo MunicipalityData { get; set; } = new();
    }

    public class OwnedPoiSummary
    {
        public string Identifier { get; set; } = string.Empty;
        public string OfficialName { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class OfferInfo
    {
        public string Description { get; set; } = string.Empty;
        public decimal? PriceSpecificationCurrencyValue { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string ValidityDescription { get; set; } = string.Empty;
        public DateTime? ValidityStartDate { get; set; }
        public DateTime? ValidityEndDate { get; set; }
        public string UserTypeName { get; set; } = string.Empty;
        public string UserTypeDescription { get; set; } = string.Empty;
        public string TicketDescription { get; set; } = string.Empty;
    }

    public class OrganizationEventCard
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string BadgeText { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public MunicipalityInfo MunicipalityData { get; set; } = new();
        public string Date { get; set; } = string.Empty;
    }
}
