using MassTransit;
using Microsoft.EntityFrameworkCore;
using OmniCommerce.Contracts;
using OrderService.Consumers;
using OrderService.Data;
using OrderService.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB

var connStr = builder.Configuration.GetConnectionString("OrderDb");

if (string.IsNullOrWhiteSpace(connStr))
    throw new InvalidOperationException("Connection string 'OrderDb' not found.");

builder.Services.AddDbContext<OrderDbContext>(opt =>
    opt.UseNpgsql(connStr));

// MASSTRANSIT (RABBITMQ)
builder.Services.AddMassTransit(x =>
{
    // consumers
    x.AddConsumer<PaymentSucceededConsumer>();
    x.AddConsumer<PaymentFailedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("oc");
            h.Password("ocpass");
        });

        // Stabilize ayarları
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

        // orderService, payment event' lerini burada dinler
        cfg.ReceiveEndpoint("order.payment-events", e =>
        {
            e.ConfigureConsumer<PaymentSucceededConsumer>(context);
            e.ConfigureConsumer<PaymentFailedConsumer>(context);
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ENDPOINTS

// 1) post/orders

app.MapPost("/orders", async (CreateOrderRequest req, OrderDbContext db, IPublishEndpoint publish) =>
{
    if (string.IsNullOrWhiteSpace(req.CustomerId))
        return Results.BadRequest("CustomerId is required");

    if (req.Amount <= 0)
        return Results.BadRequest("Amount must be > 0");

    if (string.IsNullOrWhiteSpace(req.Currency))
        return Results.BadRequest("Currency is required");

    var entity = new OrderEntity
    {
        Id = Guid.NewGuid(),
        CustomerId = req.CustomerId,
        Amount = req.Amount,
        Currency = req.Currency.Trim().ToUpperInvariant(),
        Status = OrderStatus.Created,
        CreatedAt = DateTimeOffset.UtcNow
    };

    db.Orders.Add(entity);
    await db.SaveChangesAsync();

    // Event yayınla
    await publish.Publish(new OrderCreated(
        OrderId: entity.Id,
        CustomerId: entity.CustomerId,
        Amount: entity.Amount,
        Currency: entity.Currency,
        CreatedAt: entity.CreatedAt
     ));

    return Results.Created($"/orders/{entity.Id}", entity);
});

// 2) get/orders (Sipariş listeleme)
app.MapGet("/orders", async (OrderDbContext db) =>
{
    var list = await db.Orders
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync();

    return Results.Ok(list);
});

// 3) get/orders/{id} Sipariş detay (id' ye göre getirme)
app.MapGet("/orders/{id:guid}", async (Guid id, OrderDbContext db) =>
{
    var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == id);
    return order is null ? Results.NotFound("Order not found") : Results.Ok(order);
});

app.Run();


// Modeller
public record CreateOrderRequest(string CustomerId, decimal Amount, string Currency);
