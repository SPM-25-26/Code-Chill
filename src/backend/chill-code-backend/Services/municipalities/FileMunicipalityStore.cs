using ChillCodeBackend.Models.Entities;
using System.Text.Json;


namespace ChillCodeBackend.Services
{
    public class FileMunicipalityStore : IMunicipalityStore
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _fileLock = new(1,1);
        private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        public FileMunicipalityStore(IHostEnvironment env)
        {
            var dataDir = Path.Combine(env.ContentRootPath, "App_Data");
            Directory.CreateDirectory(dataDir);
            _filePath = Path.Combine(dataDir, "municipalities.json");
        }

        public async Task<Municipality?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            await _fileLock.WaitAsync(cancellationToken);
            try
            {
                var list = await ReadUnsafeAsync(cancellationToken);
                return list.FirstOrDefault(x => x.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            }
            finally { _fileLock.Release(); }
        }

        public async Task AddOrUpdateAsync(Municipality municipality, CancellationToken cancellationToken = default)
        {
            await _fileLock.WaitAsync(cancellationToken);
            try
            {
                var list = await ReadUnsafeAsync(cancellationToken);
                var index = list.FindIndex(x => x.Name.Equals(municipality.Name, System.StringComparison.OrdinalIgnoreCase));

                if (index >= 0) list[index] = municipality;
                else list.Add(municipality);

                await WriteUnsafeAsync(list, cancellationToken);
            }
            finally { _fileLock.Release(); }
        }

        public async Task<IReadOnlyCollection<Municipality>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await _fileLock.WaitAsync(cancellationToken);
            try { return await ReadUnsafeAsync(cancellationToken); }
            finally { _fileLock.Release(); }
        }

        private async Task<List<Municipality>> ReadUnsafeAsync(CancellationToken ct)
        {
            if (!File.Exists(_filePath)) return new List<Municipality>();
            await using var stream = File.OpenRead(_filePath);
            return (await JsonSerializer.DeserializeAsync<List<Municipality>>(stream, _serializerOptions, ct)) ?? new List<Municipality>();
        }

        private async Task WriteUnsafeAsync(List<Municipality> list, CancellationToken ct)
        {
            await using var stream = File.Open(_filePath, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(stream, list, _serializerOptions, ct);
        }
    }
}
