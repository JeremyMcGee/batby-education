using BatbyEducation.Domain.Common;
using MediatR;

namespace BatbyEducation.Application.Commands.Students;

public record UpdateStudentCommand(
    Guid StudentId,
    string Name,
    string Email,
    string PhoneNumber,
    string GuardianName,
    string GuardianEmail,
    decimal? HourlyRate = null) : IRequest<Result<Guid>>;
