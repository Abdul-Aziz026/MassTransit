using MassTransit;
using MassTransitRequestResponseWebApi2.Contracts;

namespace MassTransitRequestResponseWebApi2.Consumers;

public class OrderProcessingConsumer : IConsumer<ProcessOrder>
{
    public async Task Consume(ConsumeContext<ProcessOrder> context)
    {
        var order = context.Message;
        
        // Respond back to the sender
        await context.RespondAsync(new OrderProcessingResult
            {
                OrderId = order.OrderId,
                Status = "Completed",
                Message = $"Order processed successfully with {order.Priority} priority",
                ProcessedAt = DateTime.UtcNow
            });
    }
}