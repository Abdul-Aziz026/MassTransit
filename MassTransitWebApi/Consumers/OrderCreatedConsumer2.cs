using MassTransit;
using MassTransitWebApi.Contracts;

namespace MassTransitWebApi.Consumers;

public class OrderCreatedConsumer2 : IConsumer<OrderCreated>
{
    private readonly ILogger<OrderCreatedConsumer2> logger;
    public OrderCreatedConsumer2(ILogger<OrderCreatedConsumer2> logger)
    {
        this.logger = logger;
    }
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var order = context.Message;
        logger.LogInformation($"Second Subscriber receibe order: {order.Id}");
        await ProcessOrder(order);
    }

    private async Task ProcessOrder(OrderCreated order)
    {
        logger.LogInformation("Order Processing...");
        await Task.Delay(2000);
        return;
    }
}
