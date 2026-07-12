using BatbyEducation.Domain.Entities;

namespace BatbyEducation.Domain.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id);
    Task<Student?> GetByEmailAsync(string email);
    Task<IReadOnlyList<Student>> GetAllAsync();
    Task AddAsync(Student student);
    Task UpdateAsync(Student student);
}
