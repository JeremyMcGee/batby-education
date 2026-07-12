using BatbyEducation.Domain.Entities;

namespace BatbyEducation.Domain.Interfaces;

public interface ITutorRepository
{
    Task<Tutor?> GetByIdAsync(Guid id);
    Task<Tutor?> GetByEmailAsync(string email);
    Task<IReadOnlyList<Tutor>> GetAllAsync();
    Task AddAsync(Tutor tutor);
    Task UpdateAsync(Tutor tutor);
}
