using System.Text.Json;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.ToTable("Organizations");
            builder.HasKey(o => new { o.TaxCode, o.Language });

            builder.Property(o => o.TaxCode).IsRequired();
            builder.Property(o => o.Language).IsRequired().HasDefaultValue("it");
            builder.Property(o => o.CreatedAt).IsRequired();
            builder.Property(o => o.UpdatedAt).IsRequired();

            // JSON columns for complex types
            builder
                .Property(o => o.Gallery)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)
                        ?? new List<string>()
                );

            builder
                .Property(o => o.Neighbors)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v =>
                        JsonSerializer.Deserialize<List<NeighborSummary>>(
                            v,
                            (JsonSerializerOptions?)null
                        ) ?? new List<NeighborSummary>()
                );

            builder
                .Property(o => o.OwnedPoi)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v =>
                        JsonSerializer.Deserialize<List<OwnedPoiSummary>>(
                            v,
                            (JsonSerializerOptions?)null
                        ) ?? new List<OwnedPoiSummary>()
                );

            builder
                .Property(o => o.Offers)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v =>
                        JsonSerializer.Deserialize<List<OfferInfo>>(v, (JsonSerializerOptions?)null)
                        ?? new List<OfferInfo>()
                );

            builder
                .Property(o => o.Events)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v =>
                        JsonSerializer.Deserialize<List<OrganizationEventCard>>(
                            v,
                            (JsonSerializerOptions?)null
                        ) ?? new List<OrganizationEventCard>()
                );

            builder.OwnsOne(o => o.NearestCarPark);
            builder.OwnsOne(o => o.MunicipalityData);

            // Indexes
            builder.HasIndex(o => new { o.TaxCode, o.Language }).IsUnique();
        }
    }
}
