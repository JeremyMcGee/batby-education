using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Interfaces;
using MediatR;

namespace BatbyEducation.Application.Commands.Tutors;

public class RegisterTutorCommandHandler : IRequestHandler<RegisterTutorCommand, Result<Guid>>
{
    private readonly ITutorRepository _tutorRepository;

    public RegisterTutorCommandHandler(ITutorRepository tutorRepository)
    {
        _tutorRepository = tutorRepository;
    }

    public async Task<Result<Guid>> Handle(RegisterTutorCommand request, CancellationToken cancellationToken)
    {
        var existingTutor = await _tutorRepository.GetByEmailAsync(request.Email);
        if (existingTutor is not null)
        {
            return Result<Guid>.Failure("Email", "A tutor with this email already exists");
        }

        var result = Tutor.Create(
            request.Name,
            request.Email,
            request.Subjects,
            request.HourlyRate);

        if (!result.IsSuccess)
        {
            return Result<Guid>.Failure(result.Errors);
        }

        await _tutorRepository.AddAsync(result.Value!);

        return Result<Guid>.Success(result.Value!.Id);
    }
}
