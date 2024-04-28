using Common.Implementations;
using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureCQRSServices(this IServiceCollection services)
        {
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            services.AddScoped<IQueryDispatcher, QueryDispatcher>();

            services.Scan(selector =>
            {
                selector.FromEntryAssembly()
                .AddClasses(filter =>
                {
                    filter.AssignableTo(typeof(IQueryHandler<,>));
                })
                .AsImplementedInterfaces()
                .WithScopedLifetime();
            });

            services.Scan(selector =>
            {
                selector.FromEntryAssembly()
                .AddClasses(filter =>
                {
                    filter.AssignableTo(typeof(ICommandHandler<,>));
                })
                .AsImplementedInterfaces()
                .WithScopedLifetime();
            });

            return services;
        }
    }
}
