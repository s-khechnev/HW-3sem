using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;

namespace BigDataApp;

public sealed class DataContext : DbContext
{
    public DbSet<Person> Persons { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Tag> Tags { get; set; }


    public DataContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.UseNpgsql("server=localhost;Port=5432;database=bigDataAppDB;userId=test;password=12345");

        var conn = new NpgsqlConnection(
            "server=localhost;Port=5432;database=bigDataAppDB;userId=test;password=12345;Pooling=false;Timeout=300;CommandTimeout=300");
        optionsBuilder.UseNpgsql(conn);
            //.LogTo(Console.WriteLine);

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Movie>()
            .HasMany(m => m.Top)
            .WithMany();
    }
}