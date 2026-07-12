namespace BatbyEducation.Application.DTOs;

/// <summary>
/// Per-tutor earnings summary showing completed session hours and invoiced revenue for a date range.
/// </summary>
public record TutorEarningsDto(
    string TutorName,
    decimal TotalHours,
    decimal TotalInvoicedAmount);
