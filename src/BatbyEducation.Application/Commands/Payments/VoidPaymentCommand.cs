using BatbyEducation.Domain.Common;
using MediatR;

namespace BatbyEducation.Application.Commands.Payments;

public record VoidPaymentCommand(Guid PaymentId) : IRequest<Result<bool>>;
