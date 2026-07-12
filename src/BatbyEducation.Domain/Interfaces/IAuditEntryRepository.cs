using BatbyEducation.Domain.Entities;

namespace BatbyEducation.Domain.Interfaces;

public interface IAuditEntryRepository
{
    Task<IReadOnlyList<AuditEntry>> GetByEntityIdAsync(Guid entityId);
    Task AddAsync(AuditEntry entry);
    Task AddRangeAsync(IEnumerable<AuditEntry> entries);
}
