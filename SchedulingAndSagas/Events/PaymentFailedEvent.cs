using MassTransit;

namespace SchedulingAndSagas.Events;

public class PaymentFailedEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public string Cause { get; set; }
}