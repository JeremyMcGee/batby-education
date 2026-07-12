using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Interfaces;
using BatbyEducation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BatbyEducation.Infrastructure.Repositories;

public class TutorAvailabilityRepository : ITutorAvailabilityRepository
{
    private readonly BatbyEducationDbContext _context;

    public TutorAvailabilityRepository(BatbyEducationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TutorAvailability>> GetByTutorIdAsync(Guid tutorId)
    {
        return await _context.TutorAvailabilities
            .Where(ta => ta.TutorId == tutorId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TutorAvailability>> GetByTutorAndDayAsync(Guid tutorId, DayOfWeek day)
    {
        return await _context.TutorAvailabilities
            .Where(ta => ta.TutorId == tutorId
                && ta.DayOfWeek == day
                && ta.SpecificDate == null)
            .ToListAsync();
    }

    public async Task<TutorAvailability?> GetByTutorAndDateAsync(Guid tutorId, DateOnly date)
    {
        return await _context.TutorAvailabilities
            .FirstOrDefaultAsync(ta => ta.TutorId == tutorId
                && ta.SpecificDate == date);
    }

    public async Task AddAsync(TutorAvailability availability)
    {
        await _context.TutorAvailabilities.AddAsync(availability);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<TutorAvailability> availabilities)
    {
        await _context.TutorAvailabilities.AddRangeAsync(availabilities);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveByTutorIdAsync(Guid tutorId)
    {
        var entries = await _context.TutorAvailabilities
            .Where(ta => ta.TutorId == tutorId)
            .ToListAsync();

        _context.TutorAvailabilities.RemoveRange(entries);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TutorAvailability availability)
    {
        _context.TutorAvailabilities.Update(availability);
        await _context.SaveChangesAsync();
    }
}
