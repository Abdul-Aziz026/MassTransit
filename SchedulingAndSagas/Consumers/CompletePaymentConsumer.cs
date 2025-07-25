using MassTransit;
using SchedulingAndSagas.Contracts;
using SchedulingAndSagas.Events;
using SchedulingAndSagas.Services;

namespace SchedulingAndSagas.Consumers;

public class CompletePaymentConsumer : IConsumer<CompletePaymentMessage>
{
    private readonly IMessagePublisher _messagePublisher;
    public CompletePaymentConsumer (IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    public async Task Consume (ConsumeContext<CompletePaymentMessage> context)
    {
        var paymentMessage = context.Message;
        if (paymentMessage.TotalPrice == 0)
        {
            var paymentFailed = new PaymentFailedEvent()
            {
                CorrelationId = paymentMessage.CorrelationId,
                Cause = "Money is unavailable!!!"
            };
            await _messagePublisher.PublishAsync(paymentFailed);
            return;
        }
        var completePayment = new PaymentCompletedEvent()
        {
            CorrelationId = paymentMessage.CorrelationId,
            CompletedAt = DateTime.Now,
            Amount = paymentMessage.TotalPrice
        };
        await _messagePublisher.PublishAsync(completePayment);
    }
}