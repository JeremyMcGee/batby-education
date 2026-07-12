using MediatR;
using BatbyEducation.Application.DTOs;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;

namespace BatbyEducation.Application.Queries;

/// <summary>
/// Handles the GetRevenueReportQuery by aggregating invoice and payment data for the specified date range.
/// </summary>
public class GetRevenueReportQueryHandler : IRequestHandler<GetRevenueReportQuery, RevenueReportDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;

    public GetRevenueReportQueryHandler(
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<RevenueReportDto> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        // Sum invoices issued within range
        var invoices = await _invoiceRepository.GetByDateRangeAsync(request.StartDate, request.EndDate);
        var totalInvoiced = invoices.Sum(i => i.TotalAmount);

        // Calculate outstanding balance (unpaid and partially paid invoices within the range)
        var totalOutstanding = invoices
            .Where(i => i.Status == InvoiceStatus.Created || i.Status == InvoiceStatus.PartiallyPaid || i.Status == InvoiceStatus.Overdue)
            .Sum(i => i.GetOutstandingBalance());

        // Sum payments within range
        var payments = await _paymentRepository.GetByDateRangeAsync(request.StartDate, request.EndDate);
        var totalPaymentsReceived = payments.Sum(p => p.Amount);

        // Break down by payment method
        var cashReceipts = payments
            .Where(p => p.Method == PaymentMethod.Cash)
            .Sum(p => p.Amount);

        var bankTransferReceipts = payments
            .Where(p => p.Method == PaymentMethod.BankTransfer)
            .Sum(p => p.Amount);

        return new RevenueReportDto(
            totalInvoiced,
            totalPaymentsReceived,
            totalOutstanding,
            cashReceipts,
            bankTransferReceipts);
    }
}
