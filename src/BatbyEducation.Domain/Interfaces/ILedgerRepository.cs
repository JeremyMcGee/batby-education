using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Enumerations;

namespace BatbyEducation.Domain.Interfaces;

public interface ILedgerRepository
{
    Task<IReadOnlyList<LedgerEntry>> GetEntriesAsync(PaymentMethod method, DateOnly start, DateOnly end);
    Task<decimal> GetTotalAsync(PaymentMethod method, DateOnly start, DateOnly end);
    Task<int> GetTransactionCountAsync(PaymentMethod method, DateOnly start, DateOnly end);
    Task AddAsync(LedgerEntry entry);
    Task DeleteByPaymentIdAsync(Guid paymentId);
}
