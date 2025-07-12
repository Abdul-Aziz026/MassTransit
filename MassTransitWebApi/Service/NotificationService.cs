using MassTransit;
using MassTransitWebApi.Contracts;

namespace MassTransitWebApi.Service;

public interface INotificationService
{
    Task<string> SendEmailAsync(string message);
    Task<string> SendSmsAsync(string message);
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

    public async Task<string> SendEmailAsync(string message)
    {
        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:email-notifications"));

        var notification = new SendNotification
        {
            NotificationType = "Email",
            Message = message
        };

        // SEND - Only email service will receive this command
        await sendEndpoint.Send(notification);

        _logger.LogInformation("📤 SENT email notification command - ONLY email service will receive this");

        return "Email notification sent to specific email service";
    }

    public async Task<string> SendSmsAsync(string message)
    {
        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:sms-notifications"));

        var notification = new SendNotification
        {
            NotificationType = "SMS",
            Message = message
        };

        // SEND - Only SMS service will receive this command
        await sendEndpoint.Send(notification);

        _logger.LogInformation("📤 SENT SMS notification command - ONLY SMS service will receive this");

        return "SMS notification sent to specific SMS service";
    }
}