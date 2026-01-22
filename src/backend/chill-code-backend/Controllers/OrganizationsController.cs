using System;
using System.Collections.Generic;
using ChillCodeBackend.Data.Repositories.Municipalities;
using ChillCodeBackend.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace ChillCodeBackend.Controllers
{
    [ApiController]
    [Route("api/organizations")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationRepository _organizationRepository;

        public OrganizationsController(IOrganizationRepository organizationRepository)
        {
            _organizationRepository = organizationRepository;
        }

        /// <summary>
        /// Get list of organization cards for a municipality.
        /// </summary>
        [HttpGet("card-list")]
        public async Task<ActionResult<List<OrganizationCardDto>>> GetCardList(
            [FromQuery] string municipality,
            [FromQuery] string? language,
            CancellationToken ct
        )
        {
            if (string.IsNullOrWhiteSpace(municipality))
            {
                return BadRequest("Municipality is required.");
            }

            var lang = string.IsNullOrWhiteSpace(language) ? "it" : language.Trim();

            var normalized = municipality.Trim();
            var organizations = await _organizationRepository.GetByMunicipalityAsync(
                normalized,
                ct
            );
            var filtered = organizations.Where(o => o.Language == lang);

            var result = filtered
                .Select(o => new OrganizationCardDto
                {
                    TaxCode = o.TaxCode,
                    LegalName = o.LegalName,
                    PrimaryImagePath = o.PrimaryImagePath,
                    Type = o.Type,
                    Address = o.Address,
                    MunicipalityName = o.MunicipalityData?.Name ?? string.Empty,
                    MunicipalityLogo = o.MunicipalityData?.LogoPath ?? string.Empty,
                    Latitude = o.Latitude,
                    Longitude = o.Longitude
                })
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Get organization detail by tax code.
        /// </summary>
        [HttpGet("detail/{taxCode}")]
        public async Task<ActionResult<OrganizationDetailDto>> GetDetail(
            [FromRoute] string taxCode,
            [FromQuery] string? language,
            CancellationToken ct
        )
        {
            if (string.IsNullOrWhiteSpace(taxCode))
            {
                return BadRequest("Tax code is required.");
            }

            var organization = await _organizationRepository.GetByTaxCodeAsync(taxCode.Trim(), ct);
            if (organization == null)
            {
                return NotFound(new { error = "Organization not found." });
            }

            if (!string.IsNullOrWhiteSpace(language))
            {
                var lang = language.Trim();
                if (!string.Equals(organization.Language, lang, StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new { error = "Organization not found." });
                }
            }

            var dto = new OrganizationDetailDto
            {
                TaxCode = organization.TaxCode,
                LegalName = organization.LegalName,
                PrimaryImagePath = organization.PrimaryImagePath,
                Type = organization.Type,
                Address = organization.Address,
                Description = organization.Description,
                MainFunction = organization.MainFunction,
                FoundationDate = organization.FoundationDate,
                LegalStatus = organization.LegalStatus,
                Gallery = organization.Gallery ?? new List<string>(),
                Email = organization.Email,
                Telephone = organization.Telephone,
                Website = organization.Website,
                Instagram = organization.Instagram,
                Facebook = organization.Facebook,
                Latitude = organization.Latitude,
                Longitude = organization.Longitude,
                Neighbors = organization.Neighbors ?? new List<Models.Entities.NeighborSummary>(),
                NearestCarPark = organization.NearestCarPark,
                OwnedPoi = organization.OwnedPoi ?? new List<Models.Entities.OwnedPoiSummary>(),
                Offers = organization.Offers ?? new List<Models.Entities.OfferInfo>(),
                Events = organization.Events ?? new List<Models.Entities.OrganizationEventCard>(),
                MunicipalityData = organization.MunicipalityData ?? new Models.Entities.MunicipalityInfo(),
                SourceLastUpdated = organization.SourceLastUpdated,
                Language = organization.Language
            };

            return Ok(dto);
        }
    }
}
