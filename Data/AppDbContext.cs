using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Sale> Sales => Set<Sale>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Sale>().HasIndex(s => s.StoreCode);
        modelBuilder.Entity<Sale>().HasIndex(s => s.ProductCode);
        modelBuilder.Entity<Sale>().HasIndex(s => s.SaleDate);
        modelBuilder.Entity<Sale>().HasIndex(s => s.Quantity);
    }
}
