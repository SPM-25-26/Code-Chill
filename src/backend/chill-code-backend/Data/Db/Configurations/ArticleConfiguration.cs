using System.Text.Json;
using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder.HasKey(a => a.Identifier);
            builder.Property(a => a.Title).IsRequired();
            builder.Property(a => a.Script).IsRequired();
            builder.Property(a => a.ImagePath).IsRequired();

            builder.Property(a => a.CreatedAt).IsRequired();
            builder.Property(a => a.UpdatedAt).IsRequired();
            builder.Property(a => a.SourceLastUpdated);

            builder.Property(a => a.Region).HasMaxLength(200);
            builder.Property(a => a.Subtitle).HasMaxLength(500);
            builder.Property(a => a.TimeToRead).HasMaxLength(100);

            builder
                .Property(a => a.Themes)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, SerializerOptions),
                    v =>
                        string.IsNullOrWhiteSpace(v)
                            ? new List<string>()
                            : JsonSerializer.Deserialize<List<string>>(v, SerializerOptions)
                                ?? new List<string>()
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<List<string>>(
                        (c1, c2) => (c1 ?? new()).SequenceEqual(c2 ?? new()),
                        c =>
                            (c ?? new()).Aggregate(
                                0,
                                (a, v) => HashCode.Combine(a, v.GetHashCode())
                            ),
                        c => c == null ? new List<string>() : c.ToList()
                    )
                );

            builder
                .Property(a => a.Paragraphs)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, SerializerOptions),
                    v =>
                        string.IsNullOrWhiteSpace(v)
                            ? new List<ArticleParagraph>()
                            : JsonSerializer.Deserialize<List<ArticleParagraph>>(
                                v,
                                SerializerOptions
                            ) ?? new List<ArticleParagraph>()
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<List<ArticleParagraph>>(
                        (c1, c2) =>
                            JsonSerializer.Serialize(c1, SerializerOptions)
                            == JsonSerializer.Serialize(c2, SerializerOptions),
                        c => JsonSerializer.Serialize(c, SerializerOptions).GetHashCode(),
                        c =>
                            c == null
                                ? new List<ArticleParagraph>()
                                : JsonSerializer.Deserialize<List<ArticleParagraph>>(
                                    JsonSerializer.Serialize(c, SerializerOptions),
                                    SerializerOptions
                                ) ?? new List<ArticleParagraph>()
                    )
                );

            builder.OwnsOne(
                a => a.MunicipalityData,
                nav =>
                {
                    nav.Property(m => m.Name).HasMaxLength(200);
                    nav.Property(m => m.LogoPath).HasMaxLength(500);
                }
            );
        }
    }
}
