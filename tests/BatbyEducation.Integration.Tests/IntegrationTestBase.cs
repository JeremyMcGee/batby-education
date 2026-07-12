using BatbyEducation.Domain.Common;
using BatbyEducation.Infrastructure.Data;
using BatbyEducation.Infrastructure.Events;
using BatbyEducation.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BatbyEducation.Integration.Tests;

/// <summary>
/// Base class for integration tests. Sets up a real SQLite database (in-memory mode)
/// with the full EF Core stack, repositories, and MediatR handlers wired up.
/// Each test gets a fresh database.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly BatbyEducationDbContext DbContext;
    protected readonly IMediator Mediator;

    protected IntegrationTestBase()
    {
        var services = new ServiceCollection();

        // Use SQLite in-memory with a shared connection to keep the DB alive for the test
        services.AddDbContext<BatbyEducationDbContext>(options =>
            options.UseSqlite("Data Source=:memory:"));

        // MediatR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<BatbyEducation.Application.Commands.Students.RegisterStudentCommand>());

        // Repositories
        services.AddScoped<Domain.Interfaces.IStudentRepository, StudentRepository>();
        services.AddScoped<Domain.Interfaces.ITutorRepository, TutorRepository>();
        services.AddScoped<Domain.Interfaces.ITutorAvailabilityRepository, TutorAvailabilityRepository>();
        services.AddScoped<Domain.Interfaces.ISessionRepository, SessionRepository>();
        services.AddScoped<Domain.Interfaces.IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<Domain.Interfaces.IPaymentRepository, PaymentRepository>();
        services.AddScoped<Domain.Interfaces.ILedgerRepository, LedgerRepository>();
        services.AddScoped<Domain.Interfaces.IStudentAccountRepository, StudentAccountRepository>();
        services.AddScoped<Domain.Interfaces.IAuditEntryRepository, AuditEntryRepository>();

        // Domain event dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        ServiceProvider = services.BuildServiceProvider();

        DbContext = ServiceProvider.GetRequiredService<BatbyEducationDbContext>();
        DbContext.Database.OpenConnection();
        DbContext.Database.EnsureCreated();

        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    public void Dispose()
    {
        DbContext.Database.CloseConnection();
        DbContext.Dispose();
        ServiceProvider.Dispose();
    }
}
