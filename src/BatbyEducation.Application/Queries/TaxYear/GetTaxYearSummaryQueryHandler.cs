using MediatR;
using BatbyEducation.Application.DTOs;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;
using BatbyEducation.Domain.ValueObjects;

namespace BatbyEducation.Application.Queries.TaxYear;

/// <summary>
/// Handles the GetTaxYearSummaryQuery by calculating totals across the full tax year date range
/// (6 April Y to 5 April Y+1) and including a month-by-month breakdown.
/// </summary>
public class GetTaxYearSummaryQueryHandler : IRequestHandler<GetTaxYearSummaryQuery, TaxYearSummaryDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILedgerRepository _ledgerRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IStudentAccountRepository _studentAccountRepository;
    private readonly IMediator _mediator;

    public GetTaxYearSummaryQueryHandler(
        IPaymentRepository paymentRepository,
        ILedgerRepository ledgerRepository,
        ISessionRepository sessionRepository,
        IInvoiceRepository invoiceRepository,
        IStudentAccountRepository studentAccountRepository,
        IMediator mediator)
    {
        _paymentRepository = paymentRepository;
        _ledgerRepository = ledgerRepository;
        _sessionRepository = sessionRepository;
        _invoiceRepository = invoiceRepository;
        _studentAccountRepository = studentAccountRepository;
        _mediator = mediator;
    }

    public async Task<TaxYearSummaryDto> Handle(GetTaxYearSummaryQuery request, CancellationToken cancellationToken)
    {
        var taxYear = DateRange.TaxYear(request.TaxYearStartYear);
        var start = taxYear.Start;
        var end = taxYear.End;

        // Total income = sum of all payments in the tax year date range
        var payments = await _paymentRepository.GetByDateRangeAsync(start, end);
        var totalIncome = payments.Sum(p => p.Amount);

        // Cash/bank split via ledger repository
        var cashTotal = await _ledgerRepository.GetTotalAsync(PaymentMethod.Cash, start, end);
        var bankTransferTotal = await _ledgerRepository.GetTotalAsync(PaymentMethod.BankTransfer, start, end);

        // Sessions and hours from session repository
        var sessions = await _sessionRepository.GetByDateRangeAsync(start, end);
        var completedSessions = sessions.Where(s => s.Status == SessionStatus.Completed).ToList();
        var totalCompletedSessions = completedSessions.Count;
        var totalHoursTutored = completedSessions
            .Sum(s => (s.ActualDurationMinutes ?? s.ScheduledDurationMinutes) / 60m);

        // Total invoiced amount for invoices issued within the tax year
        var invoices = await _invoiceRepository.GetByDateRangeAsync(start, end);
        var totalInvoicedAmount = invoices.Sum(inv => inv.TotalAmount);

        // Outstanding receivables = unpaid/partially paid invoices minus credits
        var unpaidInvoices = invoices
            .Where(inv => inv.Status == InvoiceStatus.Issued || inv.Status == InvoiceStatus.PartiallyPaid)
            .ToList();
        var outstandingReceivables = unpaidInvoices.Sum(inv => inv.GetOutstandingBalance());

        // Monthly breakdown via the GetMonthlySummaryQuery
        var monthlyBreakdown = await _mediator.Send(
            new GetMonthlySummaryQuery(request.TaxYearStartYear), cancellationToken);

        return new TaxYearSummaryDto(
            TotalIncome: totalIncome,
            CashLedgerTotal: cashTotal,
            BankTransferLedgerTotal: bankTransferTotal,
            TotalCompletedSessions: totalCompletedSessions,
            TotalHoursTutored: totalHoursTutored,
            TotalInvoicedAmount: totalInvoicedAmount,
            OutstandingReceivables: outstandingReceivables,
            MonthlyBreakdown: monthlyBreakdown);
    }
}
