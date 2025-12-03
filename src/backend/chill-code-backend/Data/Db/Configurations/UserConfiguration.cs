using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email).IsRequired();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.ChangedAt).IsRequired();
        builder.Property(u => u.EmailVerificationToken).HasMaxLength(200);
        builder.Property(u => u.EmailVerificationExpiresAt);
    }
}
}
