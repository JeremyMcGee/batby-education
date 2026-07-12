# Infrastructure

## Overview

The system runs as a single ASP.NET Core application with an embedded SQLite database. There is no separate database server, message broker, or external service dependency. This makes it straightforward to deploy and operate for a small tutoring business.

## Components

```
┌────────────────────────────────────┐
│   ASP.NET Core (Blazor Server)     │
│   ┌────────────────────────────┐   │
│   │   Kestrel Web Server       │   │
│   ├────────────────────────────┤   │
│   │   Blazor SignalR Hub       │   │
│   ├────────────────────────────┤   │
│   │   EF Core + SQLite         │   │
│   └────────────────────────────┘   │
│              ↓                     │
│   ┌────────────────────────────┐   │
│   │   batbyeducation.db        │   │ (SQLite file)
│   └────────────────────────────┘   │
└────────────────────────────────────┘
```

## Database

**Engine:** SQLite 3 via `Microsoft.EntityFrameworkCore.Sqlite`

**File:** `batbyeducation.db` (created automatically on first run in the working directory)

**Migrations:** Applied automatically on application startup via `DatabaseMigrationExtensions.MigrateDatabaseAsync()`. No manual migration steps needed.

### Tables

| Table | Purpose |
|-------|---------|
| `Students` | Student records with optional hourly rate (`HourlyRate`) and default booking preferences (`DefaultTutorId`, `DefaultSubject`, `DefaultDay`, `DefaultStartTime`) |
| `Tutors` | Tutor records with subjects and default rate |
| `TutorAvailabilities` | Recurring and one-off availability slots |
| `Sessions` | Tutoring sessions with state machine and optional per-session `RateOverride` |
| `Invoices` | Billing documents with line items |
| `InvoiceLineItems` | Individual charges per session |
| `Payments` | Payment records against invoices |
| `LedgerEntries` | Cash and Bank Transfer ledger transactions |
| `StudentAccounts` | Credit balances from overpayments |
| `AuditEntries` | Field-level change history for students |

### Indexes

- Unique on `Students.Email`, `Tutors.Email`, `Invoices.InvoiceNumber`
- Composite on `TutorAvailabilities(TutorId, DayOfWeek)`
- Composite on `LedgerEntries(LedgerType, EntryDate)`
- Foreign key indexes on all FK columns

## Hosting Options

### Local Development

Run directly with `dotnet run`. The SQLite file is created in the application's working directory. No setup required beyond the .NET 8 SDK.

### Azure App Service (Recommended for Production)

The application deploys to Azure App Service with minimal configuration:

- **Plan:** Basic B1 or Free F1 tier is sufficient for a small business
- **Runtime:** .NET 8 (Linux or Windows)
- **Database:** SQLite file stored on App Service's persistent local storage (`/home/site/` on Linux)
- **Deployment:** `dotnet publish -c Release` then deploy via ZIP deploy, GitHub Actions, or `az webapp deploy`

**Key considerations:**
- Single instance only (SQLite doesn't support concurrent access from multiple app instances)
- Back up the `.db` file periodically (e.g., copy to Azure Blob Storage via a scheduled task)
- The connection string in `Program.cs` uses `Data Source=batbyeducation.db` — update the path for production if needed (e.g., `Data Source=/home/site/data/batbyeducation.db`)

### Upgrading to Azure SQL (Future)

If the business grows and needs multi-instance scaling:

1. Add `Microsoft.EntityFrameworkCore.SqlServer` package
2. Change the connection string to an Azure SQL connection string
3. Change `UseSqlite(...)` to `UseSqlServer(...)` in `Program.cs`
4. Generate a new initial migration targeting SQL Server
5. No application code changes needed — the repository layer and domain are database-agnostic

## Security Considerations

- The application currently has no authentication (designed for single-user/office use)
- For production deployment behind Azure App Service, consider enabling Azure AD authentication via Easy Auth
- The SQLite database file should not be publicly accessible — ensure it's outside the `wwwroot` folder
- Connection strings should be managed via Azure App Configuration or environment variables in production

## Monitoring

- Standard ASP.NET Core logging (console by default)
- For Azure: enable Application Insights for request tracing, exception logging, and performance metrics
- The application logs EF Core SQL queries at Debug level

## Backup Strategy

Since SQLite is a single file:

1. **Manual:** Copy `batbyeducation.db` to a safe location periodically
2. **Automated (Azure):** Use a WebJob or Azure Function on a timer to copy the file to Blob Storage
3. **Before upgrades:** Always back up the database file before applying new migrations

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Set to `Production` for deployed environments |
| `ASPNETCORE_URLS` | `https://localhost:5001;http://localhost:5000` | Listening URLs |

The connection string is currently hardcoded in `Program.cs`. For production, move it to `appsettings.Production.json` or an environment variable.
