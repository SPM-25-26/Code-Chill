namespace ChillCodeBackend.Models.Entities
{
    public class Route
    {
        public string ImagePath { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PathTheme { get; set; } = string.Empty;
        public string TravellingMethod { get; set; } = string.Empty; 
        public string ShortName { get; set; } = string.Empty;
        public string OrganizationWebsite { get; set; } = string.Empty;
        public string OrganizationEmail { get; set; } = string.Empty;
        public string OrganizationFacebook { get; set; } = string.Empty;
        public string OrganizationInstragram { get; set; } = string.Empty;
        public string OrganizationTelephone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;

        public string Duration { get; set; } = string.Empty;
        public string RouteLength { get; set; } = string.Empty;

        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Municipality MunicipalityData { get; set; } = new Municipality();
    }
}