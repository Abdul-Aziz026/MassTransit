using MassTransit;
using SchedulingAndSagasStateMachine.Contracts;
using SchedulingAndSagasStateMachine.Events;
using SchedulingAndSagasStateMachine.Services;

namespace SchedulingAndSagasStateMachine.Consumers;

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
        await Task.Delay(5000);
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