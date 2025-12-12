using System.Text.Json;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.HasKey(e => e.Identifier);
            builder.Property(e => e.Title).IsRequired();
            builder.Property(e => e.Typology).IsRequired();
            builder.Property(e => e.PrimaryImage).IsRequired();

            builder.Property(e => e.Address).HasMaxLength(500);
            builder.Property(e => e.Audience).HasMaxLength(200);
            builder.Property(e => e.Email).HasMaxLength(200);
            builder.Property(e => e.Telephone).HasMaxLength(100);
            builder.Property(e => e.Website).HasMaxLength(500);
            builder.Property(e => e.Facebook).HasMaxLength(200);
            builder.Property(e => e.Instagram).HasMaxLength(200);

            ConfigureList(builder, e => e.Gallery);
            ConfigureList(builder, e => e.VirtualTours);
            ConfigureList(builder, e => e.Neighbors);
            ConfigureList(builder, e => e.TicketsAndCosts);

            builder.OwnsOne(
                e => e.NearestCarPark,
                nav =>
                {
                    nav.Property(p => p.Address).HasMaxLength(500);
                }
            );

            builder.OwnsOne(
                e => e.Organizer,
                nav =>
                {
                    nav.Property(o => o.TaxCode).HasMaxLength(200);
                    nav.Property(o => o.LegalName).HasMaxLength(500);
                    nav.Property(o => o.Website).HasMaxLength(500);
                }
            );

            builder.OwnsOne(
                e => e.MunicipalityData,
                nav =>
                {
                    nav.Property(m => m.Name).HasMaxLength(200);
                    nav.Property(m => m.LogoPath).HasMaxLength(500);
                }
            );
        }

        private static void ConfigureList<T>(
            EntityTypeBuilder<Event> builder,
            System.Linq.Expressions.Expression<Func<Event, List<T>>> propertyExpression
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
