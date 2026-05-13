using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Sale> Sales => Set<Sale>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
