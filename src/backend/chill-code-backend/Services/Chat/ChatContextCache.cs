using Microsoft.Extensions.Caching.Memory;

namespace ChillCodeBackend.Services.Chat
{
    public class ChatContextCache
    {
        private readonly IMemoryCache _cache;

        public ChatContextCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<IReadOnlyList<TourismContextItem>> GetOrAddAsync(
            Guid userId,
            ChatContextRequest request,
            Func<CancellationToken, Task<IReadOnlyList<TourismContextItem>>> factory,
            CancellationToken cancellationToken
        )
        {
            var cacheKey = $"chat-context:{userId}";
            if (_cache.TryGetValue(cacheKey, out ChatContextEntry? entry))
            {
                if (IsCompatible(entry, request))
                {
                    return entry.Items;
                }
            }

            var items = await factory(cancellationToken);
            var newEntry = new ChatContextEntry
            {
                Language = NormalizeLanguage(request.Language),
                Municipality = request.Municipality?.Trim(),
                Latitude = NormalizeCoord(request.Latitude),
                Longitude = NormalizeCoord(request.Longitude),
                Items = items.ToList()
            };

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(20),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
                Size = Math.Max(newEntry.Items.Count, 1)
            };

            _cache.Set(cacheKey, newEntry, cacheEntryOptions);
            return newEntry.Items;
        }

        private static bool IsCompatible(ChatContextEntry entry, ChatContextRequest request)
        {
            var language = NormalizeLanguage(request.Language);
            var municipality = request.Municipality?.Trim();
            var lat = NormalizeCoord(request.Latitude);
            var lng = NormalizeCoord(request.Longitude);

            return string.Equals(entry.Language, language, StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.Municipality, municipality, StringComparison.OrdinalIgnoreCase)
                && Nullable.Equals(entry.Latitude, lat)
                && Nullable.Equals(entry.Longitude, lng);
        }

        private static string NormalizeLanguage(string language)
        {
            return string.IsNullOrWhiteSpace(language) ? "it" : language.Trim();
        }

        private static double? NormalizeCoord(double? value)
        {
            return value.HasValue ? Math.Round(value.Value, 3) : null;
        }

        private class ChatContextEntry
        {
            public string Language { get; set; } = "it";
            public string? Municipality { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public List<TourismContextItem> Items { get; set; } = new();
        }
    }
}
