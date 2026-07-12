using MediatR;
using BatbyEducation.Application.DTOs;

namespace BatbyEducation.Application.Queries.TaxYear;

/// <summary>
/// Query to retrieve a month-by-month financial summary for a specified UK tax year.
/// Returns 12 entries with 6th-to-5th month boundaries.
/// </summary>
public record GetMonthlySummaryQuery(int TaxYearStartYear) : IRequest<List<MonthlySummaryDto>>;
