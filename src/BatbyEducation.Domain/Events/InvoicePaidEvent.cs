using BatbyEducation.Domain.Common;

namespace BatbyEducation.Domain.Events;

public record InvoicePaidEvent(Guid InvoiceId, Guid StudentId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
