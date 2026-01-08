using System.Text.Json;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class RouteConfiguration : IEntityTypeConfiguration<Models.Entities.Route>
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public void Configure(EntityTypeBuilder<Models.Entities.Route> builder)
        {
            builder.HasKey(r => r.Name);

            builder.Property(r => r.ExternalId).IsRequired();
            builder.HasIndex(r => r.ExternalId).IsUnique();

            builder.Property(r => r.Language).IsRequired().HasMaxLength(10);

            builder.Property(r => r.CreatedAt).IsRequired();
            builder.Property(r => r.UpdatedAt).IsRequired();
            builder.Property(r => r.SourceLastUpdated);

            builder.Property(r => r.ImagePath).IsRequired();
            builder.Property(r => r.Number).IsRequired();
            builder.Property(r => r.Name).IsRequired();
            builder.Property(r => r.Description).IsRequired();
            builder.Property(r => r.PathTheme).IsRequired();
            builder.Property(r => r.TravellingMethod).IsRequired();
            builder.Property(r => r.ShortName).IsRequired();

            builder.Property(r => r.OrganizationWebsite).HasMaxLength(500);
            builder.Property(r => r.OrganizationEmail).HasMaxLength(200);
            builder.Property(r => r.OrganizationFacebook).HasMaxLength(200);
            builder.Property(r => r.OrganizationInstagram).HasMaxLength(200);
            builder.Property(r => r.OrganizationTelephone).HasMaxLength(100);
            builder.Property(r => r.Website).HasMaxLength(500);
            builder.Property(r => r.SecurityLevel).HasMaxLength(100);
            builder.Property(r => r.Duration).HasMaxLength(200);
            builder.Property(r => r.RouteLength).HasMaxLength(200);

            ConfigureList(builder, r => r.Gallery);
            ConfigureList(builder, r => r.VirtualTours);
            ConfigureList(builder, r => r.Stages);
            ConfigureList(builder, r => r.StagesPoi);
            ConfigureList(builder, r => r.BestWhen);

            builder.OwnsOne(
                r => r.StartingPoint,
                nav =>
                {
                    nav.Property(p => p.Address).HasMaxLength(500);
                }
            );

            builder.OwnsOne(
                r => r.MunicipalityData,
                nav =>
                {
                    nav.Property(m => m.Name).HasMaxLength(200);
                    nav.Property(m => m.LogoPath).HasMaxLength(500);
                }
            );
        }

        private static void ConfigureList<T>(
            EntityTypeBuilder<Models.Entities.Route> builder,
            System.Linq.Expressions.Expression<
                Func<Models.Entities.Route, List<T>>
            > propertyExpression
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
