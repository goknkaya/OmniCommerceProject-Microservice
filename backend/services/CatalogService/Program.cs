using MassTransit;
using Microsoft.EntityFrameworkCore;
using CatalogService.Consumers;
using CatalogService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// DB
var connStr = builder.Configuration.GetConnectionString("CatalogDb");

if (string.IsNullOrWhiteSpace(connStr))
    throw new InvalidOperationException("Connection string 'CatalogDb' not found.");

builder.Services.AddDbContext<CatalogDbContext>(opt =>
    opt.UseNpgsql(connStr));

// MASSTRANSIT (RABBITMQ)
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

        // Consumer endpoint
        cfg.ReceiveEndpoint("catalog.order-created", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
        });

        // Stabilize ayarlarý
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
    });
});

var app = builder.Build();
app.UseCors("AllowReact");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ENDPOINTS

// get/orders

app.MapGet("/received-orders", async (CatalogDbContext db) =>
{
    var list = await db.ReceivedOrders
        .OrderByDescending(x => x.ReceivedAt)
        .Take(50)
        .ToListAsync();

    return Results.Ok(list);
});

app.MapGet("/received-orders/{id:guid}", async (Guid id, CatalogDbContext db) =>
{
    var item = await db.ReceivedOrders.FirstOrDefaultAsync(x => x.Id == id);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

// RAPOR 1: Özel dashboard
app.MapGet("/reports/summary", async (CatalogDbContext db) =>
{
    var now = DateTimeOffset.UtcNow;
    var since24h = now.AddHours(-24);

    var total = await db.ReceivedOrders.CountAsync();
    var last24h = await db.ReceivedOrders.CountAsync(x => x.ReceivedAt >= since24h);

    var byCurrency = await db.ReceivedOrders
        .GroupBy(x => x.Currency)
        .Select(g => new { Currency = g.Key, Count = g.Count() })
        .OrderByDescending(x => x.Count)
        .ToListAsync();

    var last10 = await db.ReceivedOrders
        .OrderByDescending(x => x.ReceivedAt)
        .Take(10)
        .Select(x => new
        {
            x.Id,
            x.OrderId,
            x.CustomerId,
            x.Amount,
            x.Currency,
            x.CreatedAt,
            x.ReceivedAt
        })
        .ToListAsync();

    return Results.Ok(new
    {
        total,
        last24h,
        byCurrency,
        last10
    });
});

// RAPOR 2: Son N received order
app.MapGet("/reports/received-orders", async (int take, CatalogDbContext db) =>
{
    if (take <= 0) take = 10;
    if (take > 200) take = 200;

    var list = await db.ReceivedOrders
        .OrderByDescending(x => x.ReceivedAt)
        .Take(take)
        .ToListAsync();

    return Results.Ok(list);
});

// RAPOR 3: Top Customers (en çok sipariþ geçen müþteri)
app.MapGet("/reports/top-customers", async (int take, CatalogDbContext db) =>
{
    if (take <= 0) take = 5;
    if (take > 50) take = 50;

    var list = await db.ReceivedOrders
        .GroupBy(x => x.CustomerId)
        .Select(g => new
        {
            CustomerId = g.Key,
            OrderCount = g.Count(),
            TotalAmount = g.Sum(x => x.Amount)
        })
        .OrderByDescending(x => x.OrderCount)
        .ThenByDescending(x => x.TotalAmount)
        .Take(take)
        .ToListAsync();

    return Results.Ok(list);
});

// RAPOR 4: Daily Volume (son N gün)
app.MapGet("/reports/daily-volume", async (int days, CatalogDbContext db) =>
{
    if (days <= 0) days = 7;
    if (days > 60) days = 60;

    var since = DateTimeOffset.UtcNow.AddDays(-days);

    var list = await db.ReceivedOrders
        .Where(x => x.ReceivedAt >= since)
        .GroupBy(x => x.ReceivedAt.Date)
        .Select(g => new
        {
            Date = g.Key,
            OrderCount = g.Count(),
            TotalAmount = g.Sum(x => x.Amount)
        })
        .OrderByDescending(x => x.Date)
        .ToListAsync();

    return Results.Ok(list);
});


app.Run();