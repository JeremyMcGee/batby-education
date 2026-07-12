using BatbyEducation.Domain.Entities;

namespace BatbyEducation.Domain.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<IReadOnlyList<Invoice>> GetAllAsync();
    Task<IReadOnlyList<Invoice>> GetByStudentAsync(Guid studentId);
    Task<IReadOnlyList<Invoice>> GetOverdueAsync();
    Task<IReadOnlyList<Invoice>> GetByDateRangeAsync(DateOnly start, DateOnly end);
    Task AddAsync(Invoice invoice);
    Task UpdateAsync(Invoice invoice);
}
