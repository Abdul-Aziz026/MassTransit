using MassTransit;

namespace MassTransitRequestResponseWebApi2.Extensions;


public static class CorrelationExtensions
{
    public static async Task PublishWithCorrelation<T>(this IPublishEndpoint endpoint, T message, Guid correlationId)
        where T : class
    {
        await endpoint.Publish(message, context => context.CorrelationId = correlationId);
    }

    public static async Task SendWithCorrelation<T>(this ISendEndpoint endpoint, T message, Guid correlationId)
        where T : class
    {
        await endpoint.Send(message, context => context.CorrelationId = correlationId);
    }

    public static void LogWithCorrelation(this ILogger logger, LogLevel level, Guid correlationId, string message, params object[] args)
    {
        var correlatedMessage = $"[CorrelationId: {correlationId}] {message}";
        logger.Log(level, correlatedMessage, args);
    }
}