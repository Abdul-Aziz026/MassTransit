using MassTransit;
using MassTransitRequestResponseWebApi2.Contracts;

namespace MassTransitRequestResponseWebApi2.Service;

public interface INotificationService
{
    Task<string> SendEmailNotificationAsync(string message);
    Task<string> SendSmsNotificationAsync(string message);
}

public class NotificationService : INotificationService
{
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ISendEndpointProvider sendEndpointProvider, ILogger<NotificationService> logger)
    {
        _sendEndpointProvider = sendEndpointProvider;
        _logger = logger;
    }

    public async Task<string> SendEmailNotificationAsync (string message)
    {
        var notification = new SendNotification
        {
            NotificationType = "Email",
            Message = message
        };

        // SEND Command - Only email consumer will receive this
        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:email-notifications"));
        await sendEndpoint.Send(notification);

        _logger.LogInformation("📧 SENT email command to specific queue");
        return "Email notification sent to queue";
    }

    public async Task<string> SendSmsNotificationAsync(string message)
    {
        var notification = new SendNotification
        {
            NotificationType = "SMS",
            Message = message
        };

        // SEND Command - Only SMS consumer will receive this
        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:sms-notifications"));
        await sendEndpoint.Send(notification);

        _logger.LogInformation("📱 SENT SMS command to specific queue");
        return "SMS notification sent to queue";
    }
}