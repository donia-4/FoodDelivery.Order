using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Common.Interfaces.Messaging
{
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(
            TEvent @event,
            string routingKey,
            CancellationToken cancellationToken = default)
            where TEvent : class;
    }
}
