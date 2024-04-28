using Common.Implementations;
using Common.Interfaces;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        public static IServiceCollection ConfigureMapster(this IServiceCollection services)
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
