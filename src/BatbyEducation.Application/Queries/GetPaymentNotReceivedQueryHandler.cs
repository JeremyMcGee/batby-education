using MediatR;
using BatbyEducation.Application.DTOs;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;

namespace BatbyEducation.Application.Queries;

/// <summary>
/// Handles the GetPaymentNotReceivedQuery by identifying sessions where bank transfer
/// payment was not received at least 24 hours before the session start time.
/// Separates results into upcoming sessions and past-due sessions.
/// </summary>
public class GetPaymentNotReceivedQueryHandler : IRequestHandler<GetPaymentNotReceivedQuery, PaymentNotReceivedReportDto>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentNotReceivedQueryHandler(
        IStudentRepository studentRepository,
        ISessionRepository sessionRepository,
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository)
    {
        _studentRepository = studentRepository;
        _sessionRepository = sessionRepository;
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentNotReceivedReportDto> Handle(GetPaymentNotReceivedQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Get all students to iterate their sessions
        var students = await _studentRepository.GetAllAsync();

        var upcomingSessions = new List<PaymentNotReceivedEntryDto>();
        var pastDueSessions = new List<PaymentNotReceivedEntryDto>();

        foreach (var student in students)
        {
            // Get sessions for the student in a broad range (past month to next month)
            // to capture both upcoming and recently past-due sessions
            var rangeStart = today.AddDays(-60);
            var rangeEnd = today.AddDays(60);
            var sessions = await _sessionRepository.GetByStudentAndDateRangeAsync(
                student.Id, rangeStart, rangeEnd);

            // Filter to scheduled sessions (not cancelled/completed) that require payment check
            var relevantSessions = sessions
                .Where(s => s.Status == SessionStatus.Scheduled
                         || s.Status == SessionStatus.PendingConfirmation)
                .ToList();

            // Get student's invoices to check for bank transfer payments
            var invoices = await _invoiceRepository.GetByStudentAsync(student.Id);

            foreach (var session in relevantSessions)
            {
                var sessionStart = session.SessionDate.ToDateTime(session.StartTime);
                var paymentDeadline = sessionStart.AddHours(-24);

                // Only flag sessions where the payment deadline has passed
                if (now < paymentDeadline)
                    continue;

                // Check if there's an invoice covering this session with bank transfer payment
                var coveringInvoice = invoices.FirstOrDefault(i =>
                    i.LineItems.Any(li => li.SessionId == session.Id));

                if (coveringInvoice is null)
                {
                    // No invoice yet - compute expected amount from session duration and rate
                    // For simplicity, flag as payment not received with zero expected amount
                    // (the amount will be calculated when invoice is generated)
                    var expectedAmount = 0m;

                    var entry = CreateEntry(student.Name, student.GuardianName,
                        session.SessionDate, expectedAmount, now, paymentDeadline);

                    if (session.SessionDate < today)
                        pastDueSessions.Add(entry);
                    else
                        upcomingSessions.Add(entry);

                    continue;
                }

                // Check if bank transfer payments exist on the invoice
                var payments = await _paymentRepository.GetByInvoiceAsync(coveringInvoice.Id);
                var bankTransferPayments = payments
                    .Where(p => p.Method == PaymentMethod.BankTransfer)
                    .ToList();

                // If no bank transfer payment received before deadline, flag it
                var paidBeforeDeadline = bankTransferPayments
                    .Any(p => p.PaymentDate.ToDateTime(TimeOnly.MinValue) <= paymentDeadline);

                if (!paidBeforeDeadline)
                {
                    var expectedAmount = coveringInvoice.GetOutstandingBalance();

                    var entry = CreateEntry(student.Name, student.GuardianName,
                        session.SessionDate, expectedAmount, now, paymentDeadline);

                    if (session.SessionDate < today)
                        pastDueSessions.Add(entry);
                    else
                        upcomingSessions.Add(entry);
                }
            }
        }

        // Sort upcoming by session date ascending, past-due by session date ascending
        var sortedUpcoming = upcomingSessions.OrderBy(e => e.SessionDate).ToList();
        var sortedPastDue = pastDueSessions.OrderBy(e => e.SessionDate).ToList();

        return new PaymentNotReceivedReportDto(sortedUpcoming, sortedPastDue);
    }

    private static PaymentNotReceivedEntryDto CreateEntry(
        string studentName,
        string guardianName,
        DateOnly sessionDate,
        decimal expectedAmount,
        DateTime now,
        DateTime paymentDeadline)
    {
        var daysSinceDeadline = (int)(now - paymentDeadline).TotalDays;
        if (daysSinceDeadline < 0) daysSinceDeadline = 0;

        return new PaymentNotReceivedEntryDto(
            StudentName: studentName,
            GuardianName: guardianName,
            SessionDate: sessionDate,
            ExpectedAmount: expectedAmount,
            DaysSinceDeadline: daysSinceDeadline);
    }
}
