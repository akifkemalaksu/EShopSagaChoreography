﻿using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EventBus.RabbitMQ.Connection;
using EventBus.Bus;
using EventBus.Subscriptions;
using EventBus.Events;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.RabbitMQ.Bus
{
    /// <summary>
	/// Event Bus implementation that uses RabbitMQ as the message broker.
	/// The implementation is based on eShopOnContainers (Microsoft's tutorial about microservices in .NET Core), but it implements some features I have found that are based in different libraries.
	/// 
	/// References:
	/// - https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/integration-event-based-microservice-communications
	/// - https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/rabbitmq-event-bus-development-test-environment
	/// - https://github.com/ojdev/RabbitMQ.EventBus.AspNetCore
	/// </summary>
    public class RabbitMQEventBus : IEventBus
    {
        // todo: dead letter exchange implementasyonu
        private const string _exchangeName = "saga_event_bus";
        private readonly string _queueName;
        private readonly int _publishRetryCount = 5;
        private readonly TimeSpan _subscribeRetryTime = TimeSpan.FromSeconds(5);

        private readonly IPersistentConnection _persistentConnection;
        private readonly IEventBusSubscriptionManager _eventBusSubscriptionManager;
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<RabbitMQEventBus> _logger;

        private IModel _consumerChannel;

        public RabbitMQEventBus(IPersistentConnection persistentConnection, IEventBusSubscriptionManager eventBusSubscriptionManager, IServiceProvider serviceProvider, ILogger<RabbitMQEventBus> logger, string queueName)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _eventBusSubscriptionManager = eventBusSubscriptionManager ?? throw new ArgumentNullException(nameof(eventBusSubscriptionManager));
            _serviceProvider = serviceProvider;
            _logger = logger;
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));

            ConfigureMessageBroker();
        }

        public void Publish<TEvent>(TEvent @event) where TEvent : Event
        {
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            var policy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_publishRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, timeSpan) =>
                {
                    _logger.LogWarning(exception, "Could not publish event #{EventId} after {Timeout} seconds: {ExceptionMessage}.", @event.Id, $"{timeSpan.TotalSeconds:n1}", exception.Message);
                });

            var eventName = @event.GetType().Name;

            _logger.LogTrace("Creating RabbitMQ channel to publish event #{EventId} ({EventName})...", @event.Id, eventName);

            using (var channel = _persistentConnection.CreateModel())
            {
                _logger.LogTrace("Declaring RabbitMQ exchange to publish event #{EventId}...", @event.Id);

                channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);

                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();

                    properties.DeliveryMode = (byte)DeliveryMode.Persistent;

                    _logger.LogTrace("Publishing event to RabbitMQ with ID #{EventId}...", @event.Id);

                    channel.BasicPublish(
                        exchange: _exchangeName,
                        routingKey: eventName,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);

                    _logger.LogTrace("Published event with ID #{EventId}.", @event.Id);
                });
            }
        }

        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = _eventBusSubscriptionManager.GetEventIdentifier<TEvent>();
            var eventHandlerName = typeof(TEventHandler).Name;

            AddQueueBindForEventSubscription(eventName);

            _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}...", eventName, eventHandlerName);

            _eventBusSubscriptionManager.AddSubscription<TEvent, TEventHandler>();
            StartBasicConsume();

            _logger.LogInformation("Subscribed to event {EventName} with {EventHandler}.", eventName, eventHandlerName);
        }

        public void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = _eventBusSubscriptionManager.GetEventIdentifier<TEvent>();

            _logger.LogInformation("Unsubscribing from event {EventName}...", eventName);

            _eventBusSubscriptionManager.RemoveSubscription<TEvent, TEventHandler>();

            _logger.LogInformation("Unsubscribed from event {EventName}", eventName);
        }

        private void ConfigureMessageBroker()
        {
            _consumerChannel = CreateConsumerChannel();
            _eventBusSubscriptionManager.OnEventRemoved += SubscriptionManager_OnEventRemoved;
            _persistentConnection.OnReconnectedAfterConnectionFailure += PersistentConnection_OnReconnectedAfterConnectionFailure;
        }

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            _logger.LogTrace("Creating RabbitMQ consumer channel...");

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);
            channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
                );

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel...");
                DoCreateConsumerChannel();
            };

            _logger.LogTrace("Created RabbitMQ consumer channel.");

            return channel;
        }

        private void StartBasicConsume()
        {
            _logger.LogTrace("Starting RabbitMQ basic consume...");

            if (_consumerChannel == null)
            {
                _logger.LogError("Could not start basic consume because consumer channel is null.");
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
            consumer.Received += Consumer_Received;

            _consumerChannel.BasicConsume(
                queue: _queueName,
                autoAck: false,
                consumer: consumer
                );

            _logger.LogTrace("Started RabbitMQ basic consume.");
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

            bool isAcknowledged = false;

            try
            {
                await ProcessEvent(eventName, message);

                _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                isAcknowledged = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing the following message: {Message}", message);
            }
            finally
            {
                if (!isAcknowledged)
                {
                    await TryEnqueueMessageAgainAsync(eventArgs);
                }
            }
        }

        private async Task TryEnqueueMessageAgainAsync(BasicDeliverEventArgs eventArgs)
        {
            try
            {
                _logger.LogWarning("Adding message to queue again with {Time} seconds delay...", $"{_subscribeRetryTime.TotalSeconds:n1}");

                await Task.Delay(_subscribeRetryTime);
                _consumerChannel.BasicNack(eventArgs.DeliveryTag, false, true);

                _logger.LogTrace("Message added to queue again.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not enqueue message again: {Error}.", ex.Message);
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}...", eventName);

            if (!_eventBusSubscriptionManager.HasSubscriptionsForEvent(eventName))
            {
                _logger.LogTrace("There are no subscriptions for this event.");
                return;
            }

            var subscriptions = _eventBusSubscriptionManager.GetHandlersForEvent(eventName);

            // Fixed "Cannot resolve scoped service from root provider" error
            using var scope = _serviceProvider.CreateScope();

            foreach (var subscription in subscriptions)
            {
                var handler = scope.ServiceProvider.GetService(subscription.HandlerType);

                if (handler == null)
                {
                    _logger.LogWarning("There are no handlers for the following event: {EventName}", eventName);
                    continue;
                }

                var eventType = _eventBusSubscriptionManager.GetEventTypeByName(eventName);

                var @event = JsonSerializer.Deserialize(message, eventType);

                var eventHandlerType = typeof(IEventHandler<>).MakeGenericType(eventType);

                await Task.Yield();

                await (Task)eventHandlerType.GetMethod(nameof(IEventHandler<Event>.HandleAsync)).Invoke(handler, [@event]);
            }

            _logger.LogTrace("Processed event {EventName}.", eventName);
        }

        private void SubscriptionManager_OnEventRemoved(object? sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(queue: _queueName, exchange: _exchangeName, routingKey: eventName);

                if (_eventBusSubscriptionManager.IsEmpty)
                    _consumerChannel.Close();
            }
        }

        private void AddQueueBindForEventSubscription(string eventName)
        {
            var containsKey = _eventBusSubscriptionManager.HasSubscriptionsForEvent(eventName);

            if (containsKey)
                return;

            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            using (var channel = _persistentConnection.CreateModel())
                channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: eventName);
        }

        private void PersistentConnection_OnReconnectedAfterConnectionFailure(object? sender, EventArgs e)
        {
            DoCreateConsumerChannel();
            RecreateSubscriptions();
        }

        private void DoCreateConsumerChannel()
        {
            _consumerChannel.Dispose();
            _consumerChannel = CreateConsumerChannel();
            StartBasicConsume();
        }

        private void RecreateSubscriptions()
        {
            var subscriptions = _eventBusSubscriptionManager.GetAllSubscriptions();

            _eventBusSubscriptionManager.Clear();

            Type eventBusType = this.GetType();
            MethodInfo genericSubscribe;

            foreach (var entry in subscriptions)
                foreach (var subscription in entry.Value)
                {
                    genericSubscribe = eventBusType.GetMethod("Subscribe").MakeGenericMethod(subscription.EventType, subscription.HandlerType);
                    genericSubscribe.Invoke(this, null);
                }
        }

    }
}
