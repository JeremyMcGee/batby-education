using MediatR;
using BatbyEducation.Application.DTOs;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;

namespace BatbyEducation.Application.Queries;

/// <summary>
/// Handles the GetOverdueInvoicesQuery by finding invoices where status is
/// Issued/PartiallyPaid and more than 30 days have elapsed since issue date.
/// </summary>
public class GetOverdueInvoicesQueryHandler : IRequestHandler<GetOverdueInvoicesQuery, List<OverdueInvoiceDto>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public GetOverdueInvoicesQueryHandler(
        IStudentRepository studentRepository,
        IInvoiceRepository invoiceRepository)
    {
        _studentRepository = studentRepository;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<List<OverdueInvoiceDto>> Handle(GetOverdueInvoicesQuery request, CancellationToken cancellationToken)
    {
        var students = await _studentRepository.GetAllAsync();
        var results = new List<OverdueInvoiceDto>();

        foreach (var student in students)
        {
            var invoices = await _invoiceRepository.GetByStudentAsync(student.Id);

            var overdueInvoices = invoices
                .Where(i => (i.Status == InvoiceStatus.Created
                          || i.Status == InvoiceStatus.PartiallyPaid
                          || i.Status == InvoiceStatus.Overdue)
                            && (DateTime.UtcNow - i.IssuedAt).Days > 30)
                .ToList();

            foreach (var invoice in overdueInvoices)
            {
                var daysOverdue = (DateTime.UtcNow - invoice.IssuedAt).Days;

                results.Add(new OverdueInvoiceDto(
                    StudentName: student.Name,
                    GuardianName: student.GuardianName,
                    InvoiceNumber: invoice.InvoiceNumber,
                    AmountOutstanding: invoice.GetOutstandingBalance(),
                    DaysOverdue: daysOverdue));
            }
        }

        // Sort by days overdue descending
        return results.OrderByDescending(r => r.DaysOverdue).ToList();
    }
}
