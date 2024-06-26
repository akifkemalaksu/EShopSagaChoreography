﻿using Microsoft.Extensions.DependencyInjection;
using Common.Interfaces;

namespace Common.Implementations
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public QueryDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TQueryResult> Dispatch<TQuery, TQueryResult>(TQuery query, CancellationToken token)
        {
            var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TQueryResult>>();
            return handler.Handle(query, token);
        }
    }
}
