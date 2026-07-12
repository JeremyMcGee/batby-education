using MediatR;
using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Enumerations;

namespace BatbyEducation.Application.Commands.Payments;

/// <summary>
/// Command to record a payment against an invoice and post to the appropriate ledger.
/// </summary>
public record RecordPaymentCommand(
    Guid InvoiceId,
    decimal Amount,
    DateOnly PaymentDate,
    PaymentMethod Method) : IRequest<Result<Guid>>;
