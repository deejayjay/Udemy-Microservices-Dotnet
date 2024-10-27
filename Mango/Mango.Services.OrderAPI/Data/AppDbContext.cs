using Mango.Services.OrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.OrderAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<OrderHeader> OrderHeaders { get; set; }
    public DbSet<OrderDetails> OrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderHeader>()
                    .Property(o => o.Discount)
                    .HasPrecision(18, 4);

        modelBuilder.Entity<OrderHeader>()
                    .Property(o => o.OrderTotal)
                    .HasPrecision(18, 4);

        modelBuilder.Entity<OrderDetails>()
                    .Property(o => o.Price)
                    .HasPrecision(18, 4);


        base.OnModelCreating(modelBuilder);
    }
}
