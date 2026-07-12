using BatbyEducation.Application.Commands.Students;
using BatbyEducation.Application.Commands.Tutors;
using BatbyEducation.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BatbyEducation.Integration.Tests.Workflows;

public class StudentDefaultsTests : IntegrationTestBase
{
    private async Task<Guid> CreateTutor()
    {
        var result = await Mediator.Send(new RegisterTutorCommand(
            "Defaults Tutor", $"tutor-{Guid.NewGuid():N}@test.com",
            new List<string> { "Maths", "English" }, 40.00m));
        return result.Value;
    }

    [Fact]
    public async Task RegisterStudent_WithDefaults_StoresAllDefaultsCorrectly()
    {
        var tutorId = await CreateTutor();

        var result = await Mediator.Send(new RegisterStudentCommand(
            "Default Student", "defaults@example.com", "+441234567890",
            "Guardian", "guardian@example.com",
            HourlyRate: 30.00m,
            DefaultTutorId: tutorId,
            DefaultSubject: "Maths",
            DefaultDay: DayOfWeek.Wednesday,
            DefaultStartTime: new TimeOnly(14, 30)));

        Assert.True(result.IsSuccess);

        var repo = ServiceProvider.GetRequiredService<IStudentRepository>();
        var student = await repo.GetByIdAsync(result.Value);
        Assert.NotNull(student);
        Assert.Equal(30.00m, student.HourlyRate);
        Assert.Equal(tutorId, student.DefaultTutorId);
        Assert.Equal("Maths", student.DefaultSubject);
        Assert.Equal(DayOfWeek.Wednesday, student.DefaultDay);
        Assert.Equal(new TimeOnly(14, 30), student.DefaultStartTime);
    }

    [Fact]
    public async Task UpdateStudent_ChangingDefaults_RecordsAuditTrail()
    {
        var tutorId = await CreateTutor();
        var email = $"audit-defaults-{Guid.NewGuid():N}@example.com";
        var guardianEmail = $"guardian-{Guid.NewGuid():N}@example.com";

        // Register with initial defaults
        var createResult = await Mediator.Send(new RegisterStudentCommand(
            "Audit Student", email, "+441234567890",
            "Guardian", guardianEmail,
            HourlyRate: 30.00m,
            DefaultTutorId: tutorId,
            DefaultSubject: "Maths",
            DefaultDay: DayOfWeek.Monday,
            DefaultStartTime: new TimeOnly(9, 0)));

        Assert.True(createResult.IsSuccess);
        var studentId = createResult.Value;

        // Update defaults (keep name, email, phone, guardian fields the same)
        var updateResult = await Mediator.Send(new UpdateStudentCommand(
            studentId,
            "Audit Student", email, "+441234567890",
            "Guardian", guardianEmail,
            HourlyRate: 45.00m,
            DefaultTutorId: tutorId,
            DefaultSubject: "English",
            DefaultDay: DayOfWeek.Friday,
            DefaultStartTime: new TimeOnly(16, 0)));

        Assert.True(updateResult.IsSuccess);

        var repo = ServiceProvider.GetRequiredService<IStudentRepository>();
        var student = await repo.GetByIdAsync(studentId);
        Assert.NotNull(student);

        // Verify new values
        Assert.Equal(45.00m, student.HourlyRate);
        Assert.Equal("English", student.DefaultSubject);
        Assert.Equal(DayOfWeek.Friday, student.DefaultDay);
        Assert.Equal(new TimeOnly(16, 0), student.DefaultStartTime);

        // Verify audit entries were recorded for the changed fields
        Assert.Contains(student.AuditHistory, a => a.FieldName == "HourlyRate");
        Assert.Contains(student.AuditHistory, a => a.FieldName == "DefaultSubject" && a.OldValue == "Maths" && a.NewValue == "English");
        Assert.Contains(student.AuditHistory, a => a.FieldName == "DefaultDay" && a.OldValue == "Monday" && a.NewValue == "Friday");
        Assert.Contains(student.AuditHistory, a => a.FieldName == "DefaultStartTime" && a.OldValue == "09:00" && a.NewValue == "16:00");
    }

    [Fact]
    public async Task RegisterStudent_WithZeroHourlyRate_ReturnsFailure()
    {
        var result = await Mediator.Send(new RegisterStudentCommand(
            "Bad Rate Student", "badrate@example.com", "+441234567890",
            "Guardian", "guardian@example.com",
            HourlyRate: 0m));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Field == "HourlyRate");
    }

    [Fact]
    public async Task RegisterStudent_WithExcessiveHourlyRate_ReturnsFailure()
    {
        var result = await Mediator.Send(new RegisterStudentCommand(
            "Expensive Student", "expensive@example.com", "+441234567890",
            "Guardian", "guardian@example.com",
            HourlyRate: 1000m));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Field == "HourlyRate");
    }
}
