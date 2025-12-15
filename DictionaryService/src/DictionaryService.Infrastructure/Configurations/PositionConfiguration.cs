using DictionaryService.Domain;
using DictionaryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DictionaryService.Infrastructure.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");

        builder.HasKey(p => p.Id)
            .HasName("pk_positions");

        builder.Property(p => p.Id)
            .IsRequired()
            .HasColumnName("id");

        builder.ComplexProperty(p => p.Name, nb =>
        {
            nb.Property(t => t.Value)
                .IsRequired()
                .HasMaxLength(Name.MAX_NAME_LENGTH)
                .HasColumnName("name");
        });

        builder.ComplexProperty(p => p.Description, db =>
        {
            db.Property(t => t.Value)
                .IsRequired(false)
                .HasMaxLength(Description.MAX_DESCRIPTION_LENGTH)
                .HasColumnName("description");
        });

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.HasMany(p => p.DepartmentPositions)
            .WithOne()
            .HasForeignKey(dp => dp.PositionId);
    }
}