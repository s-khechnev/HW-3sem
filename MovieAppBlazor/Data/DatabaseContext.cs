using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MovieAppBlazor.Data;

public class DatabaseContext : DbContext
{
    public DbSet<Person?> Persons { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Tag?> Tags { get; set; }
    public DbSet<Title> Titles { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Top)
            .WithMany();
    }
}