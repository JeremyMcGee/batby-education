using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Enumerations;

namespace BatbyEducation.Domain.Interfaces;

public interface IPaymentRepository
{
    Task<IReadOnlyList<Payment>> GetAllAsync();
    Task<Payment?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Payment>> GetByInvoiceAsync(Guid invoiceId);
    Task<IReadOnlyList<Payment>> GetByDateRangeAndMethodAsync(DateOnly start, DateOnly end, PaymentMethod method);
    Task<IReadOnlyList<Payment>> GetByDateRangeAsync(DateOnly start, DateOnly end);
    Task AddAsync(Payment payment);
    Task DeleteAsync(Payment payment);
}
