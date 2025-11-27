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
                .IsRequired()
                .HasMaxLength(LengthConstants.Length500)
                .HasColumnName("name");
        });

        builder.ComplexProperty(d => d.Identifier, ib =>
        {
            ib.Property(t => t.Value)
                .IsRequired()
                .HasMaxLength(LengthConstants.Length500)
                .HasColumnName("identifier");
        });

        builder.ComplexProperty(d => d.Path, pb =>
        {
            pb.Property(t => t.Value)
                .IsRequired()
                .HasMaxLength(LengthConstants.Length500)
                .HasColumnName("path");
        });

        builder.Property(d => d.Depth)
            .IsRequired()
            .HasColumnName("depth");

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasColumnName("isActive");

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.HasOne(d => d.Parent)
            .WithMany(d => d.Children)
            .HasForeignKey(d => d.ParentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(d => d.ParentId)
            .IsRequired(false)
            .HasColumnName("parent_id");

        builder.HasMany(d => d.Children)
            .WithOne()
            .HasForeignKey("departments_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}