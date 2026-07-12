using MediatR;
using BatbyEducation.Application.DTOs;
using BatbyEducation.Domain.Interfaces;

namespace BatbyEducation.Application.Queries.TaxYear;

/// <summary>
/// Handles the GetMonthlySummaryQuery by calculating invoiced revenue, collected revenue,
/// and outstanding amounts for each of the 12 UK tax year months (6th to 5th boundaries).
/// </summary>
public class GetMonthlySummaryQueryHandler : IRequestHandler<GetMonthlySummaryQuery, List<MonthlySummaryDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;

    public GetMonthlySummaryQueryHandler(
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<List<MonthlySummaryDto>> Handle(GetMonthlySummaryQuery request, CancellationToken cancellationToken)
    {
        var months = GetTaxYearMonths(request.TaxYearStartYear);
        var result = new List<MonthlySummaryDto>();

        for (int i = 0; i < months.Count; i++)
        {
            var (start, end) = months[i];

            // Invoiced revenue: sum of TotalAmount for invoices issued within the month
            var invoices = await _invoiceRepository.GetByDateRangeAsync(start, end);
            var invoicedRevenue = invoices.Sum(inv => inv.TotalAmount);

            // Collected revenue: sum of payments received within the month
            var payments = await _paymentRepository.GetByDateRangeAsync(start, end);
            var collectedRevenue = payments.Sum(p => p.Amount);

            // Outstanding: invoiced minus collected for this period
            var outstandingAmount = invoicedRevenue - collectedRevenue;
            if (outstandingAmount < 0) outstandingAmount = 0;

            result.Add(new MonthlySummaryDto(
                Month: i + 1,
                StartDate: start,
                EndDate: end,
                InvoicedRevenue: invoicedRevenue,
                CollectedRevenue: collectedRevenue,
                OutstandingAmount: outstandingAmount));
        }

        return result;
    }

    /// <summary>
    /// Generates the 12 tax year month boundaries (6th to 5th) for a given start year.
    /// Month 1 = 6 April – 5 May, Month 2 = 6 May – 5 June, etc.
    /// </summary>
    internal static List<(DateOnly Start, DateOnly End)> GetTaxYearMonths(int startYear)
    {
        var months = new List<(DateOnly, DateOnly)>();
        for (int i = 0; i < 12; i++)
        {
            var monthStart = new DateOnly(startYear, 4, 6).AddMonths(i);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            months.Add((monthStart, monthEnd));
        }
        return months;
    }
}
