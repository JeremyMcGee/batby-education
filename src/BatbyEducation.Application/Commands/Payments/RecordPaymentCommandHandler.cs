using MediatR;
using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;

namespace BatbyEducation.Application.Commands.Payments;

/// <summary>
/// Handles the RecordPaymentCommand by validating the invoice state, creating the payment,
/// updating invoice status, posting to the appropriate ledger, and handling overpayment credits.
/// </summary>
public class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, Result<Guid>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILedgerRepository _ledgerRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentAccountRepository _studentAccountRepository;

    public RecordPaymentCommandHandler(
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository,
        ILedgerRepository ledgerRepository,
        IStudentRepository studentRepository,
        IStudentAccountRepository studentAccountRepository)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _ledgerRepository = ledgerRepository;
        _studentRepository = studentRepository;
        _studentAccountRepository = studentAccountRepository;
    }

    public async Task<Result<Guid>> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        // 1. Load invoice
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);
        if (invoice is null)
        {
            return Result<Guid>.Failure("InvoiceId", "Invoice not found.");
        }

        // 2. Validate invoice status — not Paid or Cancelled
        if (invoice.Status == InvoiceStatus.Paid || invoice.Status == InvoiceStatus.Cancelled)
        {
            return Result<Guid>.Failure("InvoiceId", "Invoice is not eligible for payment.");
        }

        // 3. Create Payment via domain factory method
        var paymentResult = Payment.Create(
            request.InvoiceId,
            request.Amount,
            request.PaymentDate,
            request.Method);

        if (!paymentResult.IsSuccess)
        {
            return Result<Guid>.Failure(paymentResult.Errors);
        }

        var payment = paymentResult.Value!;

        // 4. Save payment
        await _paymentRepository.AddAsync(payment);

        // 5. Record payment on invoice — returns overpayment amount
        var overpayment = invoice.RecordPayment(request.Amount);

        // 6. Handle overpayment — credit to student account
        if (overpayment > 0)
        {
            var studentAccount = await _studentAccountRepository.GetByStudentIdAsync(invoice.StudentId);
            if (studentAccount is null)
            {
                studentAccount = StudentAccount.Create(invoice.StudentId);
                studentAccount.AddCredit(overpayment);
                await _studentAccountRepository.AddAsync(studentAccount);
            }
            else
            {
                studentAccount.AddCredit(overpayment);
                await _studentAccountRepository.UpdateAsync(studentAccount);
            }
        }

        // 7. Post to ledger — load student name for the ledger entry
        var student = await _studentRepository.GetByIdAsync(invoice.StudentId);
        var studentName = student?.Name ?? "Unknown";

        var ledgerEntry = LedgerEntry.Create(
            payment.Id,
            request.Method,
            request.Amount,
            request.PaymentDate,
            invoice.InvoiceNumber,
            studentName);

        await _ledgerRepository.AddAsync(ledgerEntry);

        // 8. Save invoice (status may have changed)
        await _invoiceRepository.UpdateAsync(invoice);

        // 9. Return success with payment ID
        return Result<Guid>.Success(payment.Id);
    }
}
