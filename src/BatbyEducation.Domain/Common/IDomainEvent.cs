using MediatR;

namespace BatbyEducation.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// Extends INotification to enable MediatR-based event dispatching.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
