namespace BatbyEducation.Application.DTOs;

/// <summary>
/// Represents a complete financial summary for a UK tax year (6 April to 5 April).
/// </summary>
public record TaxYearSummaryDto(
    decimal TotalIncome,
    decimal CashLedgerTotal,
    decimal BankTransferLedgerTotal,
    int TotalCompletedSessions,
    decimal TotalHoursTutored,
    decimal TotalInvoicedAmount,
    decimal OutstandingReceivables,
    List<MonthlySummaryDto> MonthlyBreakdown);
