namespace Common.Interfaces
{
    public interface ICommandDispatcher
    {
        Task<TCommandResult> Dispatch<TCommand, TCommandResult>(TCommand command, CancellationToken token = default);
    }
}
