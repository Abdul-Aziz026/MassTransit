namespace MassTransitWebApi.Contracts;

public record SendNotification
{
    public string NotificationType { get; init; } = string.Empty; // Email, SMS, Push
    public string Message { get; init; } = string.Empty;
}