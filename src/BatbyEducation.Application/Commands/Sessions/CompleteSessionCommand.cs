using BatbyEducation.Domain.Common;
using MediatR;

namespace BatbyEducation.Application.Commands.Sessions;

public record CompleteSessionCommand(Guid SessionId, int? ActualDurationMinutes) : IRequest<Result<bool>>;
