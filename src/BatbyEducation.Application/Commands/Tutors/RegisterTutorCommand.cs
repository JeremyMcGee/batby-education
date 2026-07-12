using BatbyEducation.Domain.Common;
using MediatR;

namespace BatbyEducation.Application.Commands.Tutors;

public record RegisterTutorCommand(
    string Name,
    string Email,
    List<string> Subjects,
    decimal HourlyRate) : IRequest<Result<Guid>>;
