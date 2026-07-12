using MediatR;
using BatbyEducation.Application.DTOs;

namespace BatbyEducation.Application.Queries.TaxYear;

/// <summary>
/// Query to retrieve a complete financial summary for a specified UK tax year (6 April to 5 April).
/// Includes total income, cash/bank split, sessions, hours, invoiced amount, and outstanding receivables.
/// </summary>
public record GetTaxYearSummaryQuery(int TaxYearStartYear) : IRequest<TaxYearSummaryDto>;
