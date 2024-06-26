﻿using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Implementations
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TCommandResult> Dispatch<TCommand, TCommandResult>(TCommand command, CancellationToken token)
        {
            var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TCommandResult>>();
            return handler.Handle(command, token);
        }
    }
}
