using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Interfaces;
using MediatR;

namespace BatbyEducation.Application.Commands.Students;

public class RegisterStudentCommandHandler : IRequestHandler<RegisterStudentCommand, Result<Guid>>
{
    private readonly IStudentRepository _studentRepository;

    public RegisterStudentCommandHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Result<Guid>> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
    {
        var existingStudent = await _studentRepository.GetByEmailAsync(request.Email);
        if (existingStudent is not null)
        {
            return Result<Guid>.Failure("Email", "A student with this email already exists");
        }

        var result = Student.Create(
            request.Name,
            request.Email,
            request.PhoneNumber,
            request.GuardianName,
            request.GuardianEmail,
            hourlyRate: request.HourlyRate,
            defaultTutorId: request.DefaultTutorId,
            defaultSubject: request.DefaultSubject,
            defaultDay: request.DefaultDay,
            defaultStartTime: request.DefaultStartTime);

        if (!result.IsSuccess)
        {
            return Result<Guid>.Failure(result.Errors);
        }

        await _studentRepository.AddAsync(result.Value!);

        return Result<Guid>.Success(result.Value!.Id);
    }
}
