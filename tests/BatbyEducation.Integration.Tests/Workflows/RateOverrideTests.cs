using BatbyEducation.Application.Commands.Invoices;
using BatbyEducation.Application.Commands.Sessions;
using BatbyEducation.Application.Commands.Students;
using BatbyEducation.Application.Commands.Tutors;
using BatbyEducation.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BatbyEducation.Integration.Tests.Workflows;

public class RateOverrideTests : IntegrationTestBase
{
    private async Task<(Guid TutorId, Guid StudentId)> SetupTutorAndStudent(
        decimal tutorRate = 40.00m, decimal? studentRate = null)
    {
        var tutorResult = await Mediator.Send(new RegisterTutorCommand(
            "Rate Tutor", $"tutor-{Guid.NewGuid():N}@test.com",
            new List<string> { "Maths", "English" }, tutorRate));

        var studentResult = await Mediator.Send(new RegisterStudentCommand(
            "Rate Student", $"student-{Guid.NewGuid():N}@test.com", "+441234567890",
            "Guardian", $"guardian-{Guid.NewGuid():N}@test.com", studentRate));

        // Set tutor availability for Monday
        var slots = new List<AvailabilitySlotDto>
        {
            new(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0), null, true)
        };
        await Mediator.Send(new SetTutorAvailabilityCommand(tutorResult.Value, slots));

        return (tutorResult.Value, studentResult.Value);
    }

    private DateOnly GetNextMonday()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 7;
        return today.AddDays(daysUntilMonday);
    }

    [Fact]
    public async Task Invoice_StudentRateOverridesTutorRate()
    {
        // Tutor rate: £40, Student rate: £35
        var (tutorId, studentId) = await SetupTutorAndStudent(tutorRate: 40.00m, studentRate: 35.00m);
        var monday = GetNextMonday();

        // Book and complete a session (no session override)
        var bookResult = await Mediator.Send(new BookSessionCommand(
            tutorId, studentId, "Maths", monday, new TimeOnly(10, 0), 60));
        Assert.True(bookResult.IsSuccess);
        await Mediator.Send(new CompleteSessionCommand(bookResult.Value, 60));

        // Generate invoice
        var today = DateOnly.FromDateTime(DateTime.Today);
        var invoiceResult = await Mediator.Send(new GenerateInvoiceCommand(
            studentId, today.AddDays(-1), today.AddDays(30)));
        Assert.True(invoiceResult.IsSuccess);

        var invoiceRepo = ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var invoice = await invoiceRepo.GetByIdAsync(invoiceResult.Value);
        Assert.NotNull(invoice);
        // 1 hour at £35/hr (student rate overrides tutor rate)
        Assert.Equal(35.00m, invoice.TotalAmount);
    }

    [Fact]
    public async Task Invoice_SessionRateOverridesBothStudentAndTutorRate()
    {
        // Tutor rate: £40, Student rate: £35, Session override: £50
        var (tutorId, studentId) = await SetupTutorAndStudent(tutorRate: 40.00m, studentRate: 35.00m);
        var monday = GetNextMonday();

        // Book session with a rate override
        var bookResult = await Mediator.Send(new BookSessionCommand(
            tutorId, studentId, "Maths", monday, new TimeOnly(10, 0), 60, 50.00m));
        Assert.True(bookResult.IsSuccess);
        await Mediator.Send(new CompleteSessionCommand(bookResult.Value, 60));

        // Generate invoice
        var today = DateOnly.FromDateTime(DateTime.Today);
        var invoiceResult = await Mediator.Send(new GenerateInvoiceCommand(
            studentId, today.AddDays(-1), today.AddDays(30)));
        Assert.True(invoiceResult.IsSuccess);

        var invoiceRepo = ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var invoice = await invoiceRepo.GetByIdAsync(invoiceResult.Value);
        Assert.NotNull(invoice);
        // 1 hour at £50/hr (session override > student > tutor)
        Assert.Equal(50.00m, invoice.TotalAmount);
    }

    [Fact]
    public async Task UpdateSessionRate_OnScheduledSession_Succeeds()
    {
        var (tutorId, studentId) = await SetupTutorAndStudent();
        var monday = GetNextMonday();

        var bookResult = await Mediator.Send(new BookSessionCommand(
            tutorId, studentId, "Maths", monday, new TimeOnly(10, 0), 60));
        Assert.True(bookResult.IsSuccess);
        var sessionId = bookResult.Value;

        var updateResult = await Mediator.Send(new UpdateSessionRateCommand(sessionId, 55.00m));
        Assert.True(updateResult.IsSuccess);

        var repo = ServiceProvider.GetRequiredService<ISessionRepository>();
        var session = await repo.GetByIdAsync(sessionId);
        Assert.NotNull(session);
        Assert.Equal(55.00m, session.RateOverride);
    }

    [Fact]
    public async Task UpdateSessionRate_OnCancelledSession_ReturnsFailure()
    {
        var (tutorId, studentId) = await SetupTutorAndStudent();
        var monday = GetNextMonday();

        var bookResult = await Mediator.Send(new BookSessionCommand(
            tutorId, studentId, "Maths", monday, new TimeOnly(10, 0), 60));
        Assert.True(bookResult.IsSuccess);
        var sessionId = bookResult.Value;

        // Cancel the session
        await Mediator.Send(new CancelSessionCommand(sessionId));

        // Attempt to update rate on cancelled session
        var updateResult = await Mediator.Send(new UpdateSessionRateCommand(sessionId, 50.00m));
        Assert.False(updateResult.IsSuccess);
        Assert.Contains(updateResult.Errors, e => e.Field == "Status");
    }

    [Fact]
    public async Task ClearSessionRateOverride_FallsBackToStudentOrTutorRate()
    {
        // Tutor rate: £40, Student rate: £35, Session starts with override: £50
        var (tutorId, studentId) = await SetupTutorAndStudent(tutorRate: 40.00m, studentRate: 35.00m);
        var monday = GetNextMonday();

        // Book with session override
        var bookResult = await Mediator.Send(new BookSessionCommand(
            tutorId, studentId, "Maths", monday, new TimeOnly(10, 0), 60, 50.00m));
        Assert.True(bookResult.IsSuccess);
        var sessionId = bookResult.Value;

        // Clear the override (set to null)
        var clearResult = await Mediator.Send(new UpdateSessionRateCommand(sessionId, null));
        Assert.True(clearResult.IsSuccess);

        var repo = ServiceProvider.GetRequiredService<ISessionRepository>();
        var session = await repo.GetByIdAsync(sessionId);
        Assert.NotNull(session);
        Assert.Null(session.RateOverride);

        // Complete session and generate invoice — should use student rate (£35)
        await Mediator.Send(new CompleteSessionCommand(sessionId, 60));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var invoiceResult = await Mediator.Send(new GenerateInvoiceCommand(
            studentId, today.AddDays(-1), today.AddDays(30)));
        Assert.True(invoiceResult.IsSuccess);

        var invoiceRepo = ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var invoice = await invoiceRepo.GetByIdAsync(invoiceResult.Value);
        Assert.NotNull(invoice);
        // Falls back to student rate £35 since session override was cleared
        Assert.Equal(35.00m, invoice.TotalAmount);
    }
}
