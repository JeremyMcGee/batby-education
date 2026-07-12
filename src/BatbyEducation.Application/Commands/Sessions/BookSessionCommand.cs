using MediatR;
using BatbyEducation.Domain.Common;

namespace BatbyEducation.Application.Commands.Sessions;

/// <summary>
/// Command to book a new tutoring session.
/// </summary>
public record BookSessionCommand(
    Guid TutorId,
    Guid StudentId,
    string Subject,
    DateOnly SessionDate,
    TimeOnly StartTime,
    int DurationMinutes,
    decimal? RateOverride = null) : IRequest<Result<Guid>>;
