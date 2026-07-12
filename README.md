# Batby Education — Tutoring Booking System

A booking and accounting system for a small tutoring business, built with .NET 8 and Blazor Server.

## What It Does

- **Student & Tutor Management** — Register students (with optional per-student hourly rates) and tutors, manage tutor availability
- **Default Booking Preferences** — Configure per-student defaults (default tutor, subject, day, and timeslot) to streamline repeat bookings
- **Session Booking** — Select a student first (auto-populates tutor, subject, day, and timeslot from their defaults), then book with conflict detection, availability checking, available slot picker, and computed rate display
- **Per-Session Rate Overrides** — Override the hourly rate on individual sessions when needed
- **Effective Rate Priority Chain** — Invoice amounts resolve using: session rate override > student hourly rate > tutor hourly rate
- **Attendance Tracking** — Completion form includes an "Attended" checkbox; unchecked triggers a no-show (late cancellation for billing)
- **Session Lifecycle** — Cancel, reschedule, and mark sessions as completed with full audit trails
- **Invoicing** — Generate invoices applying the rate priority chain automatically
- **Payment Tracking** — Record cash and bank transfer payments with separate ledgers
- **Financial Reporting** — Revenue reports, tutor earnings, outstanding balances, overdue invoices, and UK tax year summaries (6 April – 5 April)
- **Calendar View** — Weekly calendar with student name shown first and emboldened, tutor/student filtering, and status colour coding

## Documentation

| Document | Description |
|----------|-------------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | System architecture, patterns, and project structure |
| [INFRASTRUCTURE.md](INFRASTRUCTURE.md) | Infrastructure setup, database, and deployment |
| [BUILD.md](BUILD.md) | Build, run, and test instructions |

## Quick Start

```bash
dotnet build
dotnet run --project src/BatbyEducation.Web
```

Then open `https://localhost:5001` (or the port shown in the console).

## Tech Stack

- .NET 8 / C# 12
- Blazor Server (interactive UI)
- SQLite (via EF Core)
- MediatR (CQRS)
- FluentValidation
- FsCheck + xUnit (property-based testing)

## Project Layout

```
src/
├── BatbyEducation.Domain/           # Entities, value objects, interfaces
├── BatbyEducation.Application/      # Commands, queries, handlers, validators
├── BatbyEducation.Infrastructure/   # EF Core, repositories, migrations
└── BatbyEducation.Web/              # Blazor Server UI

tests/
├── BatbyEducation.Domain.Tests/     # Domain unit + property tests
├── BatbyEducation.Application.Tests/# Application layer tests
└── BatbyEducation.Integration.Tests/# Integration tests
```

## Licence

Private — Batby Education.
