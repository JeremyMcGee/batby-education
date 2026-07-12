using BatbyEducation.Domain.Common;

namespace BatbyEducation.Domain.Entities;

/// <summary>
/// Tracks credits and overall balance per student.
/// Used to record overpayments and apply credits to future invoices.
/// </summary>
public class StudentAccount : Entity
{
    public Guid StudentId { get; private set; }
    public decimal CreditBalance { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private StudentAccount() { }

    /// <summary>
    /// Creates a new StudentAccount with zero credit balance.
    /// </summary>
    public static StudentAccount Create(Guid studentId)
    {
        return new StudentAccount
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            CreditBalance = 0m,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Adds credit to the student account. Amount must be positive.
    /// </summary>
    /// <param name="amount">The credit amount to add (must be greater than zero).</param>
    /// <exception cref="ArgumentException">Thrown when amount is less than or equal to zero.</exception>
    public void AddCredit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Credit amount must be greater than zero.", nameof(amount));

        CreditBalance += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deducts credit from the student account, capped at the current balance (will not go below zero).
    /// </summary>
    /// <param name="amount">The amount to deduct (must be greater than zero).</param>
    /// <returns>The actual amount deducted (may be less than requested if insufficient balance).</returns>
    /// <exception cref="ArgumentException">Thrown when amount is less than or equal to zero.</exception>
    public decimal DeductCredit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Deduction amount must be greater than zero.", nameof(amount));

        var actualDeduction = Math.Min(amount, CreditBalance);
        CreditBalance -= actualDeduction;
        UpdatedAt = DateTime.UtcNow;

        return actualDeduction;
    }
}
