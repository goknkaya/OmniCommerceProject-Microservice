using MassTransit;
using Microsoft.EntityFrameworkCore;
using OmniCommerce.Contracts;
using PaymentService.Data;
using PaymentService.Entities;

namespace PaymentService.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly PaymentDbContext _db;

    public OrderCreatedConsumer(PaymentDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var msg = context.Message;

        // (idempotency) aynı event ikinci kez gelirse DB unique index patlamasın diye önce kontrol
        var exists = await _db.Payments.AnyAsync(x => x.OrderId == msg.OrderId);
        if (exists) return;

        var shouldFail = msg.CustomerId?.ToLowerInvariant().Contains("fail") == true;

        var payment = new PaymentEntity
        {
            Id = Guid.NewGuid(),
            OrderId = msg.OrderId,
            CustomerId = msg.CustomerId,
            Amount = msg.Amount,
            Currency = msg.Currency,
            Success = !shouldFail,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        if (shouldFail)
        {
            await context.Publish(new PaymentFailed(
                OrderId: msg.OrderId,
                Reason: "Mock fail rule: customerId contains 'fail'",
                FailedAt: DateTimeOffset.UtcNow
                ));

            Console.WriteLine($"[PaymentService] Payment FAILED published: {msg.OrderId}");
            return;
        }

        await context.Publish(new PaymentSucceeded(
            PaymentId: payment.Id,
            OrderId: payment.OrderId,
            CustomerId: payment.CustomerId,
            Amount: payment.Amount,
            Currency: payment.Currency,
            PaidAt: DateTimeOffset.UtcNow
        ));

        Console.WriteLine($"[PaymentService] Payment saved & PaymentSucceeded published: {payment.OrderId}");
    }
}
