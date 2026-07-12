# Architecture

## Overview

The system follows **Clean Architecture** with **CQRS** (Command Query Responsibility Segregation) and **Domain-Driven Design** principles. Business logic is isolated in the domain layer, completely independent of infrastructure concerns.

## Layers

```
┌─────────────────────────────────────────────┐
│           Presentation (Blazor Server)       │
├─────────────────────────────────────────────┤
│           Application (CQRS Handlers)        │
├─────────────────────────────────────────────┤
│           Domain (Entities & Rules)          │
├─────────────────────────────────────────────┤
│           Infrastructure (EF Core / SQLite)  │
└─────────────────────────────────────────────┘
```

### Domain Layer (`BatbyEducation.Domain`)

Pure C# with zero external dependencies (aside from MediatR.Contracts for the `INotification` marker interface on domain events).

Contains:
- **Entities** — `Student` (with `HourlyRate`, `DefaultTutorId`, `DefaultSubject`, `DefaultDay`, `DefaultStartTime`), `Tutor`, `Session` (with `RateOverride`), `Invoice`, `Payment`, `LedgerEntry`, `StudentAccount`, `AuditEntry`, `TutorAvailability`, `InvoiceLineItem`
- **Value Objects** — `Money`, `DateRange`, `EmailAddress`
- **Enumerations** — `SessionStatus`, `InvoiceStatus`, `PaymentMethod`
- **Domain Events** — `SessionCompletedEvent`, `SessionCancelledEvent`, `PaymentRecordedEvent`, `InvoicePaidEvent`
- **Repository Interfaces** — All data access contracts live here (Dependency Inversion)
- **Exceptions** — `DomainException`, `BookingConflictException`, `InvalidStateTransitionException`
- **Common** — `Entity` base class, `AggregateRoot`, `Result<T>`, `IDomainEvent`, `IDomainEventDispatcher`

### Application Layer (`BatbyEducation.Application`)

Orchestrates use cases through commands and queries using MediatR.

- **Commands** — Write operations (register student, book session, record payment, etc.)
- **Queries** — Read operations (calendar, reports, balances)
- **Validators** — FluentValidation rules applied before handlers execute
- **DTOs** — Data transfer objects for query responses

### Infrastructure Layer (`BatbyEducation.Infrastructure`)

Implements persistence and external concerns.

- **Data** — `BatbyEducationDbContext`, entity type configurations, migrations, design-time factory
- **Repositories** — EF Core implementations of all domain interfaces
- **Events** — `DomainEventDispatcher` using MediatR notifications

### Presentation Layer (`BatbyEducation.Web`)

Blazor Server application with interactive server-side rendering.

- **Pages** — Razor components for each functional area (Students, Tutors, Sessions, Calendar, Invoices, Payments, Reports)
- **Layout** — Shared layout with navigation sidebar
- **Program.cs** — DI composition root, middleware pipeline

## Bounded Contexts

The system has two bounded contexts that communicate through domain events:

1. **Booking Context** — Students, Tutors, Sessions, Availability
2. **Accounting Context** — Invoices, Payments, Ledgers, Student Accounts

When a session is completed, a `SessionCompletedEvent` is raised. The accounting context listens for these to know which sessions are billable.

## Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| Clean Architecture | Testable domain logic independent of frameworks |
| CQRS via MediatR | Separates read/write concerns, keeps handlers focused |
| Rich Domain Entities | Business rules encapsulated in entities (e.g., Session state machine) |
| Result<T> Pattern | Explicit error handling without exceptions for validation |
| Domain Events | Loose coupling between bounded contexts |
| SQLite | Simple deployment for a single-user small business app |
| Blazor Server | Rich interactive UI without a separate frontend build |

## Rate Priority Chain

When calculating invoice line items, the system resolves the effective rate using:

1. **Session Rate Override** — If the session has a `RateOverride` set, use it
2. **Student Hourly Rate** — Else if the student has a `HourlyRate` set, use it
3. **Tutor Hourly Rate** — Else fall back to the tutor's configured rate

## Session State Machine

```
Scheduled ──→ Completed
    │              
    ├──→ Cancelled (+ IsLateCancellation if < 24h before start)
    │
    ├──→ Rescheduled (creates new linked session)
    │
    └──→ RequiresRescheduling (tutor availability removed)

Scheduled ──→ PendingConfirmation (session end time passed) ──→ Completed
```

## Dependency Flow

```
Web → Application → Domain ← Infrastructure
         ↓                         ↑
    (uses interfaces)      (implements interfaces)
```

The Domain layer defines interfaces. Infrastructure implements them. Application depends only on Domain. Web composes everything via DI.
