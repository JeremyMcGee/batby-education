using BatbyEducation.Application.Commands.Students;
using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Interfaces;
using BatbyEducation.Infrastructure.Data;
using BatbyEducation.Infrastructure.Events;
using BatbyEducation.Infrastructure.Repositories;
using BatbyEducation.Web.Components;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<BatbyEducationDbContext>(options =>
    options.UseSqlite("Data Source=batbyeducation.db"));

// Repositories
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITutorRepository, TutorRepository>();
builder.Services.AddScoped<ITutorAvailabilityRepository, TutorAvailabilityRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
builder.Services.AddScoped<IStudentAccountRepository, StudentAccountRepository>();
builder.Services.AddScoped<IAuditEntryRepository, AuditEntryRepository>();

// MediatR (from Application assembly)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterStudentCommand>());

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterStudentCommandValidator>();

// Domain Event Dispatcher
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

var app = builder.Build();

// Apply pending migrations on startup
await app.MigrateDatabaseAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
