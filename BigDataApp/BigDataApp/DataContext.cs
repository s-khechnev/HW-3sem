using Microsoft.EntityFrameworkCore;

namespace BigDataApp;

public sealed class DataContext:DbContext
{
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Movie> Movies { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    
    public DataContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("server=localhost;Port=5432;database=bigDataAppDB;userId=test;password=12345");
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Actor>();
        builder.Entity<Director>();

        base.OnModelCreating(builder);
    }
}