using DictionaryService.Domain.DepartmentLocations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DictionaryService.Infrastructure.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");

        builder.HasKey(dl => dl.Id)
            .HasName("pk_department_locations");

        builder.Property(dl => dl.Id)
            .IsRequired()
            .HasColumnName("id");

        builder.Property(dl => dl.DepartmentId)
            .IsRequired()
            .HasColumnName("department_id");

        builder.Property(dl => dl.LocationId)
            .IsRequired()
            .HasColumnName("location_id");
    }
}