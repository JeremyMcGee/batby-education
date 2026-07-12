using BatbyEducation.Domain.Common;
using MediatR;

namespace BatbyEducation.Application.Commands.Students;

public record RegisterStudentCommand(
    string Name,
    string Email,
    string PhoneNumber,
    string GuardianName,
    string GuardianEmail,
    decimal? HourlyRate = null,
    Guid? DefaultTutorId = null,
    string? DefaultSubject = null,
    DayOfWeek? DefaultDay = null,
    TimeOnly? DefaultStartTime = null) : IRequest<Result<Guid>>;
