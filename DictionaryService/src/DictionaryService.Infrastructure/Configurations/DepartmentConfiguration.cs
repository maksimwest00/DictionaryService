using DictionaryService.Domain;
using DictionaryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DictionaryService.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id)
            .HasName("pk_departments");

        builder.Property(d => d.Id)
            .IsRequired()
            .HasColumnName("id");

        builder.ComplexProperty(d => d.Name, nb =>
        {
            nb.Property(t => t.Value)
                .HasMaxLength(Name.NAME_MAX_LENGTH)
                .HasColumnName("name")
                .IsRequired();
        });

        builder.ComplexProperty(d => d.Identifier, ib =>
        {
            ib.Property(t => t.Value)
                .HasMaxLength(Identifier.IDENTIFIER_MAX_LENGTH)
                .HasColumnName("identifier")
                .IsRequired();
        });

        builder.ComplexProperty(d => d.Path, pb =>
        {
            pb.Property(t => t.Value)
                .HasColumnName("path")
                .IsRequired();
        });

        builder.Property(d => d.Depth)
            .IsRequired()
            .HasColumnName("depth");

        builder.Property(x => x.ChildrenCount)
            .IsRequired()
            .HasColumnName("children_count");

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasColumnName("isActive");

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.Property(d => d.ParentId)
            .IsRequired(false)
            .HasColumnName("parent_id");

        builder.HasMany(d => d.Children)
            .WithOne(x => x.Parent)
            .IsRequired(false)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.DepartmentLocations)
            .WithOne()
            .HasForeignKey(d => d.DepartmentId);

        builder.HasMany(d => d.DepartmentPositions)
            .WithOne()
            .HasForeignKey(d => d.DepartmentId);
    }
}