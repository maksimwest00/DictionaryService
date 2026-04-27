using DictionaryService.Domain.DepartmentLocations;
using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Locations;
using DictionaryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace DictionaryService.Infrastructure;

public class DictionaryServiceDbContext : DbContext
{
    private readonly string _connectionString;

    public DictionaryServiceDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<DepartmentLocation> DepartmentLocations => Set<DepartmentLocation>();

    public DbSet<Location> Locations => Set<Location>();

    public DbSet<Position> Positions => Set<Position>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(_connectionString);
        optionsBuilder.LogTo(Console.WriteLine);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("ltree");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DictionaryServiceDbContext).Assembly);
    }
}