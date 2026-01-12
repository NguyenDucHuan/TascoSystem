using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasco.NotificationService.Core.Interfaces;

namespace Tasco.NotificationService.Infrastructure.Messaging
{
    public class RabbitMQConnection : IRabbitMQConnection, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQConnection> _logger;
        private IConnection? _connection;
        private readonly object _lock = new object();

        public RabbitMQConnection(IConfiguration configuration, ILogger<RabbitMQConnection> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public IConnection GetConnection()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                lock (_lock)
                {
                    if (_connection == null || !_connection.IsOpen)
                    {
                        _connection = CreateConnection();
                    }
                }
            }
            return _connection;
        }

        private IConnection CreateConnection()
        {
            try
            {
                var virtualHost = _configuration["RabbitMQ:VirtualHost"];
                if (string.IsNullOrWhiteSpace(virtualHost))
                {
                    virtualHost = "/";
                }

                var factory = new ConnectionFactory()
                {
                    HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                    Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                    UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                    Password = _configuration["RabbitMQ:Password"] ?? "guest",
                    VirtualHost = virtualHost,
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = true,
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                    RequestedHeartbeat = TimeSpan.FromSeconds(60)
                };

                _logger.LogInformation("Creating RabbitMQ connection to {HostName}:{Port} with VirtualHost: {VirtualHost}", 
                    factory.HostName, factory.Port, factory.VirtualHost);
                
                var connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                _logger.LogInformation("RabbitMQ connection established successfully");
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create RabbitMQ connection");
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                if (_connection?.IsOpen == true)
                {
                    _connection.CloseAsync().GetAwaiter().GetResult();
                }
                _connection?.Dispose();
                _logger.LogInformation("RabbitMQ connection disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ connection");
            }
        }
    }
}
