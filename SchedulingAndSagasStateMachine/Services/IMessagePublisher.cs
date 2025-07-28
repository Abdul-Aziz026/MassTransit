namespace SchedulingAndSagasStateMachine.Services;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message) where T : class;
}