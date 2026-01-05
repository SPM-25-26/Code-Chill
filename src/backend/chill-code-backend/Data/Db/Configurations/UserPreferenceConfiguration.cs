using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
    {
        public void Configure(EntityTypeBuilder<UserPreference> builder)
        {
            builder.HasKey(p => p.Id);
            builder.HasIndex(p => p.UserId).IsUnique();

            builder.Property(p => p.TravelVibe).IsRequired().HasMaxLength(32);
            builder.Property(p => p.ActivityLevel).IsRequired().HasMaxLength(32);
            builder.Property(p => p.CitySize).IsRequired().HasMaxLength(32);
            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.UpdatedAt).IsRequired();

            builder
                .Property(p => p.Interests)
                .HasConversion(
                    v => string.Join(',', v),
                    v =>
                        string.IsNullOrWhiteSpace(v)
                            ? new List<string>()
                            : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(i => i.Trim())
                                .Where(i => !string.IsNullOrWhiteSpace(i))
                                .ToList()
                );
        }
    }
}
