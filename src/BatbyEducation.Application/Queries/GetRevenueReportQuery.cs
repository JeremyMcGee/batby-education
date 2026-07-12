using MediatR;
using BatbyEducation.Application.DTOs;

namespace BatbyEducation.Application.Queries;

/// <summary>
/// Query to retrieve a revenue report for a specified date range.
/// Returns total invoiced, total payments received, outstanding balance, and cash/bank breakdown.
/// </summary>
public record GetRevenueReportQuery(
    DateOnly StartDate,
    DateOnly EndDate) : IRequest<RevenueReportDto>;
