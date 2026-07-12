using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;
using BatbyEducation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BatbyEducation.Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly BatbyEducationDbContext _context;

    public SessionRepository(BatbyEducationDbContext context)
    {
        _context = context;
    }

    public async Task<Session?> GetByIdAsync(Guid id)
    {
        return await _context.Sessions.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IReadOnlyList<Session>> GetByDateRangeAsync(DateOnly start, DateOnly end)
    {
        return await _context.Sessions
            .Where(s => s.SessionDate >= start && s.SessionDate <= end)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Session>> GetByTutorAndDateRangeAsync(Guid tutorId, DateOnly start, DateOnly end)
    {
        return await _context.Sessions
            .Where(s => s.TutorId == tutorId && s.SessionDate >= start && s.SessionDate <= end)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Session>> GetByStudentAndDateRangeAsync(Guid studentId, DateOnly start, DateOnly end)
    {
        return await _context.Sessions
            .Where(s => s.StudentId == studentId && s.SessionDate >= start && s.SessionDate <= end)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Session>> GetConflictingSessionsAsync(Guid tutorId, DateTime start, DateTime end)
    {
        var startDate = DateOnly.FromDateTime(start);
        var endDate = DateOnly.FromDateTime(end);

        // Load candidate sessions for the tutor on the relevant dates with active statuses
        var candidates = await _context.Sessions
            .Where(s => s.TutorId == tutorId
                && s.SessionDate >= startDate
                && s.SessionDate <= endDate
                && (s.Status == SessionStatus.Scheduled || s.Status == SessionStatus.PendingConfirmation))
            .ToListAsync();

        // Filter in memory for time overlap
        return candidates
            .Where(s =>
            {
                var sessionStart = s.SessionDate.ToDateTime(s.StartTime);
                var sessionEnd = sessionStart.AddMinutes(s.ScheduledDurationMinutes);
                return sessionStart < end && sessionEnd > start;
            })
            .ToList();
    }

    public async Task<IReadOnlyList<Session>> GetStudentConflictsAsync(Guid studentId, DateTime start, DateTime end)
    {
        var startDate = DateOnly.FromDateTime(start);
        var endDate = DateOnly.FromDateTime(end);

        // Load candidate sessions for the student on the relevant dates with active statuses
        var candidates = await _context.Sessions
            .Where(s => s.StudentId == studentId
                && s.SessionDate >= startDate
                && s.SessionDate <= endDate
                && (s.Status == SessionStatus.Scheduled || s.Status == SessionStatus.PendingConfirmation))
            .ToListAsync();

        // Filter in memory for time overlap
        return candidates
            .Where(s =>
            {
                var sessionStart = s.SessionDate.ToDateTime(s.StartTime);
                var sessionEnd = sessionStart.AddMinutes(s.ScheduledDurationMinutes);
                return sessionStart < end && sessionEnd > start;
            })
            .ToList();
    }

    public async Task AddAsync(Session session)
    {
        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Session session)
    {
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();
    }
}
