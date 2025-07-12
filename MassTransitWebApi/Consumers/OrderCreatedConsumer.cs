using MassTransit;
using MassTransitWebApi.Contracts;

namespace MassTransitWebApi.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly ILogger<OrderCreatedConsumer> logger;
    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    {
        this.logger = logger;
    }
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var order = context.Message;
        logger.LogInformation($"First Subscriber receibe order: {order.Id}");
        await ProcessOrder(order);
    }

    private async Task ProcessOrder(OrderCreated order)
    {
        logger.LogInformation("Order Processing...");
        await Task.Delay(2000);
        return;
    }
}
