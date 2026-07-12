using BatbyEducation.Domain.Common;

namespace BatbyEducation.Domain.Entities;

/// <summary>
/// Represents a single line item on an invoice, corresponding to a tutoring session or late cancellation fee.
/// </summary>
public class InvoiceLineItem : Entity
{
    public Guid InvoiceId { get; private set; }
    public Guid SessionId { get; private set; }
    public DateOnly SessionDate { get; private set; }
    public string TutorName { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public int DurationMinutes { get; private set; }
    public decimal Rate { get; private set; }
    public decimal Amount { get; private set; }
    public bool IsLateCancellationFee { get; private set; }

    private InvoiceLineItem() { }

    internal static InvoiceLineItem Create(
        Guid invoiceId,
        Guid sessionId,
        DateOnly sessionDate,
        string tutorName,
        string subject,
        int durationMinutes,
        decimal rate,
        decimal amount,
        bool isLateCancellationFee)
    {
        return new InvoiceLineItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            SessionId = sessionId,
            SessionDate = sessionDate,
            TutorName = tutorName,
            Subject = subject,
            DurationMinutes = durationMinutes,
            Rate = rate,
            Amount = amount,
            IsLateCancellationFee = isLateCancellationFee
        };
    }
}
