using MassTransit;
using SchedulingAndSagasStateMachine.Consumers;
using SchedulingAndSagasStateMachine.Events;
using SchedulingAndSagasStateMachine.Services;
using SchedulingAndSagasStateMachine.StateInstances;
using StateMachines;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// register services
builder.Services.AddScoped<IMessagePublisher, MessagePublisher>();

// Add MassTransit with simplified in-memory configuration
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedEventConsumer>();
    x.AddConsumer<CompletePaymentConsumer>();
    x.AddConsumer<OrderFailedEventConsumer>();


    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .InMemoryRepository();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Configure the scheduler - uses RabbitMQ delayed exchange
        cfg.UseDelayedMessageScheduler();
        cfg.ConfigureEndpoints(context, f =>
        {
            // Exclude timeout events from automatic endpoint creation
            f.Exclude<PaymentTimeoutEvent>();
            f.Exclude<OrderExpirationEvent>();
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();

app.Run();
