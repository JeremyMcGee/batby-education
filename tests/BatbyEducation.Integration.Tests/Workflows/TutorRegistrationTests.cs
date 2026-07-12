using BatbyEducation.Application.Commands.Tutors;
using BatbyEducation.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BatbyEducation.Integration.Tests.Workflows;

/// <summary>
/// These tests exercise tutor registration through the full stack (MediatR → Handler → EF Core → SQLite).
/// They would have caught the EmailAddress value conversion query issue and the Subjects JSON serialisation issues.
/// </summary>
public class TutorRegistrationTests : IntegrationTestBase
{
    [Fact]
    public async Task RegisterTutor_WithValidData_CreatesTutorInDatabase()
    {
        var command = new RegisterTutorCommand(
            "Dr. Jones", "jones@example.com",
            new List<string> { "Maths", "Physics" }, 45.00m);

        var result = await Mediator.Send(command);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        var repo = ServiceProvider.GetRequiredService<ITutorRepository>();
        var tutor = await repo.GetByIdAsync(result.Value);
        Assert.NotNull(tutor);
        Assert.Equal("Dr. Jones", tutor.Name);
        Assert.Equal("jones@example.com", tutor.Email.Value);
        Assert.Contains("Maths", tutor.Subjects);
        Assert.Contains("Physics", tutor.Subjects);
        Assert.Equal(45.00m, tutor.HourlyRate);
    }

    [Fact]
    public async Task RegisterTutor_DuplicateEmail_ReturnsFailure()
    {
        await Mediator.Send(new RegisterTutorCommand(
            "First Tutor", "tutor@example.com",
            new List<string> { "English" }, 30.00m));

        var result = await Mediator.Send(new RegisterTutorCommand(
            "Second Tutor", "tutor@example.com",
            new List<string> { "Maths" }, 35.00m));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Field == "Email");
    }

    [Fact]
    public async Task RegisterTutor_ThenLookupByEmail_FindsTutor()
    {
        await Mediator.Send(new RegisterTutorCommand(
            "Email Lookup Tutor", "lookup@example.com",
            new List<string> { "History" }, 25.00m));

        var repo = ServiceProvider.GetRequiredService<ITutorRepository>();
        var tutor = await repo.GetByEmailAsync("lookup@example.com");

        Assert.NotNull(tutor);
        Assert.Equal("Email Lookup Tutor", tutor.Name);
    }
}
