using MassTransit;
using MassTransitRequestResponseWebApi2.Contracts;

namespace MassTransitRequestResponseWebApi2.Consumers;

public class SmsNotificationConsumer : IConsumer<SendNotification>
{
    private readonly ILogger<SmsNotificationConsumer> logger;
    public SmsNotificationConsumer (ILogger<SmsNotificationConsumer> logger)
    {
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<SendNotification> context)
    {
        var notification = context.Message;
        if (notification.NotificationType != "Sms")
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