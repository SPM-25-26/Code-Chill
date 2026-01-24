using System;
using ChillCodeBackend.Models.Entities;
using ChillCodeBackend.Models.Unicam;

namespace ChillCodeBackend.Services.Ingestion.Mappers
{
    /// <summary>
    /// Maps Unicam municipality visit payloads into Municipality entities.
    /// </summary>
    public class MunicipalityMapper
    {
        public Municipality Map(string externalId, UnicamMunicipalityVisit visit, string language)
        {
            var name = !string.IsNullOrWhiteSpace(visit.Name)
                ? visit.Name!
                : visit.LegalName ?? string.Empty;

            return new Municipality
            {
                ExternalId = externalId,
                Language = string.IsNullOrWhiteSpace(language) ? "it" : language,
                Name = name,
                LegalName = visit.LegalName ?? string.Empty,
                Description = visit.Description ?? string.Empty,
                Contacts = new Contact
                {
                    Email = visit.Contacts?.Email ?? string.Empty,
                    Telephone = visit.Contacts?.Telephone ?? string.Empty,
                    Website = visit.Contacts?.Website ?? string.Empty,
                    Facebook = visit.Contacts?.Facebook ?? string.Empty,
                    Instagram = visit.Contacts?.Instagram ?? string.Empty,
                },
                Latitude = visit.Latitude,
                Longitude = visit.Longitude,
                LogoPath = visit.LogoPath ?? string.Empty,
                HomeImages = visit.HomeImages ?? new List<string>(),
                Events = MapRelatedEntities(visit.Events),
                ArticlesAndPaths = MapRelatedEntities(visit.ArticlesAndPaths),
                PanoramaPath = visit.PanoramaPath ?? string.Empty,
                PanoramaWidth = visit.PanoramaWidth,
                VirtualTourUrls = visit.VirtualTourUrls ?? new List<string>(),
                NameAndProvince = visit.NameAndProvince ?? string.Empty,
            };
        }

        private static List<MunicipalityRelatedEntity> MapRelatedEntities(
            IEnumerable<UnicamMunicipalityVisitHighlight> items
        )
        {
            var result = new List<MunicipalityRelatedEntity>();
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.EntityId))
                {
                    continue;
                }

                result.Add(
                    new MunicipalityRelatedEntity
                    {
                        EntityId = item.EntityId!,
                        Title = item.Title ?? string.Empty,
                        Category = MapCategory(item.Category),
                        ImagePath = item.ImagePath ?? string.Empty,
                        ExtraInfo = item.ExtraInfo ?? string.Empty,
                    }
                );
            }

            return result;
        }

        private static MobileCategory MapCategory(string? category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return MobileCategory.Articles;
            }

            return Enum.TryParse<MobileCategory>(category, true, out var parsed)
                ? parsed
                : MobileCategory.Articles;
        }
    }
}
