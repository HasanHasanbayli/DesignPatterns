using Microsoft.EntityFrameworkCore;

namespace Stock.API.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Models.Stock> Stocks { get; set; }
}