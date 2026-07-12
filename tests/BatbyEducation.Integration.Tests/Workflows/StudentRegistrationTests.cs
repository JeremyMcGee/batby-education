using BatbyEducation.Application.Commands.Students;
using BatbyEducation.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BatbyEducation.Integration.Tests.Workflows;

public class StudentRegistrationTests : IntegrationTestBase
{
    [Fact]
    public async Task RegisterStudent_WithValidData_CreatesStudentInDatabase()
    {
        var command = new RegisterStudentCommand(
            "Alice Smith", "alice@example.com", "+441234567890",
            "Bob Smith", "bob@example.com");

        var result = await Mediator.Send(command);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        var repo = ServiceProvider.GetRequiredService<IStudentRepository>();
        var student = await repo.GetByIdAsync(result.Value);
        Assert.NotNull(student);
        Assert.Equal("Alice Smith", student.Name);
        Assert.Equal("alice@example.com", student.Email.Value);
    }

    [Fact]
    public async Task RegisterStudent_WithHourlyRate_StoresRateInDatabase()
    {
        var command = new RegisterStudentCommand(
            "Charlie Brown", "charlie@example.com", "+441234567890",
            "Parent Brown", "parent@example.com", 35.00m);

        var result = await Mediator.Send(command);

        Assert.True(result.IsSuccess);

        var repo = ServiceProvider.GetRequiredService<IStudentRepository>();
        var student = await repo.GetByIdAsync(result.Value);
        Assert.NotNull(student);
        Assert.Equal(35.00m, student.HourlyRate);
    }

    [Fact]
    public async Task RegisterStudent_DuplicateEmail_ReturnsFailure()
    {
        var command1 = new RegisterStudentCommand(
            "First Student", "same@example.com", "+441234567890",
            "Guardian", "guardian@example.com");
        await Mediator.Send(command1);

        var command2 = new RegisterStudentCommand(
            "Second Student", "same@example.com", "+441234567891",
            "Guardian 2", "guardian2@example.com");
        var result = await Mediator.Send(command2);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Field == "Email");
    }

    [Fact]
    public async Task UpdateStudent_RecordsAuditHistory()
    {
        var createResult = await Mediator.Send(new RegisterStudentCommand(
            "Original Name", "audit@example.com", "+441234567890",
            "Guardian", "guardian@example.com"));

        var updateResult = await Mediator.Send(new UpdateStudentCommand(
            createResult.Value,
            "Updated Name", "audit@example.com", "+441234567890",
            "Guardian", "guardian@example.com"));

        Assert.True(updateResult.IsSuccess);

        var repo = ServiceProvider.GetRequiredService<IStudentRepository>();
        var student = await repo.GetByIdAsync(createResult.Value);
        Assert.NotNull(student);
        Assert.Equal("Updated Name", student.Name);
        Assert.Contains(student.AuditHistory, a => a.FieldName == "Name" && a.OldValue == "Original Name");
    }
}
