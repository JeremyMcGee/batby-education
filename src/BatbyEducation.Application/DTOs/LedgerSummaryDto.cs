namespace BatbyEducation.Application.DTOs;

/// <summary>
/// Summary of a ledger for a date range including totals and chronological entries.
/// </summary>
public record LedgerSummaryDto(
    decimal TotalReceipts,
    int TransactionCount,
    IReadOnlyList<LedgerEntryDto> Entries);

/// <summary>
/// Individual ledger entry for display in a ledger summary.
/// </summary>
public record LedgerEntryDto(
    DateOnly Date,
    decimal Amount,
    string InvoiceReference,
    string StudentName);
