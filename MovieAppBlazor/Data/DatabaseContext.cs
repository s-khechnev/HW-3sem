using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MovieAppBlazor.Data;

public class DatabaseContext : DbContext
{
    public DbSet<Person> Persons { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Title> Titles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connection = new NpgsqlConnection(new NpgsqlConnectionStringBuilder()
        {
            Host = "localhost",
            Port = 5432,
            Database = "bigDataAppDB",
            Username = "test",
            Password = "12345",
            Timeout = 1024
        }.ToString());

        optionsBuilder.UseNpgsql(connection);

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>()
            .HasMany(m => m.Top)
            .WithMany();
    }
}