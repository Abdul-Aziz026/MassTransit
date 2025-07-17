using MassTransit;
using MassTransitRequestResponseWebApi2.Contracts.Commands;
using MassTransitRequestResponseWebApi2.Extensions;

namespace MassTransitRequestResponseWebApi2.Consumers;


public class PaymentConsumer : IConsumer<ProcessPayment>
{
    private readonly ILogger<PaymentConsumer> _logger;
    private static int _paymentAttempts = 0;

    public PaymentConsumer(ILogger<PaymentConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        var payment = context.Message;
        var attemptNumber = Interlocked.Increment(ref _paymentAttempts);
        var correlationId = context.CorrelationId ?? payment.CorrelationId;

        _logger.LogWithCorrelation(LogLevel.Information, correlationId,
            "💳 [PAYMENT-CONSUMER] Processing payment for order {OrderId} - Amount: ${Amount} (Attempt #{AttemptNumber})",
            payment.OrderId, payment.Amount, attemptNumber);

        try
        {
            // Simulate payment processing time
            await Task.Delay(Random.Shared.Next(1000, 3000));

            // Intentionally fail some payments to demonstrate retry and multiple response types
            if (ShouldSimulatePaymentFailure(payment.OrderId, attemptNumber))
            {
                var errorMessage = GetPaymentFailureMessage(payment.OrderId, attemptNumber);
                _logger.LogWithCorrelation(LogLevel.Error, correlationId,
                    "❌ [PAYMENT-CONSUMER] Payment failed for order {OrderId}: {Error}",
                    payment.OrderId, errorMessage);

                await context.RespondAsync(new PaymentFailed
                {
                    CorrelationId = correlationId,
                    OrderId = payment.OrderId,
                    ErrorCode = GetErrorCode(payment.OrderId),
                    ErrorMessage = errorMessage
                });
                return;
            }

            // Simulate successful payment
            var transactionId = $"TXN-{payment.OrderId}-{DateTime.UtcNow:yyyyMMddHHmmss}-{attemptNumber}";

            await context.RespondAsync(new PaymentSuccessful
            {
                CorrelationId = correlationId,
                OrderId = payment.OrderId,
                TransactionId = transactionId,
                Amount = payment.Amount
            });

            _logger.LogWithCorrelation(LogLevel.Information, correlationId,
                "✅ [PAYMENT-CONSUMER] Payment successful for order {OrderId} - Transaction: {TransactionId}",
                payment.OrderId, transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogWithCorrelation(LogLevel.Error, correlationId,
                "💥 [PAYMENT-CONSUMER] Payment processing error for order {OrderId}: {Error}",
                payment.OrderId, ex.Message);

            await context.RespondAsync(new PaymentFailed
            {
                CorrelationId = correlationId,
                OrderId = payment.OrderId,
                ErrorCode = "SYSTEM_ERROR",
                ErrorMessage = $"Payment processing failed: {ex.Message}"
            });
        }
    }

    private bool ShouldSimulatePaymentFailure(int orderId, int attemptNumber)
    {
        return orderId switch
        {
            // Order 10: Always fails payment
            10 => true,
            // Order 11: Fails first attempt, succeeds on retry
            11 => attemptNumber == 1,
            // Order 12: Fails on even attempts
            12 => attemptNumber % 2 == 0,
            // Orders 13-15: Random 25% failure rate
            >= 13 and <= 15 => Random.Shared.Next(1, 5) == 1,
            // All other orders: Success
            _ => false
        };
    }

    private string GetPaymentFailureMessage(int orderId, int attemptNumber)
    {
        return orderId switch
        {
            10 => "Credit card declined - insufficient funds",
            11 => $"Payment gateway timeout (attempt {attemptNumber})",
            12 => $"Payment processor unavailable (attempt {attemptNumber})",
            _ => $"Random payment error for order {orderId} (attempt {attemptNumber})"
        };
    }

    private string GetErrorCode(int orderId)
    {
        return orderId switch
        {
            10 => "CARD_DECLINED",
            11 => "GATEWAY_TIMEOUT",
            12 => "PROCESSOR_UNAVAILABLE",
            _ => "PAYMENT_ERROR"
        };
    }
}