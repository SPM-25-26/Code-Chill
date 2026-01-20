using System;
using System.Collections.Generic;
using ChillCodeBackend.Models.Entities;

namespace ChillCodeBackend.Models.Api
{
    public class OrganizationCardDto
    {
        public string TaxCode { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string PrimaryImagePath { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string MunicipalityName { get; set; } = string.Empty;
        public string MunicipalityLogo { get; set; } = string.Empty;
    }

    public class OrganizationDetailDto
    {
        public string TaxCode { get; set; } = string.Empty;
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
        public List<OwnedPoiSummary> OwnedPoi { get; set; } = new();
        public List<OfferInfo> Offers { get; set; } = new();
        public List<OrganizationEventCard> Events { get; set; } = new();

        public MunicipalityInfo MunicipalityData { get; set; } = new();
        public DateTime? SourceLastUpdated { get; set; }
        public string Language { get; set; } = "it";
    }
}
