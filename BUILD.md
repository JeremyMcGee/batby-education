# Build & Run Instructions

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or later — .NET 9/10 also work)
- No other dependencies required (SQLite is embedded, no Docker needed)

Verify your SDK:

```bash
dotnet --version
# Should show 8.0.x or higher
```

## Build

From the repository root:

```bash
dotnet build
```

This builds all 7 projects (4 source + 3 test). First build will restore NuGet packages automatically.

## Run

```bash
dotnet run --project src/BatbyEducation.Web
```

The application will:
1. Create the SQLite database file (`batbyeducation.db`) if it doesn't exist
2. Apply any pending EF Core migrations automatically
3. Start the Blazor Server on `https://localhost:5001` (and `http://localhost:5000`)

Open your browser to the URL shown in the console output.

## Run Tests

```bash
dotnet test
```

Runs all tests across the three test projects. The optional property-based tests (FsCheck) are included when implemented.

To run a specific test project:

```bash
dotnet test tests/BatbyEducation.Domain.Tests
dotnet test tests/BatbyEducation.Application.Tests
dotnet test tests/BatbyEducation.Integration.Tests
```

## Publish for Deployment

```bash
dotnet publish src/BatbyEducation.Web -c Release -o ./publish
```

This produces a self-contained deployment in `./publish/`. To run it:

```bash
cd publish
dotnet BatbyEducation.Web.dll
```

Or deploy the `./publish` folder to Azure App Service.

## Database Management

### Migrations

Migrations are applied automatically on startup. To manage them manually:

```bash
# Add a new migration (after changing entities/configurations)
dotnet ef migrations add MigrationName --project src/BatbyEducation.Infrastructure --startup-project src/BatbyEducation.Web

# Apply migrations manually (not usually needed)
dotnet ef database update --project src/BatbyEducation.Infrastructure --startup-project src/BatbyEducation.Web

# Revert last migration
dotnet ef migrations remove --project src/BatbyEducation.Infrastructure --startup-project src/BatbyEducation.Web
```

### Reset Database

Delete `batbyeducation.db` and restart the application. Migrations will recreate it from scratch.

## Development Workflow

1. Make changes to domain entities or application handlers
2. Run `dotnet build` to check compilation
3. Run `dotnet test` to verify nothing is broken
4. If you changed entity properties or relationships, add an EF Core migration
5. Run with `dotnet run --project src/BatbyEducation.Web` to test in browser

## Configuration

The main configuration is in `src/BatbyEducation.Web/Program.cs`:

- **Database connection string:** `"Data Source=batbyeducation.db"` — change for production
- **DI registrations:** All repositories, MediatR, FluentValidation, domain event dispatcher

For production, create `src/BatbyEducation.Web/appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=/path/to/production/batbyeducation.db"
  }
}
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Port already in use | Kill the existing process or change URLs via `ASPNETCORE_URLS` |
| Database locked | Ensure only one instance is running (SQLite limitation) |
| Migration errors | Delete `batbyeducation.db` and restart for a fresh database |
| Build errors after pulling | Run `dotnet restore` then `dotnet build` |
| EF Core tools not found | Install with `dotnet tool install --global dotnet-ef` |
