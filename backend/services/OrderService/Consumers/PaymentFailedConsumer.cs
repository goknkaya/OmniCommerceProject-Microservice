using MassTransit;
using Microsoft.EntityFrameworkCore;
using OmniCommerce.Contracts;
using OrderService.Data;
using OrderService.Entities;

namespace OrderService.Consumers
{
    public class PaymentFailedConsumer : IConsumer<PaymentFailed>
    {
        private readonly OrderDbContext _db;
        public PaymentFailedConsumer(OrderDbContext db) => _db = db;

        public async Task Consume(ConsumeContext<PaymentFailed> context)
        {
            var msg = context.Message;

            var order = await _db.Orders.FirstOrDefaultAsync(x => x.Id == msg.OrderId);
            if (order is null) return;

            // basit senaryo: fail gelirse Cancelled yap
            order.Status = OrderStatus.Cancelled;
            await _db.SaveChangesAsync();

            Console.WriteLine($"[OrderService] Order Cancelled (payment failed): {msg.OrderId} - {msg.Reason}");
        }
    }
}
