using BatbyEducation.Domain.Entities;

namespace BatbyEducation.Domain.Interfaces;

public interface ITutorAvailabilityRepository
{
    Task<IReadOnlyList<TutorAvailability>> GetByTutorIdAsync(Guid tutorId);
    Task<IReadOnlyList<TutorAvailability>> GetByTutorAndDayAsync(Guid tutorId, DayOfWeek day);
    Task<TutorAvailability?> GetByTutorAndDateAsync(Guid tutorId, DateOnly date);
    Task AddAsync(TutorAvailability availability);
    Task AddRangeAsync(IEnumerable<TutorAvailability> availabilities);
    Task RemoveByTutorIdAsync(Guid tutorId);
    Task UpdateAsync(TutorAvailability availability);
}
