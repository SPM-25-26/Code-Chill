using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class PreferenceResponseConfiguration : IEntityTypeConfiguration<PreferenceResponse>
    {
        public void Configure(EntityTypeBuilder<PreferenceResponse> builder)
        {
            builder.HasKey(r => r.Id);
            builder.HasIndex(r => r.UserId);
            builder.Property(r => r.QuestionnaireVersion).IsRequired().HasMaxLength(32);
            builder.Property(r => r.AnswersJson).IsRequired();
            builder.Property(r => r.DerivedPreferencesJson).IsRequired();
            builder.Property(r => r.Skipped).HasDefaultValue(false);
            builder.Property(r => r.CreatedAt).IsRequired();
            builder.Property(r => r.UpdatedAt).IsRequired();
        }
    }
}
