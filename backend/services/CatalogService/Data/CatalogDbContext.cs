using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Data
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }
        public DbSet<ReceivedOrderEntity> ReceivedOrders => Set<ReceivedOrderEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReceivedOrderEntity>(entity =>
            {
                entity.ToTable("received_orders");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.CustomerId).IsRequired();
                entity.Property(x => x.Currency).IsRequired();

                // idempotency için (aynı OrderId iki kere gelmesin)
                entity.HasIndex(x => x.OrderId).IsUnique();
            });
        }
    }
}
