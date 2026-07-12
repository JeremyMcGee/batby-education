using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Interfaces;
using MediatR;

namespace BatbyEducation.Application.Commands.Students;

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, Result<Guid>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAuditEntryRepository _auditEntryRepository;

    public UpdateStudentCommandHandler(
        IStudentRepository studentRepository,
        IAuditEntryRepository auditEntryRepository)
    {
        _studentRepository = studentRepository;
        _auditEntryRepository = auditEntryRepository;
    }

    public async Task<Result<Guid>> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student is null)
        {
            return Result<Guid>.Failure("StudentId", "Student not found");
        }

        var result = student.Update(
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

        // EF Core will cascade-insert new audit entries via the navigation property
        await _studentRepository.UpdateAsync(student);

        return Result<Guid>.Success(student.Id);
    }
}
