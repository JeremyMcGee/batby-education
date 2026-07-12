using BatbyEducation.Domain.Common;

namespace BatbyEducation.Domain.Events;

public record SessionCancelledEvent(Guid SessionId, bool IsLateCancellation) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
