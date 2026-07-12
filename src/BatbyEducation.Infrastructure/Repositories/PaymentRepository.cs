using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;
using BatbyEducation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BatbyEducation.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly BatbyEducationDbContext _context;

    public PaymentRepository(BatbyEducationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Payment>> GetAllAsync()
    {
        return await _context.Payments
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Payment>> GetByInvoiceAsync(Guid invoiceId)
    {
        return await _context.Payments
            .Where(p => p.InvoiceId == invoiceId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Payment>> GetByDateRangeAndMethodAsync(DateOnly start, DateOnly end, PaymentMethod method)
    {
        return await _context.Payments
            .Where(p => p.PaymentDate >= start
                && p.PaymentDate <= end
                && p.Method == method)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Payment>> GetByDateRangeAsync(DateOnly start, DateOnly end)
    {
        return await _context.Payments
            .Where(p => p.PaymentDate >= start && p.PaymentDate <= end)
            .ToListAsync();
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Payment payment)
    {
        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();
    }
}
