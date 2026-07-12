using BatbyEducation.Domain.Common;
using MediatR;

namespace BatbyEducation.Application.Commands.Sessions;

public record RescheduleSessionCommand(
    Guid OriginalSessionId,
    DateOnly NewDate,
    TimeOnly NewStartTime,
    int NewDurationMinutes) : IRequest<Result<Guid>>;
