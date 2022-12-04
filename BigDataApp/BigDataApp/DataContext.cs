using Microsoft.EntityFrameworkCore;

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
        optionsBuilder.UseNpgsql("server=localhost;Port=5432;database=bigDataAppDB;userId=test;password=12345");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Actor>();
        builder.Entity<Director>();
        
        builder.Entity<Movie>()
            .HasMany(p => p.Top)
            .WithMany();
    }
}