using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Interfaces;
using BatbyEducation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BatbyEducation.Infrastructure.Repositories;

public class AuditEntryRepository : IAuditEntryRepository
{
    private readonly BatbyEducationDbContext _context;

    public AuditEntryRepository(BatbyEducationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AuditEntry>> GetByEntityIdAsync(Guid entityId)
    {
        return await _context.AuditEntries
            .Where(a => a.EntityId == entityId)
            .OrderByDescending(a => a.ChangedAt)
            .ToListAsync();
    }

    public async Task AddAsync(AuditEntry entry)
    {
        await _context.AuditEntries.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<AuditEntry> entries)
    {
        await _context.AuditEntries.AddRangeAsync(entries);
        await _context.SaveChangesAsync();
    }
}
