using System.Text.Json;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class IngestionRunConfiguration : IEntityTypeConfiguration<IngestionRun>
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public void Configure(EntityTypeBuilder<IngestionRun> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Municipality).IsRequired().HasMaxLength(200);
            builder.Property(r => r.Language).IsRequired().HasMaxLength(10);

            builder.Property(r => r.StartedAt).IsRequired();
            builder.Property(r => r.CompletedAt);

            builder.HasIndex(r => new { r.Municipality, r.Language, r.StartedAt });

            builder
                .Property(r => r.Errors)
                .HasConversion(
                    v => JsonSerializer.Serialize(v ?? new List<string>(), SerializerOptions),
                    v =>
                        string.IsNullOrWhiteSpace(v)
                            ? new List<string>()
                            : JsonSerializer.Deserialize<List<string>>(v, SerializerOptions)
                                ?? new List<string>()
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<List<string>>(
                        (c1, c2) =>
                            JsonSerializer.Serialize(c1 ?? new List<string>(), SerializerOptions)
                            == JsonSerializer.Serialize(c2 ?? new List<string>(), SerializerOptions),
                        c =>
                            JsonSerializer
                                .Serialize(c ?? new List<string>(), SerializerOptions)
                                .GetHashCode(),
                        c =>
                            c == null
                                ? new List<string>()
                                : JsonSerializer.Deserialize<List<string>>(
                                    JsonSerializer.Serialize(c, SerializerOptions),
                                    SerializerOptions
                                ) ?? new List<string>()
                    )
                );
        }
    }
}

