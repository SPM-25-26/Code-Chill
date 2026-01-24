using System.Text.Json;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.ToTable("Services");
            builder.HasKey(s => new { s.Identifier, s.Language });

            builder.Property(s => s.Identifier).IsRequired();
            builder.Property(s => s.Language).IsRequired().HasDefaultValue("it");
            builder.Property(s => s.CreatedAt).IsRequired();
            builder.Property(s => s.UpdatedAt).IsRequired();

            // JSON columns for complex types
            builder
                .Property(s => s.Gallery)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                        ?? new List<string>()
                );

            builder
                .Property(s => s.Neighbors)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v =>
                        JsonSerializer.Deserialize<List<NeighborSummary>>(
                            v,
                            (JsonSerializerOptions?)null
                        ) ?? new List<NeighborSummary>()
                );

            builder
                .Property(s => s.Locations)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v =>
                        JsonSerializer.Deserialize<List<LocationSummary>>(
                            v,
                            (JsonSerializerOptions?)null
                        ) ?? new List<LocationSummary>()
                );

            builder
                .Property(s => s.AssociatedServices)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v =>
                        JsonSerializer.Deserialize<List<AssociatedServiceSummary>>(
                            v,
                            (JsonSerializerOptions?)null
                        ) ?? new List<AssociatedServiceSummary>()
                );

            builder.OwnsOne(s => s.NearestCarPark);
            builder.OwnsOne(s => s.MunicipalityData);
            builder.OwnsOne(s => s.OpeningHours, oh =>
            {
                oh.OwnsOne(x => x.AdmissionType);
                oh.OwnsOne(x => x.TimeInterval);
            });
            builder.OwnsOne(s => s.TemporaryClosure, tc =>
            {
                tc.OwnsOne(x => x.TimeInterval);
            });
            builder.OwnsOne(s => s.Booking, b =>
            {
                b.OwnsOne(x => x.TimeInterval);
            });

            // Indexes
            builder.HasIndex(s => new { s.Identifier, s.Language }).IsUnique();
        }
    }
}
