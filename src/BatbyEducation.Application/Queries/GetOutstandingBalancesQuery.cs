using MediatR;
using BatbyEducation.Application.DTOs;

namespace BatbyEducation.Application.Queries;

/// <summary>
/// Query to retrieve outstanding balances for all students.
/// Returns per-student balance = unpaid/partially paid invoices minus credits.
/// </summary>
public record GetOutstandingBalancesQuery : IRequest<List<StudentBalanceDto>>;
