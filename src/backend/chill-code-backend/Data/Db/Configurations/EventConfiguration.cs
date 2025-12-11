using ChillCodeBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChillCodeBackend.Data.Db.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.HasKey(e => e.Identifier);
            builder.Property(e => e.Title).IsRequired();
            builder.Property(e => e.Typology).IsRequired();
            builder.Property(e => e.PrimaryImage).IsRequired();
            builder.Property(e => e.Gallery).IsRequired();
            builder.Property(e => e.VirtualTours).IsRequired();
            builder.Property(e => e.Latitude).IsRequired();
            builder.Property(e => e.Longitude).IsRequired();
            builder.Property(e => e.Organizer).IsRequired();
            builder.Property(e => e.TicketsAndCosts).IsRequired();
            builder.Property(e => e.MunicipalityData).IsRequired();
        }
    }
}