namespace BatbyEducation.Domain.Common;

/// <summary>
/// Dispatches domain events to their respective handlers.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(IEnumerable<IDomainEvent> events);
}
