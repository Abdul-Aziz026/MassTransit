using MassTransit;
using SchedulingAndSagas.Events;
using SchedulingAndSagas.Services;

namespace SchedulingAndSagas.Consumers;

public class OrderFailedEventConsumer : IConsumer<OrderFailedEvent> 
{

    private readonly IMessagePublisher _messagePublisher;
    public OrderFailedEventConsumer(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }
    public async Task Consume(ConsumeContext<OrderFailedEvent> context)
    {
        var message = context.Message;
        var stockReserveFailed = new StockReservationFailedEvent()
        {
            CorrelationId = message.CorrelationId,
            OrderId = message.OrderId,
            Reason = "Money limited"
        };
        await _messagePublisher.PublishAsync(stockReserveFailed);
    }
}