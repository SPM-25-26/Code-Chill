namespace ChillCodeBackend.Models.Entities
{
    public class Municipality
    {
        public string Name { get; set; } = string.Empty;

        public string LegalName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public Contact Contacts { get; set; } = new Contact();

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string LogoPath { get; set; } = string.Empty;

        public List<string>? HomeImages { get; set; }

        public List<MunicipalityRelatedEntity>? Events { get; set; }
        public List<MunicipalityRelatedEntity>? ArticlesAndPaths { get; set; }

        public string PanoramaPath { get; set; } = string.Empty;
        public int? PanoramaWidth { get; set; }

        public List<string>? VirtualTourUrls { get; set; }

        public string NameAndProvince { get; set; } = string.Empty;
    }

    public record Contact
    {
        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Facebook { get; set; } = string.Empty;
        public string Instagram { get; set; } = string.Empty;
    }

    public class MunicipalityRelatedEntity
    {
        public string EntityId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public MobileCategory Category { get; set; }

        public string ImagePath { get; set; } = string.Empty;
        public string ExtraInfo { get; set; } = string.Empty;
    }

    public class MunicipalityInfo
    {
        public string Name { get; set; } = string.Empty;
        public string LogoPath { get; set; } = string.Empty;
    }

    public enum MobileCategory
    {
        Sleep,
        EatAndDrink,
        Events,
        ArtCulture,
        Nature,
        TypicalProducts,
        Routes,
        Services,
        EntertainmentLeisure,
        Organizations,
        Articles,
        Shopping,
    }
}
