using BigDataApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;

namespace BigDataApp;

public class DataContext : DbContext
{
    public virtual DbSet<Person> Persons { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Tag> Tags { get; set; }

    public DataContext()
    {
        base.ChangeTracker.AutoDetectChangesEnabled = false;
        base.ChangeTracker.LazyLoadingEnabled = false;
        base.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("server=localhost;Port=5432;database=bigDataAppDB;userId=test;password=12345");

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        /*modelBuilder.Entity<Movie>()
            .HasMany(m => m.Top)
            .WithMany();*/
    }
}
