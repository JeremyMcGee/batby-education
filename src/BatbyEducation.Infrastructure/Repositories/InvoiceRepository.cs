using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;
using BatbyEducation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BatbyEducation.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly BatbyEducationDbContext _context;

    public InvoiceRepository(BatbyEducationDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IReadOnlyList<Invoice>> GetAllAsync()
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .OrderByDescending(i => i.IssuedAt)
            .ToListAsync();
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<IReadOnlyList<Invoice>> GetByStudentAsync(Guid studentId)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Where(i => i.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Invoice>> GetOverdueAsync()
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Where(i => i.Status == InvoiceStatus.Overdue)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Invoice>> GetByDateRangeAsync(DateOnly start, DateOnly end)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Where(i => DateOnly.FromDateTime(i.IssuedAt) >= start
                && DateOnly.FromDateTime(i.IssuedAt) <= end)
            .ToListAsync();
    }

    public async Task AddAsync(Invoice invoice)
    {
        await _context.Invoices.AddAsync(invoice);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
    }
}
