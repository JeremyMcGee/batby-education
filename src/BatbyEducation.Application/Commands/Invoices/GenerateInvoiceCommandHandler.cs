using MediatR;
using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Entities;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;

namespace BatbyEducation.Application.Commands.Invoices;

/// <summary>
/// Handles the GenerateInvoiceCommand by querying billable sessions,
/// calculating line item amounts, and creating an invoice.
/// </summary>
public class GenerateInvoiceCommandHandler : IRequestHandler<GenerateInvoiceCommand, Result<Guid>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ITutorRepository _tutorRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public GenerateInvoiceCommandHandler(
        IStudentRepository studentRepository,
        ISessionRepository sessionRepository,
        ITutorRepository tutorRepository,
        IInvoiceRepository invoiceRepository)
    {
        _studentRepository = studentRepository;
        _sessionRepository = sessionRepository;
        _tutorRepository = tutorRepository;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result<Guid>> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify student exists
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student is null)
        {
            return Result<Guid>.Failure("StudentId", "Student not found.");
        }

        // 2. Query sessions for the student in the billing period
        var sessions = await _sessionRepository.GetByStudentAndDateRangeAsync(
            request.StudentId, request.BillingPeriodStart, request.BillingPeriodEnd);

        // 3. Filter to only Completed or late-cancelled sessions
        var billableSessions = sessions
            .Where(s => s.Status == SessionStatus.Completed || s.IsLateCancellation)
            .ToList();

        // 4. If no billable sessions found, return failure
        if (billableSessions.Count == 0)
        {
            return Result<Guid>.Failure("BillingPeriod", "No billable sessions found for the specified period.");
        }

        // 5. Generate unique invoice number
        var invoiceNumber = $"INV-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        // 6. Create Invoice entity
        var invoice = Invoice.Create(
            invoiceNumber,
            request.StudentId,
            request.BillingPeriodStart,
            request.BillingPeriodEnd);

        // 7. Add line items for each billable session
        foreach (var session in billableSessions)
        {
            var tutor = await _tutorRepository.GetByIdAsync(session.TutorId);
            var tutorName = tutor?.Name ?? "Unknown Tutor";

            if (session.IsLateCancellation)
            {
                // Late cancellation: use the flat fee
                invoice.AddLineItem(
                    session.Id,
                    session.SessionDate,
                    tutorName,
                    session.Subject,
                    session.ScheduledDurationMinutes,
                    request.LateCancellationFee,
                    request.LateCancellationFee,
                    isLateCancellationFee: true);
            }
            else
            {
                // Completed session: Effective_Rate = session.RateOverride ?? student.HourlyRate ?? tutor.HourlyRate ?? 0
                var effectiveRate = session.RateOverride ?? student.HourlyRate ?? tutor?.HourlyRate ?? 0m;
                var durationMinutes = session.ActualDurationMinutes ?? session.ScheduledDurationMinutes;
                var amount = (durationMinutes / 60.0m) * effectiveRate;

                invoice.AddLineItem(
                    session.Id,
                    session.SessionDate,
                    tutorName,
                    session.Subject,
                    durationMinutes,
                    effectiveRate,
                    amount,
                    isLateCancellationFee: false);
            }
        }

        // 8. Save invoice
        await _invoiceRepository.AddAsync(invoice);

        // 9. Return success with invoice ID
        return Result<Guid>.Success(invoice.Id);
    }
}
