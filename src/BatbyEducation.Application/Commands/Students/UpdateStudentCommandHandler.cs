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
            hourlyRate: request.HourlyRate);

        if (!result.IsSuccess)
        {
            return Result<Guid>.Failure(result.Errors);
        }

        // Save audit entries created during the update
        if (student.AuditHistory.Count > 0)
        {
            await _auditEntryRepository.AddRangeAsync(student.AuditHistory);
        }

        await _studentRepository.UpdateAsync(student);

        return Result<Guid>.Success(student.Id);
    }
}
