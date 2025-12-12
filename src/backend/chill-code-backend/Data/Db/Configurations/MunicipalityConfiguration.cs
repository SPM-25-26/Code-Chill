using System.Text.Json;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class MunicipalityConfiguration : IEntityTypeConfiguration<Municipality>
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public void Configure(EntityTypeBuilder<Municipality> builder)
        {
            builder.HasKey(m => m.Name);
            builder.HasIndex(m => m.Name).IsUnique();
            builder.Property(m => m.LegalName).IsRequired();
            builder.Property(m => m.Description).IsRequired();
            builder.Property(m => m.Latitude).IsRequired();
            builder.Property(m => m.Longitude).IsRequired();
            builder.Property(m => m.LogoPath).IsRequired();
            builder.Property(m => m.NameAndProvince).HasMaxLength(400);

            ConfigureList(builder, m => m.HomeImages);
            ConfigureList(builder, m => m.Events);
            ConfigureList(builder, m => m.ArticlesAndPaths);
            ConfigureList(builder, m => m.VirtualTourUrls);

            builder.OwnsOne(
                m => m.Contacts,
                nav =>
                {
                    nav.Property(c => c.Email).HasMaxLength(200);
                    nav.Property(c => c.Telephone).HasMaxLength(100);
                    nav.Property(c => c.Website).HasMaxLength(500);
                    nav.Property(c => c.Facebook).HasMaxLength(200);
                    nav.Property(c => c.Instagram).HasMaxLength(200);
                }
            );
        }

        private static void ConfigureList<T>(
            EntityTypeBuilder<Municipality> builder,
            System.Linq.Expressions.Expression<Func<Municipality, List<T>?>> propertyExpression
        )
        {
            builder
                .Property(propertyExpression)
                .HasConversion(
                    v => JsonSerializer.Serialize(v ?? new List<T>(), SerializerOptions),
                    v =>
                        string.IsNullOrWhiteSpace(v)
                            ? new List<T>()
                            : JsonSerializer.Deserialize<List<T>>(v, SerializerOptions)
                                ?? new List<T>()
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<List<T>?>(
                        (c1, c2) =>
                            JsonSerializer.Serialize(c1 ?? new List<T>(), SerializerOptions)
                            == JsonSerializer.Serialize(c2 ?? new List<T>(), SerializerOptions),
                        c =>
                            JsonSerializer
                                .Serialize(c ?? new List<T>(), SerializerOptions)
                                .GetHashCode(),
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
