using MassTransit;
using Microsoft.EntityFrameworkCore;
using OmniCommerce.Contracts;
using OrderService.Data;
using OrderService.Entities;

namespace OrderService.Consumers
{
    public class PaymentSucceededConsumer : IConsumer<PaymentSucceeded>
    {
        private readonly OrderDbContext _db;
        public PaymentSucceededConsumer(OrderDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<PaymentSucceeded> context)
        {
            var msg = context.Message;

            var order = await _db.Orders.FirstOrDefaultAsync(x => x.Id == msg.OrderId);
            if (order == null) return;

            // Zaten Paid ise tekrar yazma
            if (order.Status == OrderStatus.Paid) return;

            order.Status = OrderStatus.Paid;
            await _db.SaveChangesAsync();

            Console.WriteLine($"[OrderService] Order Paid: {msg.OrderId}");
        }
    }
}