using MediatR;
using BatbyEducation.Application.DTOs;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;

namespace BatbyEducation.Application.Queries;

/// <summary>
/// Handles the GetOutstandingBalancesQuery by computing per-student balances
/// from unpaid/partially paid invoices minus account credits.
/// </summary>
public class GetOutstandingBalancesQueryHandler : IRequestHandler<GetOutstandingBalancesQuery, List<StudentBalanceDto>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IStudentAccountRepository _studentAccountRepository;

    public GetOutstandingBalancesQueryHandler(
        IStudentRepository studentRepository,
        IInvoiceRepository invoiceRepository,
        IStudentAccountRepository studentAccountRepository)
    {
        _studentRepository = studentRepository;
        _invoiceRepository = invoiceRepository;
        _studentAccountRepository = studentAccountRepository;
    }

    public async Task<List<StudentBalanceDto>> Handle(GetOutstandingBalancesQuery request, CancellationToken cancellationToken)
    {
        var students = await _studentRepository.GetAllAsync();
        var results = new List<StudentBalanceDto>();

        foreach (var student in students)
        {
            var invoices = await _invoiceRepository.GetByStudentAsync(student.Id);

            // Filter to unpaid/partially paid/overdue invoices
            var outstandingInvoices = invoices
                .Where(i => i.Status == InvoiceStatus.Issued
                         || i.Status == InvoiceStatus.PartiallyPaid
                         || i.Status == InvoiceStatus.Overdue)
                .ToList();

            if (outstandingInvoices.Count == 0)
            {
                // Check if student has credit balance worth reporting
                var accountForZero = await _studentAccountRepository.GetByStudentIdAsync(student.Id);
                var creditForZero = accountForZero?.CreditBalance ?? 0m;

                if (creditForZero > 0)
                {
                    results.Add(new StudentBalanceDto(
                        student.Name,
                        TotalOutstanding: 0m,
                        UnpaidInvoiceCount: 0,
                        CreditBalance: creditForZero));
                }

                continue;
            }

            // Sum outstanding amounts across invoices
            var totalOutstanding = outstandingInvoices.Sum(i => i.GetOutstandingBalance());

            // Get student credit balance
            var account = await _studentAccountRepository.GetByStudentIdAsync(student.Id);
            var creditBalance = account?.CreditBalance ?? 0m;

            // Net outstanding = total invoice outstanding minus credits
            var netOutstanding = totalOutstanding - creditBalance;
            if (netOutstanding < 0) netOutstanding = 0m;

            results.Add(new StudentBalanceDto(
                student.Name,
                TotalOutstanding: netOutstanding,
                UnpaidInvoiceCount: outstandingInvoices.Count,
                CreditBalance: creditBalance));
        }

        return results;
    }
}
