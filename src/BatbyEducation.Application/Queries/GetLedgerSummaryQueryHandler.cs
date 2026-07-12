using MediatR;
using BatbyEducation.Application.DTOs;
using BatbyEducation.Domain.Interfaces;

namespace BatbyEducation.Application.Queries;

/// <summary>
/// Handles the GetLedgerSummaryQuery by retrieving entries from the appropriate ledger and calculating totals.
/// </summary>
public class GetLedgerSummaryQueryHandler : IRequestHandler<GetLedgerSummaryQuery, LedgerSummaryDto>
{
    private readonly ILedgerRepository _ledgerRepository;

    public GetLedgerSummaryQueryHandler(ILedgerRepository ledgerRepository)
    {
        _ledgerRepository = ledgerRepository;
    }

    public async Task<LedgerSummaryDto> Handle(GetLedgerSummaryQuery request, CancellationToken cancellationToken)
    {
        // Get ledger entries for the specified type and date range
        var entries = await _ledgerRepository.GetEntriesAsync(request.LedgerType, request.StartDate, request.EndDate);

        // Calculate totals
        var totalReceipts = entries.Sum(e => e.Amount);
        var transactionCount = entries.Count;

        // Map to DTOs in chronological order
        var entryDtos = entries
            .OrderBy(e => e.EntryDate)
            .Select(e => new LedgerEntryDto(
                e.EntryDate,
                e.Amount,
                e.InvoiceReference,
                e.StudentName))
            .ToList();

        return new LedgerSummaryDto(totalReceipts, transactionCount, entryDtos);
    }
}
