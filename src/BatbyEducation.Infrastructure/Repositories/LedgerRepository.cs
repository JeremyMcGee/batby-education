using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;
using BatbyEducation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BatbyEducation.Infrastructure.Repositories;

public class LedgerRepository : ILedgerRepository
{
    private readonly BatbyEducationDbContext _context;

    public LedgerRepository(BatbyEducationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LedgerEntry>> GetEntriesAsync(PaymentMethod method, DateOnly start, DateOnly end)
    {
        return await _context.LedgerEntries
            .Where(e => e.LedgerType == method
                && e.EntryDate >= start
                && e.EntryDate <= end)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalAsync(PaymentMethod method, DateOnly start, DateOnly end)
    {
        var entries = await _context.LedgerEntries
            .Where(e => e.LedgerType == method
                && e.EntryDate >= start
                && e.EntryDate <= end)
            .ToListAsync();

        return entries.Sum(e => e.Amount);
    }

    public async Task<int> GetTransactionCountAsync(PaymentMethod method, DateOnly start, DateOnly end)
    {
        return await _context.LedgerEntries
            .Where(e => e.LedgerType == method
                && e.EntryDate >= start
                && e.EntryDate <= end)
            .CountAsync();
    }

    public async Task AddAsync(LedgerEntry entry)
    {
        await _context.LedgerEntries.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByPaymentIdAsync(Guid paymentId)
    {
        var entries = await _context.LedgerEntries.Where(e => e.PaymentId == paymentId).ToListAsync();
        _context.LedgerEntries.RemoveRange(entries);
        await _context.SaveChangesAsync();
    }
}
