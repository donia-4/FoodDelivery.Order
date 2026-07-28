using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Order.Application.Common.Interfaces.Messaging;
using RabbitMQ.Client;

namespace Order.Infrastructure.RabbitMQ
{
    public sealed class RabbitMqPublisher(
    IOptions<RabbitMqOptions> options)
    : IEventPublisher, IAsyncDisposable
    {
        private readonly RabbitMqOptions _options = options.Value;

        private IConnection? _connection;

        private IChannel? _channel;

        public async Task PublishAsync<TEvent>(
            TEvent @event,
            string routingKey,
            CancellationToken cancellationToken = default)
            where TEvent : class
        {
            await EnsureConnectionAsync(cancellationToken);

            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(@event));

            await _channel!.ExchangeDeclareAsync(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await _channel.BasicPublishAsync(
                exchange: _options.ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                body: body,
                cancellationToken: cancellationToken);
        }

        private async Task EnsureConnectionAsync(
            CancellationToken cancellationToken)
        {
            if (_connection is not null &&
                _connection.IsOpen &&
                _channel is not null)
            {
                return;
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            _connection = await factory.CreateConnectionAsync(
                cancellationToken);

            _channel = await _connection.CreateChannelAsync(
                cancellationToken: cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null)
            {
                await _channel.DisposeAsync();
            }

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }
        }
    }
}
