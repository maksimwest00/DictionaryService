using DictionaryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DictionaryService.Infrastructure.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");

        builder.HasKey(dp => dp.Id)
            .HasName("pk_department_positions");

        builder.Property(dp => dp.Id)
            .IsRequired()
            .HasColumnName("id");

        builder.Property(dp => dp.DepartmentId)
            .IsRequired()
            .HasColumnName("department_id");

        builder.Property(dp => dp.PositionId)
            .IsRequired()
            .HasColumnName("position_id");

        builder.HasOne(dp => dp.Department)
            .WithMany(dp => dp.DepartmentPositions)
            .HasForeignKey(dp => dp.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(dp => dp.Position)
            .WithMany(dp => dp.DepartmentPositions)
            .HasForeignKey(dp => dp.PositionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}