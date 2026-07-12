using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Enumerations;

namespace BatbyEducation.Domain.Entities;

/// <summary>
/// Represents a ledger entry recording financial activity for reporting purposes.
/// </summary>
public class LedgerEntry : Entity
{
    public Guid PaymentId { get; private set; }
    public PaymentMethod LedgerType { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly EntryDate { get; private set; }
    public string InvoiceReference { get; private set; } = string.Empty;
    public string StudentName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private LedgerEntry() { }

    /// <summary>
    /// Creates a new LedgerEntry associated with a payment.
    /// </summary>
    public static LedgerEntry Create(Guid paymentId, PaymentMethod ledgerType, decimal amount, DateOnly entryDate, string invoiceReference, string studentName)
    {
        return new LedgerEntry
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            LedgerType = ledgerType,
            Amount = amount,
            EntryDate = entryDate,
            InvoiceReference = invoiceReference,
            StudentName = studentName,
            CreatedAt = DateTime.UtcNow
        };
    }
}
