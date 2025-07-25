namespace SchedulingAndSagas.Events;

public class Notification
{
    public Guid CorrelationId { get; set; }
    public string OrderId { get; set; }
    public string Email { get; set; }
    public string Message { get; set; }
}