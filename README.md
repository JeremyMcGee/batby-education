# Batby Education — Tutoring Booking System

A booking and accounting system for a small tutoring business, built with .NET 8 and Blazor Server.

## What It Does

- **Student & Tutor Management** — Register students (with optional per-student hourly rates) and tutors, manage tutor availability
- **Session Booking** — Book tutoring sessions with conflict detection, availability checking, and optional per-session rate overrides
- **Session Lifecycle** — Cancel, reschedule, and mark sessions as completed with full audit trails
- **Invoicing** — Generate invoices using the Effective Rate priority chain (session override > student rate > tutor rate)
- **Payment Tracking** — Record cash and bank transfer payments with separate ledgers
- **Financial Reporting** — Revenue reports, tutor earnings, outstanding balances, overdue invoices, and UK tax year summaries (6 April – 5 April)
- **Calendar View** — Weekly calendar with tutor/student filtering and status colour coding

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
