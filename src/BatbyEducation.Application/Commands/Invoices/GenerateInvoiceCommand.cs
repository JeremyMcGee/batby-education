using MediatR;
using BatbyEducation.Domain.Common;

namespace BatbyEducation.Application.Commands.Invoices;

/// <summary>
/// Command to generate an invoice for a student's billable sessions within a billing period.
/// </summary>
public record GenerateInvoiceCommand(
    Guid StudentId,
    DateOnly BillingPeriodStart,
    DateOnly BillingPeriodEnd,
    decimal LateCancellationFee = 25.00m) : IRequest<Result<Guid>>;
