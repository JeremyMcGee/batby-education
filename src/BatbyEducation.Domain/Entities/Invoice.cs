using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Enumerations;

namespace BatbyEducation.Domain.Entities;

/// <summary>
/// Represents an invoice issued to a student for tutoring sessions within a billing period.
/// </summary>
public class Invoice : AggregateRoot
{
    public string InvoiceNumber { get; private set; } = string.Empty;
    public Guid StudentId { get; private set; }
    public DateOnly BillingPeriodStart { get; private set; }
    public DateOnly BillingPeriodEnd { get; private set; }
    public decimal TotalAmount { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public decimal TotalPaid { get; private set; }

    private readonly List<InvoiceLineItem> _lineItems = new();
    public IReadOnlyList<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();

    private Invoice() { }

    /// <summary>
    /// Creates a new Invoice with Status = Issued and IssuedAt = DateTime.UtcNow.
    /// </summary>
    public static Invoice Create(
        string invoiceNumber,
        Guid studentId,
        DateOnly billingPeriodStart,
        DateOnly billingPeriodEnd)
    {
        return new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            StudentId = studentId,
            BillingPeriodStart = billingPeriodStart,
            BillingPeriodEnd = billingPeriodEnd,
            TotalAmount = 0m,
            TotalPaid = 0m,
            Status = InvoiceStatus.Created,
            IssuedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Adds a line item to the invoice and recalculates the total amount.
    /// </summary>
    public void AddLineItem(
        Guid sessionId,
        DateOnly sessionDate,
        string tutorName,
        string subject,
        int durationMinutes,
        decimal rate,
        decimal amount,
        bool isLateCancellationFee)
    {
        var lineItem = InvoiceLineItem.Create(
            Id,
            sessionId,
            sessionDate,
            tutorName,
            subject,
            durationMinutes,
            rate,
            amount,
            isLateCancellationFee);

        _lineItems.Add(lineItem);
        TotalAmount = _lineItems.Sum(li => li.Amount);
    }

    /// <summary>
    /// Records a payment against the invoice. Transitions status based on total paid vs total amount.
    /// Returns the overpayment amount (if any).
    /// </summary>
    public decimal RecordPayment(decimal amount)
    {
        TotalPaid += amount;

        if (TotalPaid >= TotalAmount)
        {
            Status = InvoiceStatus.Paid;
            PaidAt = DateTime.UtcNow;
            return TotalPaid - TotalAmount;
        }

        if (TotalPaid > 0)
        {
            Status = InvoiceStatus.PartiallyPaid;
        }

        return 0m;
    }

    /// <summary>
    /// Returns the outstanding balance on the invoice (minimum 0).
    /// </summary>
    public decimal GetOutstandingBalance()
    {
        var balance = TotalAmount - TotalPaid;
        return balance > 0 ? balance : 0m;
    }
}
