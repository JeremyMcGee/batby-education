using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Enumerations;

namespace BatbyEducation.Domain.Entities;

/// <summary>
/// Represents a payment recorded against an invoice.
/// </summary>
public class Payment : Entity
{
    public Guid InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly PaymentDate { get; private set; }
    public PaymentMethod Method { get; private set; }
    public DateTime RecordedAt { get; private set; }

    private Payment() { }

    /// <summary>
    /// Creates a new Payment after validating that the amount is greater than zero.
    /// </summary>
    public static Result<Payment> Create(Guid invoiceId, decimal amount, DateOnly paymentDate, PaymentMethod method)
    {
        if (amount <= 0)
        {
            return Result<Payment>.Failure("Amount", "Amount must be greater than zero");
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            Amount = amount,
            PaymentDate = paymentDate,
            Method = method,
            RecordedAt = DateTime.UtcNow
        };

        return Result<Payment>.Success(payment);
    }
}
