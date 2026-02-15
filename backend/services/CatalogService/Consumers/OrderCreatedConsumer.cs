using CatalogService.Data;
using CatalogService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OmniCommerce.Contracts;

namespace CatalogService.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreated>
    {
        private readonly CatalogDbContext _db;
        public OrderCreatedConsumer(CatalogDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            var msg = context.Message;

            // Aynı OrderId tekrar gelirse iki kere yazmasın kontrolü
            var exists = await _db.ReceivedOrders.AnyAsync(x => x.OrderId == msg.OrderId);
            if (exists) return;

            var entity = new ReceivedOrderEntity
            {
                Id = Guid.NewGuid(),
                OrderId = msg.OrderId,
                CustomerId = msg.CustomerId,
                Amount = msg.Amount,
                Currency = msg.Currency,
                CreatedAt = msg.CreatedAt,
                ReceivedAt = DateTimeOffset.UtcNow
            };

            try
            {
                _db.ReceivedOrders.Add(entity);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // muhtemelen unique index (OrderId) çakıştı -> aynı mesaj tekrar geldi
                return;
            }
            
            
            Console.WriteLine($"[CatalogService] Saved OrderCreated: {msg.OrderId} {msg.Amount} {msg.Currency}");
        }
    }
}
