using DictionaryService.Domain;
using DictionaryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DictionaryService.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.Id)
            .HasName("pk_locations");

        builder.Property(l => l.Id)
            .IsRequired()
            .HasColumnName("id");

        builder.ComplexProperty(d => d.Name, nb =>
        {
            nb.Property(n => n.Value)
                .IsRequired()
                .HasMaxLength(LengthConstants.Length500)
                .HasColumnName("name");
        });

        builder.ComplexProperty(l => l.Address, ab =>
        {
            ab.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(LengthConstants.Length500)
                .HasColumnName("city");

            ab.Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(LengthConstants.Length500)
                .HasColumnName("street");

            ab.Property(a => a.Building)
                .IsRequired()
                .HasMaxLength(LengthConstants.Length500)
                .HasColumnName("building");

            ab.Property(a => a.RoomNumber)
                .IsRequired()
                .HasMaxLength(LengthConstants.Length500)
                .HasColumnName("roomNumber");
        });

        builder.Property(l => l.Timezone)
            .IsRequired()
            .HasColumnName("timezone");

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(l => l.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");
    }
}