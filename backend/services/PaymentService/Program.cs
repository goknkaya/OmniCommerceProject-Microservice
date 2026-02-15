using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Consumers;
using PaymentService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB
var connStr = builder.Configuration.GetConnectionString("PaymentDb");
if (string.IsNullOrWhiteSpace(connStr))
    throw new InvalidOperationException("Connection string 'PaymentDb' not found.");

builder.Services.AddDbContext<PaymentDbContext>(opt =>
    opt.UseNpgsql(connStr));

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("oc");
            h.Password("ocpass");
        });

        cfg.ReceiveEndpoint("payment.order-created", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
        });

        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Demo endpoint (görmek için)
app.MapGet("/payments", async (PaymentDbContext db) =>
    await db.Payments.OrderByDescending(x => x.CreatedAt).ToListAsync());

app.Run();
