using BatbyEducation.Application.Commands.Students;
using BatbyEducation.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BatbyEducation.Integration.Tests.Workflows;

public class StudentActiveStatusTests : IntegrationTestBase
{
    [Fact]
    public async Task NewStudent_IsActiveByDefault()
    {
        var result = await Mediator.Send(new RegisterStudentCommand(
            "Active Student", "active@example.com", "+441234567890",
            "Guardian", "guardian@example.com"));

        Assert.True(result.IsSuccess);

        var repo = ServiceProvider.GetRequiredService<IStudentRepository>();
        var student = await repo.GetByIdAsync(result.Value);
        Assert.NotNull(student);
        Assert.True(student.IsActive);
    }

    [Fact]
    public async Task DeactivatedStudent_NotReturnedByGetActiveAsync()
    {
        var repo = ServiceProvider.GetRequiredService<IStudentRepository>();

        var result = await Mediator.Send(new RegisterStudentCommand(
            "Will Be Inactive", "inactive@example.com", "+441234567890",
            "Guardian", "guardian@example.com"));

        var student = await repo.GetByIdAsync(result.Value);
        Assert.NotNull(student);

        student.Deactivate();
        await repo.UpdateAsync(student);

        var activeStudents = await repo.GetActiveAsync();
        Assert.DoesNotContain(activeStudents, s => s.Id == result.Value);
    }

    [Fact]
    public async Task DeactivatedStudent_StillReturnedByGetAllAsync()
    {
        var repo = ServiceProvider.GetRequiredService<IStudentRepository>();

        var result = await Mediator.Send(new RegisterStudentCommand(
            "Still In List", "stillinlist@example.com", "+441234567890",
            "Guardian", "guardian@example.com"));

        var student = await repo.GetByIdAsync(result.Value);
        student!.Deactivate();
        await repo.UpdateAsync(student);

        var allStudents = await repo.GetAllAsync();
        Assert.Contains(allStudents, s => s.Id == result.Value);
    }

    [Fact]
    public async Task ReactivatedStudent_AppearsInGetActiveAsync()
    {
        var repo = ServiceProvider.GetRequiredService<IStudentRepository>();

        var result = await Mediator.Send(new RegisterStudentCommand(
            "Reactivated", "reactivated@example.com", "+441234567890",
            "Guardian", "guardian@example.com"));

        var student = await repo.GetByIdAsync(result.Value);
        student!.Deactivate();
        await repo.UpdateAsync(student);

        // Reload and reactivate
        student = await repo.GetByIdAsync(result.Value);
        student!.Activate();
        await repo.UpdateAsync(student);

        var activeStudents = await repo.GetActiveAsync();
        Assert.Contains(activeStudents, s => s.Id == result.Value);
    }
}
