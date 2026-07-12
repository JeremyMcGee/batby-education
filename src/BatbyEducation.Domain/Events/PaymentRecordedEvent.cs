using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Enumerations;

namespace BatbyEducation.Domain.Events;

public record PaymentRecordedEvent(Guid PaymentId, Guid InvoiceId, decimal Amount, PaymentMethod Method) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
