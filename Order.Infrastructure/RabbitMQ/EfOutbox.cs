using System.Text.Json;
using Order.Application.Common.Interfaces.Messaging;
using Order.Domain.Outbox;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.RabbitMQ
{
    public sealed class EfOutbox : IOutbox
    {
        private readonly OrderDbContext _dbContext;

        public EfOutbox(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task AddAsync<TEvent>(
            TEvent @event,
            string routingKey,
            CancellationToken cancellationToken = default)
            where TEvent : class
        {
            var message = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = typeof(TEvent).AssemblyQualifiedName!,
                Content = JsonSerializer.Serialize(@event, @event.GetType()),
                RoutingKey = routingKey,
                OccurredOnUtc = DateTime.UtcNow
            };

            _dbContext.OutboxMessages.Add(message);
            return Task.CompletedTask;
        }
    }
}
