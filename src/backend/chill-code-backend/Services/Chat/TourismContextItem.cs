namespace ChillCodeBackend.Services.Chat
{
    public class TourismContextItem
    {
        public string Type { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Municipality { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double? DistanceKm { get; set; }
        public string? Availability { get; set; }
        public string? Notes { get; set; }
    }
}
