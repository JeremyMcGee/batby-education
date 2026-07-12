using BatbyEducation.Application.Commands.Students;
using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Interfaces;
using BatbyEducation.Infrastructure.Data;
using BatbyEducation.Infrastructure.Events;
using BatbyEducation.Infrastructure.Repositories;
using BatbyEducation.Web.Components;
using BatbyEducation.Web.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

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

QuestPDF.Settings.License = LicenseType.Community;

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

app.MapGet("/api/invoices/{id:guid}/pdf", async (Guid id, IInvoiceRepository invoiceRepo, IStudentRepository studentRepo) =>
{
    var invoice = await invoiceRepo.GetByIdAsync(id);
    if (invoice is null) return Results.NotFound();

    var student = await studentRepo.GetByIdAsync(invoice.StudentId);
    if (student is null) return Results.NotFound();

    var document = InvoicePdfGenerator.Generate(invoice, student);
    var pdf = document.GeneratePdf();

    return Results.File(pdf, "application/pdf", $"{invoice.InvoiceNumber}.pdf");
});

app.Run();
