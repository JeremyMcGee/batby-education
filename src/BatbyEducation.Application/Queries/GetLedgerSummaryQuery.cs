using MediatR;
using BatbyEducation.Application.DTOs;
using BatbyEducation.Domain.Enumerations;

namespace BatbyEducation.Application.Queries;

/// <summary>
/// Query to retrieve a ledger summary for a specified payment method and date range.
/// Returns total receipts, transaction count, and chronological entries.
/// </summary>
public record GetLedgerSummaryQuery(
    PaymentMethod LedgerType,
    DateOnly StartDate,
    DateOnly EndDate) : IRequest<LedgerSummaryDto>;
