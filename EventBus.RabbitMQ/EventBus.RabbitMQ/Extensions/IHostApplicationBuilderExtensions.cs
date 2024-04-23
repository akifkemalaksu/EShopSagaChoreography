
using EventBus.Bus;
using EventBus.RabbitMQ.Bus;
using EventBus.RabbitMQ.Connection;
using EventBus.RabbitMQ.Settings;
using EventBus.Subscriptions;
using EventBus.Subscriptions.InMemory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ.Extensions
{
    public static class IHostApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds an event bus that uses RabbitMQ to deliver messages.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="timeoutBeforeReconnecting">The amount of time in seconds the application will wait after trying to reconnect to RabbitMQ.</param>
        public static IHostApplicationBuilder AddRabbitMQEventBus(this IHostApplicationBuilder builder, int timeoutBeforeReconnecting = 15)
        {
            var eventBusSettings = builder.Configuration.GetSection(nameof(EventBusSettings)).Get<EventBusSettings>();

            builder.Services.AddSingleton<IEventBusSubscriptionManager, InMemoryEventBusSubscriptionManager>();
            builder.Services.AddSingleton<IPersistentConnection, RabbitMQPersistentConnection>(factory =>
            {
                var connectionFactory = new ConnectionFactory
                {
                    Uri = new Uri(eventBusSettings.ConnectionUrl),
                    DispatchConsumersAsync = true
                };

                var logger = factory.GetRequiredService<ILogger<RabbitMQPersistentConnection>>();
                return new RabbitMQPersistentConnection(connectionFactory, logger, timeoutBeforeReconnecting);
            });

            builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>(factory =>
            {
                var persistentConnection = factory.GetService<IPersistentConnection>();
                var subscriptionManager = factory.GetService<IEventBusSubscriptionManager>();

                var logger = factory.GetService<ILogger<RabbitMQEventBus>>();

                return new RabbitMQEventBus(persistentConnection, subscriptionManager, factory, logger, eventBusSettings.ClientName);
            });

            return builder;
        }
    }
}
