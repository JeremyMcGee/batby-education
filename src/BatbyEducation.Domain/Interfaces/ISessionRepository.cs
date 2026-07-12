using BatbyEducation.Domain.Entities;

namespace BatbyEducation.Domain.Interfaces;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Session>> GetByDateRangeAsync(DateOnly start, DateOnly end);
    Task<IReadOnlyList<Session>> GetByTutorAndDateRangeAsync(Guid tutorId, DateOnly start, DateOnly end);
    Task<IReadOnlyList<Session>> GetByStudentAndDateRangeAsync(Guid studentId, DateOnly start, DateOnly end);
    Task<IReadOnlyList<Session>> GetConflictingSessionsAsync(Guid tutorId, DateTime start, DateTime end);
    Task<IReadOnlyList<Session>> GetStudentConflictsAsync(Guid studentId, DateTime start, DateTime end);
    Task AddAsync(Session session);
    Task UpdateAsync(Session session);
}
