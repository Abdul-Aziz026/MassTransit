
using MassTransit;

namespace SchedulingAndSagas.Services;

public class MessagePublisher : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    public MessagePublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<T>(T message) where T : class
    {
        await _publishEndpoint.Publish(message);
    }
}