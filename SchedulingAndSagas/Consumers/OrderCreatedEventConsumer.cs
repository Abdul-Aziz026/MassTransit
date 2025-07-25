using MassTransit;
using SchedulingAndSagas.Events;
using SchedulingAndSagas.Services;

namespace SchedulingAndSagas.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IMessagePublisher _messagePublisher;
    public OrderCreatedEventConsumer(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var createOrderEvent = context.Message;
        if (createOrderEvent.IsAvailable == 0)
        {
            var stockReservationFailed = new StockReservationFailedEvent()
            {
                CorrelationId = createOrderEvent.CorrelationId,
                Reason = "Stock is Unavailable",
                OrderId = createOrderEvent.OrderId
            };

            await _messagePublisher.PublishAsync(stockReservationFailed);
            return;
        }

        var stockReserved = new StockReservedEvent()
        {
            CorrelationId = createOrderEvent.CorrelationId,
            OrderId = createOrderEvent.OrderId
        };
        await _messagePublisher.PublishAsync(stockReserved);
    }
}