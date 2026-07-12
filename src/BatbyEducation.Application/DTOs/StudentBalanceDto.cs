namespace BatbyEducation.Application.DTOs;

/// <summary>
/// Represents a student's outstanding balance summary.
/// </summary>
public record StudentBalanceDto(
    string StudentName,
    decimal TotalOutstanding,
    int UnpaidInvoiceCount,
    decimal CreditBalance);
