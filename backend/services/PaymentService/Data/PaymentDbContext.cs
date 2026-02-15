using Microsoft.EntityFrameworkCore;
using PaymentService.Entities;

namespace PaymentService.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }
        public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentEntity>(e =>
            {
                e.ToTable("payments");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.OrderId).IsUnique(); // aynı order için 2 kez ödeme yapılmasın
                e.Property(x => x.CustomerId).IsRequired();
                e.Property(x => x.Currency).IsRequired();
            });
        }
    }
}
