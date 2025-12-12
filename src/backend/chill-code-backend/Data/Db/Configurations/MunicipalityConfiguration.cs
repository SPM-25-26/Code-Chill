using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class MunicipalityConfiguration : IEntityTypeConfiguration<Municipality>
    {
        public void Configure(EntityTypeBuilder<Municipality> builder)
        {
            builder.HasKey(m => m.Name);
            builder.HasIndex(m => m.Name).IsUnique();
            builder.Property(m => m.LegalName).IsRequired();
            builder.Property(m => m.Description).IsRequired();
            builder.Property(m => m.Contacts).IsRequired();
            builder.Property(m => m.Latitude).IsRequired();
            builder.Property(m => m.Longitude).IsRequired();
            builder.Property(m => m.LogoPath).IsRequired();
            builder.Property(m => m.HomeImages).IsRequired();
            builder.Property(m => m.Events).IsRequired();
            builder.Property(m => m.ArticlesAndPaths).IsRequired();
        }
    }
}
