using EventBus.Bus;
using EventBus.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EventBus.RabbitMQ.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder AddEvent<TEvent, TEventHandler>(this IApplicationBuilder builder)
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            // todo: Assembly üzerindeki tüm events ve eventHandlers çek subscribe et.

            var eventBus = builder.ApplicationServices.GetRequiredService<IEventBus>();

            eventBus.Subscribe<TEvent, TEventHandler>();

            return builder;
        }
    }
}
