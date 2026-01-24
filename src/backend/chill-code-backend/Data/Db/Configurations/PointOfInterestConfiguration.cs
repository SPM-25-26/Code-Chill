using System.Text.Json;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class PointOfInterestConfiguration : IEntityTypeConfiguration<PointOfInterest>
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public void Configure(EntityTypeBuilder<PointOfInterest> builder)
        {
            builder.HasKey(p => new { p.Identifier, p.Language });
            builder.Property(p => p.Title).IsRequired();
            builder.Property(p => p.Category).IsRequired();
            builder.Property(p => p.PrimaryImage).IsRequired();

            builder.Property(p => p.Language).IsRequired().HasMaxLength(10);

            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.UpdatedAt).IsRequired();
            builder.Property(p => p.SourceLastUpdated);

            builder.Property(p => p.OfficialName).HasMaxLength(500);
            builder.Property(p => p.Address).HasMaxLength(500);
            builder.Property(p => p.Typology).HasMaxLength(200);
            builder.Property(p => p.Email).HasMaxLength(200);
            builder.Property(p => p.Telephone).HasMaxLength(100);
            builder.Property(p => p.Website).HasMaxLength(500);
            builder.Property(p => p.Facebook).HasMaxLength(200);
            builder.Property(p => p.Instagram).HasMaxLength(200);
            builder.Property(p => p.Accessibility).HasMaxLength(1000);
            builder.Property(p => p.OrganizationTaxCode).HasMaxLength(200);
            builder.Property(p => p.OrganizationLegalName).HasMaxLength(500);

            ConfigureList(builder, p => p.Gallery);
            ConfigureList(builder, p => p.VirtualTours);
            ConfigureList(builder, p => p.Neighbors);
            ConfigureList(builder, p => p.OpeningHours);
            ConfigureList(builder, p => p.Prices);
            ConfigureList(builder, p => p.Services);
            ConfigureList(builder, p => p.Facilities);

            builder.OwnsOne(
                p => p.NearestCarPark,
                nav =>
                {
                    nav.Property(c => c.Address).HasMaxLength(500);
                }
            );

            builder.OwnsOne(
                p => p.MunicipalityData,
                nav =>
                {
                    nav.Property(m => m.Name).HasMaxLength(200);
                    nav.Property(m => m.LogoPath).HasMaxLength(500);
                }
            );
        }

        private static void ConfigureList<T>(
            EntityTypeBuilder<PointOfInterest> builder,
            System.Linq.Expressions.Expression<Func<PointOfInterest, List<T>>> propertyExpression
        )
        {
            builder
                .Property(propertyExpression)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, SerializerOptions),
                    v =>
                        string.IsNullOrWhiteSpace(v)
                            ? new List<T>()
                            : JsonSerializer.Deserialize<List<T>>(v, SerializerOptions)
                                ?? new List<T>()
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<List<T>>(
                        (c1, c2) =>
                            JsonSerializer.Serialize(c1, SerializerOptions)
                            == JsonSerializer.Serialize(c2, SerializerOptions),
                        c => JsonSerializer.Serialize(c, SerializerOptions).GetHashCode(),
                        c =>
                            c == null
                                ? new List<T>()
                                : JsonSerializer.Deserialize<List<T>>(
                                    JsonSerializer.Serialize(c, SerializerOptions),
                                    SerializerOptions
                                ) ?? new List<T>()
                    )
                );
        }
    }
}
