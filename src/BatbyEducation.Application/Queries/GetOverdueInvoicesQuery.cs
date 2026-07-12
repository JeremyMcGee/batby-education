using MediatR;
using BatbyEducation.Application.DTOs;

namespace BatbyEducation.Application.Queries;

/// <summary>
/// Query to retrieve all overdue invoices (> 30 days past issue date),
/// sorted by days overdue descending.
/// </summary>
public record GetOverdueInvoicesQuery : IRequest<List<OverdueInvoiceDto>>;
