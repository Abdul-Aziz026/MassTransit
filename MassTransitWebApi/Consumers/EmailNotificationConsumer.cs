using MassTransit;
using MassTransitWebApi.Contracts;

namespace MassTransitWebApi.Consumers;

public class EmailNotificationConsumer : IConsumer<SendNotification>
{
    private readonly ILogger<EmailNotificationConsumer> logger;
    public EmailNotificationConsumer(ILogger<EmailNotificationConsumer> logger)
    {
        this.logger = logger;
    }
    public async Task Consume(ConsumeContext<SendNotification> context)
    {
        var notification = context.Message;
        if (notification.NotificationType != "Email")
            return;
        logger.LogInformation($"Order id{notification.NotificationType}");
        await ProcessOrder(notification);
    }

    private async Task ProcessOrder(SendNotification order)
    {
        logger.LogInformation("Order Processing...");
        await Task.Delay(2000);
        return;
    }
}