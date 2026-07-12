namespace BatbyEducation.Application.DTOs;

/// <summary>
/// Revenue report summary for a date range showing totals and payment method breakdown.
/// </summary>
public record RevenueReportDto(
    decimal TotalInvoiced,
    decimal TotalPaymentsReceived,
    decimal TotalOutstanding,
    decimal CashReceipts,
    decimal BankTransferReceipts);
