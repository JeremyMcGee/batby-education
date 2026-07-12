using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Interfaces;
using MediatR;

namespace BatbyEducation.Application.Commands.Payments;

public class VoidPaymentCommandHandler : IRequestHandler<VoidPaymentCommand, Result<bool>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILedgerRepository _ledgerRepository;
    private readonly IStudentAccountRepository _studentAccountRepository;

    public VoidPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IInvoiceRepository invoiceRepository,
        ILedgerRepository ledgerRepository,
        IStudentAccountRepository studentAccountRepository)
    {
        _paymentRepository = paymentRepository;
        _invoiceRepository = invoiceRepository;
        _ledgerRepository = ledgerRepository;
        _studentAccountRepository = studentAccountRepository;
    }

    public async Task<Result<bool>> Handle(VoidPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId);
        if (payment is null)
            return Result<bool>.Failure("PaymentId", "Payment not found");

        var invoice = await _invoiceRepository.GetByIdAsync(payment.InvoiceId);
        if (invoice is null)
            return Result<bool>.Failure("InvoiceId", "Associated invoice not found");

        // Reverse the payment on the invoice
        invoice.ReversePayment(payment.Amount);
        await _invoiceRepository.UpdateAsync(invoice);

        // Remove the ledger entry
        await _ledgerRepository.DeleteByPaymentIdAsync(payment.Id);

        // Delete the payment
        await _paymentRepository.DeleteAsync(payment);

        return Result<bool>.Success(true);
    }
}
