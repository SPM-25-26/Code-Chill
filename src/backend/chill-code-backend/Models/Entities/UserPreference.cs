namespace ChillCodeBackend.Models.Entities
{
    public class UserPreference
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string TravelVibe { get; set; } = string.Empty; // chill | adventure
        public string ActivityLevel { get; set; } = string.Empty; // low | medium | high
        public string CitySize { get; set; } = string.Empty; // small | big
        public List<string> Interests { get; set; } = new();
        public bool? FamilyFriendly { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
