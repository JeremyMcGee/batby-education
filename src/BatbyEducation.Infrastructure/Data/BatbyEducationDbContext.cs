using BatbyEducation.Domain.Common;
using BatbyEducation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BatbyEducation.Infrastructure.Data;

public class BatbyEducationDbContext : DbContext
{
    private readonly IDomainEventDispatcher? _domainEventDispatcher;

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Tutor> Tutors => Set<Tutor>();
    public DbSet<TutorAvailability> TutorAvailabilities => Set<TutorAvailability>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<StudentAccount> StudentAccounts => Set<StudentAccount>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    public BatbyEducationDbContext(DbContextOptions<BatbyEducationDbContext> options)
        : base(options)
    {
    }

    public BatbyEducationDbContext(
        DbContextOptions<BatbyEducationDbContext> options,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BatbyEducationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events from all tracked entities before saving
        var entitiesWithEvents = ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Clear events from entities before saving to avoid duplicate dispatch
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        // Persist changes
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch collected events after successful save
        if (_domainEventDispatcher is not null && domainEvents.Any())
        {
            await _domainEventDispatcher.DispatchEventsAsync(domainEvents);
        }

        return result;
    }
}
