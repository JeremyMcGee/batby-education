namespace BatbyEducation.Application.DTOs;

/// <summary>
/// Represents a single month's financial summary within a tax year.
/// </summary>
public record MonthlySummaryDto(
    int Month,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal InvoicedRevenue,
    decimal CollectedRevenue,
    decimal OutstandingAmount);
