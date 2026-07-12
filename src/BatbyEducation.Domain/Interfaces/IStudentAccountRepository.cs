using BatbyEducation.Domain.Entities;

namespace BatbyEducation.Domain.Interfaces;

public interface IStudentAccountRepository
{
    Task<StudentAccount?> GetByStudentIdAsync(Guid studentId);
    Task AddAsync(StudentAccount account);
    Task UpdateAsync(StudentAccount account);
}
