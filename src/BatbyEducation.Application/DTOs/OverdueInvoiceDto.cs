namespace BatbyEducation.Application.DTOs;

/// <summary>
/// Represents an overdue invoice entry for reporting.
/// </summary>
public record OverdueInvoiceDto(
    string StudentName,
    string GuardianName,
    string InvoiceNumber,
    decimal AmountOutstanding,
    int DaysOverdue);
