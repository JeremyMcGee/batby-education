using BatbyEducation.Domain.Common;

namespace BatbyEducation.Domain.Events;

public record SessionCompletedEvent(Guid SessionId, Guid StudentId, Guid TutorId, DateOnly SessionDate, int ActualDuration) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
