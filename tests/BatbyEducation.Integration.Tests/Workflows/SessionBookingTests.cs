using BatbyEducation.Application.Commands.Sessions;
using BatbyEducation.Application.Commands.Students;
using BatbyEducation.Application.Commands.Tutors;
using BatbyEducation.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BatbyEducation.Integration.Tests.Workflows;

public class SessionBookingTests : IntegrationTestBase
{
    private async Task<(Guid TutorId, Guid StudentId)> SetupTutorAndStudent()
    {
        var tutorResult = await Mediator.Send(new RegisterTutorCommand(
            "Test Tutor", "tutor@test.com", new List<string> { "Maths", "English" }, 40.00m));

        var studentResult = await Mediator.Send(new RegisterStudentCommand(
            "Test Student", "student@test.com", "+441234567890",
            "Test Guardian", "guardian@test.com"));

        // Set tutor availability for Monday 09:00-17:00
        var availabilitySlots = new List<AvailabilitySlotDto>
        {
            new(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0), null, true),
            new(DayOfWeek.Tuesday, new TimeOnly(9, 0), new TimeOnly(17, 0), null, true),
            new(DayOfWeek.Wednesday, new TimeOnly(9, 0), new TimeOnly(17, 0), null, true),
            new(DayOfWeek.Thursday, new TimeOnly(9, 0), new TimeOnly(17, 0), null, true),
            new(DayOfWeek.Friday, new TimeOnly(9, 0), new TimeOnly(17, 0), null, true),
        };
        await Mediator.Send(new SetTutorAvailabilityCommand(tutorResult.Value, availabilitySlots));

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
    public async Task BookSession_WithValidData_CreatesSession()
    {
        var (tutorId, studentId) = await SetupTutorAndStudent();
        var monday = GetNextMonday();

        var result = await Mediator.Send(new BookSessionCommand(
            tutorId, studentId, "Maths", monday, new TimeOnly(10, 0), 60));

        Assert.True(result.IsSuccess);

        var repo = ServiceProvider.GetRequiredService<ISessionRepository>();
        var session = await repo.GetByIdAsync(result.Value);
        Assert.NotNull(session);
        Assert.Equal("Maths", session.Subject);
        Assert.Equal(60, session.ScheduledDurationMinutes);
    }

    [Fact]
    public async Task BookSession_WithRateOverride_StoresOverride()
    {
        var (tutorId, studentId) = await SetupTutorAndStudent();
        var monday = GetNextMonday();

        var result = await Mediator.Send(new BookSessionCommand(
            tutorId, studentId, "English", monday, new TimeOnly(14, 0), 45, 55.00m));

        Assert.True(result.IsSuccess);

        var repo = ServiceProvider.GetRequiredService<ISessionRepository>();
        var session = await repo.GetByIdAsync(result.Value);
        Assert.NotNull(session);
        Assert.Equal(55.00m, session.RateOverride);
    }

    [Fact]
    public async Task BookSession_InvalidSubject_ReturnsFailure()
    {
        var (tutorId, studentId) = await SetupTutorAndStudent();
        var monday = GetNextMonday();

        var result = await Mediator.Send(new BookSessionCommand(
            tutorId, studentId, "Underwater Basket Weaving", monday, new TimeOnly(10, 0), 60));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Field == "Subject");
    }

    [Fact]
    public async Task BookSession_TutorConflict_ReturnsFailure()
    {
        var (tutorId, studentId) = await SetupTutorAndStudent();
        var monday = GetNextMonday();

        // Book first session
        await Mediator.Send(new BookSessionCommand(
            tutorId, studentId, "Maths", monday, new TimeOnly(10, 0), 60));

        // Register a second student
        var student2 = await Mediator.Send(new RegisterStudentCommand(
            "Student 2", "student2@test.com", "+441234567891",
            "Guardian 2", "guardian2@test.com"));

        // Try to book overlapping session for same tutor
        var result = await Mediator.Send(new BookSessionCommand(
            tutorId, student2.Value, "Maths", monday, new TimeOnly(10, 30), 60));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Field == "StartTime");
    }

    [Fact]
    public async Task CancelSession_ThenComplete_Fails()
    {
        var (tutorId, studentId) = await SetupTutorAndStudent();
        var monday = GetNextMonday();

        var bookResult = await Mediator.Send(new BookSessionCommand(
            tutorId, studentId, "Maths", monday, new TimeOnly(10, 0), 60));
        var sessionId = bookResult.Value;

        await Mediator.Send(new CancelSessionCommand(sessionId));

        var completeResult = await Mediator.Send(new CompleteSessionCommand(sessionId, 60));
        Assert.False(completeResult.IsSuccess);
    }

    [Fact]
    public async Task UpdateSessionRate_OnScheduledSession_Succeeds()
    {
        var (tutorId, studentId) = await SetupTutorAndStudent();
        var monday = GetNextMonday();

        var bookResult = await Mediator.Send(new BookSessionCommand(
            tutorId, studentId, "Maths", monday, new TimeOnly(10, 0), 60));
        var sessionId = bookResult.Value;

        var updateResult = await Mediator.Send(new UpdateSessionRateCommand(sessionId, 50.00m));
        Assert.True(updateResult.IsSuccess);

        var repo = ServiceProvider.GetRequiredService<ISessionRepository>();
        var session = await repo.GetByIdAsync(sessionId);
        Assert.Equal(50.00m, session!.RateOverride);
    }
}
