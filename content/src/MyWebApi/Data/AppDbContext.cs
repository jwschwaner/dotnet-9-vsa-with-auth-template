namespace MyWebApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Add your DbSets here
    // public DbSet<YourEntity> YourEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure your entities here
        base.OnModelCreating(modelBuilder);
    }
}