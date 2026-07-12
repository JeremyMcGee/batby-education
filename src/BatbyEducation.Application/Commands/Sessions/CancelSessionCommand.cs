using BatbyEducation.Domain.Common;
using MediatR;

namespace BatbyEducation.Application.Commands.Sessions;

public record CancelSessionCommand(Guid SessionId) : IRequest<Result<bool>>;
