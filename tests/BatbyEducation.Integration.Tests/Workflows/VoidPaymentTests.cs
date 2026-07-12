using BatbyEducation.Application.Commands.Invoices;
using BatbyEducation.Application.Commands.Payments;
using BatbyEducation.Application.Commands.Sessions;
using BatbyEducation.Application.Commands.Students;
using BatbyEducation.Application.Commands.Tutors;
using BatbyEducation.Domain.Enumerations;
using BatbyEducation.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BatbyEducation.Integration.Tests.Workflows;

public class VoidPaymentTests : IntegrationTestBase
{
    private async Task<(Guid InvoiceId, Guid PaymentId, Guid StudentId)> SetupPaidInvoice()
    {
        var tutorResult = await Mediator.Send(new RegisterTutorCommand(
            "Void Tutor", $"tutor-{Guid.NewGuid():N}@test.com",
            new List<string> { "Maths" }, 40.00m));

        var studentResult = await Mediator.Send(new RegisterStudentCommand(
            "Void Student", $"student-{Guid.NewGuid():N}@test.com", "+441234567890",
            "Guardian", $"guardian-{Guid.NewGuid():N}@test.com"));

        await Mediator.Send(new SetTutorAvailabilityCommand(tutorResult.Value,
            new List<AvailabilitySlotDto> { new(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0), null, true) }));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 7;
        var monday = today.AddDays(daysUntilMonday);

        var bookResult = await Mediator.Send(new BookSessionCommand(
            tutorResult.Value, studentResult.Value, "Maths", monday, new TimeOnly(10, 0), 60));
        await Mediator.Send(new CompleteSessionCommand(bookResult.Value, 60));

        var invoiceResult = await Mediator.Send(new GenerateInvoiceCommand(
            studentResult.Value, today.AddDays(-1), today.AddDays(30)));

        var paymentResult = await Mediator.Send(new RecordPaymentCommand(
            invoiceResult.Value, 40.00m, DateOnly.FromDateTime(DateTime.Today), PaymentMethod.Cash));

        return (invoiceResult.Value, paymentResult.Value, studentResult.Value);
    }

    [Fact]
    public async Task VoidPayment_ReversesInvoiceStatus()
    {
        var (invoiceId, paymentId, _) = await SetupPaidInvoice();

        var invoiceRepo = ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var invoiceBefore = await invoiceRepo.GetByIdAsync(invoiceId);
        Assert.Equal(InvoiceStatus.Paid, invoiceBefore!.Status);

        var voidResult = await Mediator.Send(new VoidPaymentCommand(paymentId));
        Assert.True(voidResult.IsSuccess);

        var invoiceAfter = await invoiceRepo.GetByIdAsync(invoiceId);
        Assert.Equal(InvoiceStatus.Created, invoiceAfter!.Status);
        Assert.Equal(0m, invoiceAfter.TotalPaid);
    }

    [Fact]
    public async Task VoidPayment_RemovesLedgerEntry()
    {
        var (_, paymentId, _) = await SetupPaidInvoice();

        var ledgerRepo = ServiceProvider.GetRequiredService<ILedgerRepository>();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var cashBefore = await ledgerRepo.GetTotalAsync(PaymentMethod.Cash, today.AddDays(-1), today.AddDays(1));
        Assert.Equal(40.00m, cashBefore);

        await Mediator.Send(new VoidPaymentCommand(paymentId));

        var cashAfter = await ledgerRepo.GetTotalAsync(PaymentMethod.Cash, today.AddDays(-1), today.AddDays(1));
        Assert.Equal(0m, cashAfter);
    }

    [Fact]
    public async Task VoidPayment_DeletesPaymentRecord()
    {
        var (_, paymentId, _) = await SetupPaidInvoice();

        var paymentRepo = ServiceProvider.GetRequiredService<IPaymentRepository>();
        var paymentBefore = await paymentRepo.GetByIdAsync(paymentId);
        Assert.NotNull(paymentBefore);

        await Mediator.Send(new VoidPaymentCommand(paymentId));

        var paymentAfter = await paymentRepo.GetByIdAsync(paymentId);
        Assert.Null(paymentAfter);
    }

    [Fact]
    public async Task VoidPayment_NonExistentPayment_ReturnsFailure()
    {
        var result = await Mediator.Send(new VoidPaymentCommand(Guid.NewGuid()));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Field == "PaymentId");
    }
}
