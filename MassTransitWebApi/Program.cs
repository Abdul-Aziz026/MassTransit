using MassTransit;
using MassTransitWebApi.Consumers;
using MassTransitWebApi.Middleware;
using MassTransitWebApi.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<OrderCreatedConsumer2>();
    x.AddConsumer<EmailNotificationConsumer>();
    x.AddConsumer<SmsNotificationConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("order-submitted-queue", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
            e.ConfigureConsumer<OrderCreatedConsumer2>(context);
        });

        cfg.ReceiveEndpoint("email-notifications", e =>
        {
            e.ConfigureConsumer<EmailNotificationConsumer>(context);
            e.UseRetry(r => r.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)));
        });

        cfg.ReceiveEndpoint("sms-notifications", e =>
        {
            e.ConfigureConsumer<SmsNotificationConsumer>(context);
            e.UseMessageRetry(r => r.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)));
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<MonitoringMiddleware>();
app.MapControllers();

app.Run();