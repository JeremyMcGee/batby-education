using BatbyEducation.Application.Commands.Invoices;
using BatbyEducation.Application.Commands.Payments;
using BatbyEducation.Application.Commands.Sessions;
using BatbyEducation.Application.Commands.Students;
using BatbyEducation.Application.Commands.Tutors;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BatbyEducation.Integration.Tests.Workflows;

public class InvoiceAndPaymentTests : IntegrationTestBase
{
    private async Task<(Guid TutorId, Guid StudentId, Guid SessionId)> SetupCompletedSession(
        decimal? studentRate = null, decimal? sessionRateOverride = null)
    {
        var tutorResult = await Mediator.Send(new RegisterTutorCommand(
            "Invoice Tutor", $"tutor-{Guid.NewGuid():N}@test.com",
            new List<string> { "Maths" }, 40.00m));
        var tutorId = tutorResult.Value;

        var studentResult = await Mediator.Send(new RegisterStudentCommand(
            "Invoice Student", $"student-{Guid.NewGuid():N}@test.com", "+441234567890",
            "Guardian", $"guardian-{Guid.NewGuid():N}@test.com", studentRate));
        var studentId = studentResult.Value;

        // Set tutor availability
        var slots = new List<AvailabilitySlotDto>
        {
            new(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0), null, true)
        };
        await Mediator.Send(new SetTutorAvailabilityCommand(tutorId, slots));

        // Book session on next Monday
        var today = DateOnly.FromDateTime(DateTime.Today);
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 7;
        var monday = today.AddDays(daysUntilMonday);

        var bookResult = await Mediator.Send(new BookSessionCommand(
            tutorId, studentId, "Maths", monday, new TimeOnly(10, 0), 60, sessionRateOverride));
        var sessionId = bookResult.Value;

        // Complete the session
        await Mediator.Send(new CompleteSessionCommand(sessionId, 60));

        return (tutorId, studentId, sessionId);
    }

    [Fact]
    public async Task GenerateInvoice_UsesEffectiveRate_TutorRate()
    {
        var (_, studentId, _) = await SetupCompletedSession();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var result = await Mediator.Send(new GenerateInvoiceCommand(
            studentId, today.AddDays(-1), today.AddDays(30)));

        Assert.True(result.IsSuccess);

        var invoiceRepo = ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var invoice = await invoiceRepo.GetByIdAsync(result.Value);
        Assert.NotNull(invoice);
        // 1 hour at £40/hr = £40
        Assert.Equal(40.00m, invoice.TotalAmount);
    }

    [Fact]
    public async Task GenerateInvoice_UsesEffectiveRate_StudentRate()
    {
        var (_, studentId, _) = await SetupCompletedSession(studentRate: 35.00m);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var result = await Mediator.Send(new GenerateInvoiceCommand(
            studentId, today.AddDays(-1), today.AddDays(30)));

        Assert.True(result.IsSuccess);

        var invoiceRepo = ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var invoice = await invoiceRepo.GetByIdAsync(result.Value);
        Assert.NotNull(invoice);
        // 1 hour at £35/hr (student rate overrides tutor rate) = £35
        Assert.Equal(35.00m, invoice.TotalAmount);
    }

    [Fact]
    public async Task GenerateInvoice_UsesEffectiveRate_SessionOverride()
    {
        var (_, studentId, _) = await SetupCompletedSession(
            studentRate: 35.00m, sessionRateOverride: 50.00m);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var result = await Mediator.Send(new GenerateInvoiceCommand(
            studentId, today.AddDays(-1), today.AddDays(30)));

        Assert.True(result.IsSuccess);

        var invoiceRepo = ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var invoice = await invoiceRepo.GetByIdAsync(result.Value);
        Assert.NotNull(invoice);
        // 1 hour at £50/hr (session override > student rate > tutor rate) = £50
        Assert.Equal(50.00m, invoice.TotalAmount);
    }

    [Fact]
    public async Task RecordPayment_UpdatesInvoiceStatusToPaid()
    {
        var (_, studentId, _) = await SetupCompletedSession();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var invoiceResult = await Mediator.Send(new GenerateInvoiceCommand(
            studentId, today.AddDays(-1), today.AddDays(30)));
        var invoiceId = invoiceResult.Value;

        var paymentResult = await Mediator.Send(new RecordPaymentCommand(
            invoiceId, 40.00m, DateOnly.FromDateTime(DateTime.Today), PaymentMethod.Cash));

        Assert.True(paymentResult.IsSuccess);

        var invoiceRepo = ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var invoice = await invoiceRepo.GetByIdAsync(invoiceId);
        Assert.Equal(InvoiceStatus.Paid, invoice!.Status);
    }

    [Fact]
    public async Task RecordPayment_Overpayment_CreatesStudentCredit()
    {
        var (_, studentId, _) = await SetupCompletedSession();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var invoiceResult = await Mediator.Send(new GenerateInvoiceCommand(
            studentId, today.AddDays(-1), today.AddDays(30)));
        var invoiceId = invoiceResult.Value;

        // Pay £50 on a £40 invoice
        await Mediator.Send(new RecordPaymentCommand(
            invoiceId, 50.00m, DateOnly.FromDateTime(DateTime.Today), PaymentMethod.BankTransfer));

        var accountRepo = ServiceProvider.GetRequiredService<IStudentAccountRepository>();
        var account = await accountRepo.GetByStudentIdAsync(studentId);
        Assert.NotNull(account);
        Assert.Equal(10.00m, account.CreditBalance);
    }

    [Fact]
    public async Task RecordPayment_PostsToCorrectLedger()
    {
        var (_, studentId, _) = await SetupCompletedSession();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var invoiceResult = await Mediator.Send(new GenerateInvoiceCommand(
            studentId, today.AddDays(-1), today.AddDays(30)));
        var invoiceId = invoiceResult.Value;

        await Mediator.Send(new RecordPaymentCommand(
            invoiceId, 40.00m, DateOnly.FromDateTime(DateTime.Today), PaymentMethod.Cash));

        var ledgerRepo = ServiceProvider.GetRequiredService<ILedgerRepository>();
        var cashTotal = await ledgerRepo.GetTotalAsync(
            PaymentMethod.Cash, today.AddDays(-1), today.AddDays(1));
        var bankTotal = await ledgerRepo.GetTotalAsync(
            PaymentMethod.BankTransfer, today.AddDays(-1), today.AddDays(1));

        Assert.Equal(40.00m, cashTotal);
        Assert.Equal(0m, bankTotal);
    }
}
