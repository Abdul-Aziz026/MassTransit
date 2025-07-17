using MassTransit;
using MassTransitRequestResponseWebApi2.Consumers;
using MassTransitRequestResponseWebApi2.Contracts.Commands;
using MassTransitRequestResponseWebApi2.Service;
using System.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add application services
builder.Services.AddScoped<IOrderService, OrderService>();

// Add MassTransit with comprehensive resilience configuration
builder.Services.AddMassTransit(x =>
{
    // Add consumers
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<PaymentConsumer>();
    x.AddConsumer<InventoryConsumer>();

    // Add request clients with timeouts
    x.AddRequestClient<ProcessPayment>(timeout: TimeSpan.FromSeconds(30));
    x.AddRequestClient<CheckInventory>(timeout: TimeSpan.FromSeconds(15));

    // Configure RabbitMQ
    x.UsingRabbitMq ((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Global retry policy
        cfg.UseMessageRetry(r =>
        {
            r.Intervals(
                TimeSpan.FromSeconds(1),    // 1st retry after 1 second
                TimeSpan.FromSeconds(5),    // 2nd retry after 5 seconds  
                TimeSpan.FromSeconds(15)    // 3rd retry after 15 seconds
            );

            // Handle specific exceptions
            r.Handle<InvalidOperationException>();
            r.Handle<TimeoutException>();
            r.Handle<ArgumentException>();

            // Ignore certain exceptions (don't retry these)
            r.Ignore<UnauthorizedAccessException>();
            r.Ignore<SecurityException>();
        });

        // Circuit breaker configuration
        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromMinutes(1);
            cb.TripThreshold = 15;
            cb.ActiveThreshold = 10;
            cb.ResetInterval = TimeSpan.FromMinutes(5);
        });

        // Rate limiting
        cfg.UseRateLimit(1000, TimeSpan.FromMinutes(1));

        // Order processing endpoint with specific retry policy
        cfg.ReceiveEndpoint("order-processing", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);

            // Endpoint-specific retry policy (overrides global for this endpoint)
            e.UseMessageRetry(r =>
            {
                r.Intervals(
                    TimeSpan.FromSeconds(2),    // 1st retry after 2 seconds
                    TimeSpan.FromSeconds(10),   // 2nd retry after 10 seconds
                    TimeSpan.FromSeconds(30)    // 3rd retry after 30 seconds
                );

                // Only retry specific exceptions for order processing
                r.Handle<InvalidOperationException>();
                r.Handle<TimeoutException>();
            });

            e.PrefetchCount = 10;
            e.ConcurrentMessageLimit = 5;
            e.UseInMemoryOutbox();
        });

        // Payment processing endpoint
        cfg.ReceiveEndpoint("payment-processing", e =>
        {
            e.ConfigureConsumer<PaymentConsumer>(context);

            // Payment-specific retry policy
            e.UseMessageRetry(r =>
            {
                r.Intervals(
                    TimeSpan.FromSeconds(1),    // Quick retry for payments
                    TimeSpan.FromSeconds(3),    // Second attempt
                    TimeSpan.FromSeconds(10)    // Final attempt
                );

                r.Handle<InvalidOperationException>();
                r.Handle<TimeoutException>();
            });

            e.PrefetchCount = 5;
            e.ConcurrentMessageLimit = 3;
            e.UseInMemoryOutbox();
        });

        // Inventory checking endpoint
        cfg.ReceiveEndpoint("inventory-checking", e =>
        {
            e.ConfigureConsumer<InventoryConsumer>(context);

            // Immediate retry for inventory (fast operations)
            e.UseMessageRetry(r =>
            {
                r.Immediate(3);  // Retry immediately 3 times
                r.Handle<InvalidOperationException>();
                r.Handle<TimeoutException>();
            });

            e.PrefetchCount = 20;
            e.ConcurrentMessageLimit = 10;
            e.UseInMemoryOutbox();
        });

        // Configure endpoints
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("AllowAll");

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("🚀 MassTransit Resilience Demo Started");
app.Logger.LogInformation("📖 Swagger UI: https://localhost:7000");
app.Logger.LogInformation("🐰 RabbitMQ Management: http://localhost:15672");
app.Logger.LogInformation("🔄 Retry Policies: Configured with different strategies per endpoint");
app.Logger.LogInformation("🛡️ Circuit Breaker: Active (15 failures in 1 minute)");
app.Logger.LogInformation("⚡ Rate Limiting: 1000 messages per minute");
app.Logger.LogInformation("🎯 CorrelationId: Enabled for distributed tracing");

app.Run();