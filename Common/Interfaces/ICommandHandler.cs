namespace Common.Interfaces
{
    public interface ICommandHandler<in TCommand, TCommandResult>
    {
        Task<TCommandResult> Handle(TCommand command, CancellationToken token);
    }
}
