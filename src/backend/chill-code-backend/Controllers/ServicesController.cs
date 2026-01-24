using ChillCodeBackend.Models.Api;
using ChillCodeBackend.Data.Repositories.Municipalities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChillCodeBackend.Controllers
{
    [ApiController]
    [Route("api/services")]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceRepository _serviceRepository;

        public ServicesController(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        /// <summary>
        /// Get list of services for a municipality.
        /// </summary>
        [HttpGet("card-list")]
        public async Task<ActionResult<List<ServiceCardDto>>> GetCardList(
            [FromQuery] string? municipality,
            [FromQuery] string? language,
            CancellationToken ct
        )
        {
            var lang = string.IsNullOrWhiteSpace(language) ? "it" : language.Trim();

            IReadOnlyCollection<Models.Entities.Service> services;
            if (string.IsNullOrWhiteSpace(municipality))
            {
                services = await _serviceRepository.GetAllAsync(ct);
            }
            else
            {
                var normalized = municipality.Trim();
                services = await _serviceRepository.GetByMunicipalityAsync(normalized, lang, ct);
            }

            var result = services
                .Select(s => new ServiceCardDto
                {
                    Identifier = s.Identifier,
                    Name = s.Name,
                    Address = s.Address,
                    Typology = s.Typology,
                    PrimaryImage = s.PrimaryImage,
                    MunicipalityName = s.MunicipalityData?.Name ?? string.Empty,
                    MunicipalityLogo = s.MunicipalityData?.LogoPath ?? string.Empty,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude
                })
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Get service POI detail by identifier.
        /// </summary>
        [HttpGet("detail/{identifier}")]
        public async Task<ActionResult<ServiceDetailDto>> GetDetail(
            [FromRoute] string identifier,
            [FromQuery] string? language,
            CancellationToken ct
        )
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return BadRequest("Identifier is required.");
            }

            var service = await _serviceRepository.GetByIdentifierAsync(
                identifier.Trim(),
                language,
                ct
            );
            if (service == null)
            {
                return NotFound(new { error = "Service not found." });
            }

            var dto = new ServiceDetailDto
            {
                Identifier = service.Identifier,
                Name = service.Name,
                Address = service.Address,
                Description = service.Description,
                Typology = service.Typology,
                SpacesForDisabled = service.SpacesForDisabled,
                PayingParkingSpaces = service.PayingParkingSpaces,
                AvailableParkingSpaces = service.AvailableParkingSpaces,
                PostiAutoVenduti = service.PostiAutoVenduti,
                TotalNumberOfCarParkSpaces = service.TotalNumberOfCarParkSpaces,
                Latitude = service.Latitude,
                Longitude = service.Longitude,
                PrimaryImage = service.PrimaryImage,
                Gallery = service.Gallery ?? new List<string>(),
                Email = service.Email,
                Telephone = service.Telephone,
                Website = service.Website,
                Instagram = service.Instagram,
                Facebook = service.Facebook,
                Price = service.Price,
                ReservationUrl = service.ReservationUrl,
                Neighbors = service.Neighbors ?? new List<Models.Entities.NeighborSummary>(),
                NearestCarPark = service.NearestCarPark,
                OpeningHours = service.OpeningHours,
                TemporaryClosure = service.TemporaryClosure,
                Booking = service.Booking,
                Locations = service.Locations ?? new List<Models.Entities.LocationSummary>(),
                AssociatedServices =
                    service.AssociatedServices ?? new List<Models.Entities.AssociatedServiceSummary>(),
                MunicipalityData = service.MunicipalityData ?? new Models.Entities.MunicipalityInfo(),
                SourceLastUpdated = service.SourceLastUpdated,
                Language = service.Language
            };

            return Ok(dto);
        }
    }
}
