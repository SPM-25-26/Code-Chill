using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class RouteConfiguration : IEntityTypeConfiguration<Models.Entities.Route>
    {
        public void Configure(EntityTypeBuilder<Models.Entities.Route> builder)
        {
            builder.Property(m => m.ImagePath).IsRequired();
            builder.Property(m => m.Number).IsRequired();
            builder.Property(m => m.Name).IsRequired();
            builder.Property(m => m.Description).IsRequired();
            builder.Property(m => m.PathTheme).IsRequired();
            builder.Property(m => m.TravellingMethod).IsRequired();
            builder.Property(m => m.ShortName).IsRequired();
            builder.Property(m => m.Gallery).IsRequired();
            builder.Property(m => m.VirtualTours).IsRequired();
            builder.Property(m => m.Stages).IsRequired();
            builder.Property(m => m.StagesPoi).IsRequired();
            builder.Property(m => m.StartingPoint).IsRequired();
            builder.Property(m => m.BestWhen).IsRequired();
            builder.Property(m => m.MunicipalityData).IsRequired();

        }
    }
}