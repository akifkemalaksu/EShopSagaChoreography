using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.RabbitMQ.Connection
{
    public class RabbitMQPersistentConnection : IPersistentConnection
    {
        private readonly IConnectionFactory _connectionFactory;

        private readonly TimeSpan _timeoutBeforeReconnecting;

        private IConnection _connection;

        private bool _disposed;

        private readonly object _locker = new object();

        private readonly ILogger<RabbitMQPersistentConnection> _logger;

        private bool _connectionFailed = false;

        public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, ILogger<RabbitMQPersistentConnection> logger, int timeoutBeforeReconnecting)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
            _timeoutBeforeReconnecting = TimeSpan.FromSeconds(timeoutBeforeReconnecting);
        }

        public bool IsConnected
        {
            get
            {
                return (_connection != null) && (_connection.IsOpen) && (!_disposed);
            }
        }

        public event EventHandler OnReconnectedAfterConnectionFailure;

        public IModel CreateModel()
        {
            if (!IsConnected)
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action.");

            return _connection.CreateModel();
        }

        public bool TryConnect()
        {
            _logger.LogInformation("Trying to connect to RabbitMQ...");

            lock (_locker)
            {
                // Creates a policy to retry connecting to message broker until it succeeds.

                var policy = Policy
                    .Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetryForever((duration) => _timeoutBeforeReconnecting, (ex, time) =>
                    {
                        _logger.LogWarning(ex, "RabbitMQ Client could not connect after {TimeOut} seconds ({ExceptionMessage}). Waiting to try again...", $"{(int)time.TotalSeconds}", ex.Message);
                    });

                policy.Execute(() =>
                {
                    _connection = _connectionFactory.CreateConnection();
                });

                if (!IsConnected)
                {
                    _logger.LogCritical("ERROR: could not connect to RabbitMQ.");
                    _connectionFailed = true;
                    return false;
                }

                // These event handlers handle situations where the connection is lost by any reason. They try to reconnect the client.

                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;
                _connection.ConnectionUnblocked += OnConnectionUnBlocked;

                _logger.LogInformation("RabbitMQ Client acquired a persistent connection to '{HostName}' and is subscripted to failure events", _connection.Endpoint.HostName);

                // If the connection has failed previously because of a RabbitMQ shutdown or something similar, we need to guarantee that the exchange and queues exist again.
                // It's also necessary to rebind all application event handlers. We use this event handler below to do this.
                if (_connectionFailed)
                {
                    OnReconnectedAfterConnectionFailure?.Invoke(this, null);
                    _connectionFailed = false;
                }

                return true;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }

        private void OnCallbackException(object sender, EventArgs e)
        {
            _connectionFailed = true;

            _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");
            TryConnectIfNotDisposed();
        }

        private void OnConnectionShutdown(object sender, EventArgs e)
        {
            _connectionFailed = true;

            _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
            TryConnectIfNotDisposed();
        }

        private void OnConnectionBlocked(object sender, EventArgs e)
        {
            _connectionFailed = true;

            _logger.LogWarning("A RabbitMQ connection is blocked. Trying to re-connect...");
            TryConnectIfNotDisposed();
        }

        private void OnConnectionUnBlocked(object sender, EventArgs e)
        {
            _connectionFailed = true;

            _logger.LogWarning("A RabbitMQ connection is unblocked. Trying to re-connect...");
            TryConnectIfNotDisposed();
        }

        private void TryConnectIfNotDisposed()
        {
            if (_disposed)
            {
                _logger.LogInformation("RabbitMQ client is disposed. No action will be taken.");
                return;
            }

            TryConnect();
        }
    }
}
