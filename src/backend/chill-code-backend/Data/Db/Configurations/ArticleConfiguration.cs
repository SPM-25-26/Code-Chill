using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder.HasKey(m => m.Identifier);
            builder.Property(m => m.Title).IsRequired();
            builder.Property(m => m.Script).IsRequired();
            builder.Property(m => m.ImagePath).IsRequired();
            builder.Property(m => m.Paragraphs).IsRequired();
            builder.Property(m => m.UpdateAt).IsRequired();
            builder.Property(m => m.Themes).IsRequired();
            builder.Property(m => m.MunicipalityData).IsRequired();
        }
    }
}