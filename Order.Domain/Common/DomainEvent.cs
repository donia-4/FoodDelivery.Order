using MediatR;

namespace Order.Domain.Common;

public abstract record DomainEvent : INotification;