# Rental Applications

ASP.NET Core MVC application for managing rental properties, units, and lease applications.

## Solution structure

| Project | Role |
|---------|------|
| **RentalApplications.Domain** | Entities, enums, role names, pure business rules (no EF or ASP.NET) |
| **RentalApplications.Data** | EF Core `AppDbContext`, Identity user type, migrations, seeding |
| **RentalApplications.Services** | Application services behind interfaces; registered via DI |
| **RentalApplications.Web** | MVC UI (controllers, Razor views, view models) — startup project |
| **RentalApplications.Tests** | Unit tests for domain rules and services |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server Express LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)

## Run locally

Open **`RentalApplications.sln`** and set **RentalApplications.Web** as the startup project, or:

```bash
dotnet restore RentalApplications.sln
dotnet run --project RentalApplications.Web/RentalApplications.Web.csproj
```

On first run the app applies migrations and seeds sample data.

### Connection string

Configured in `RentalApplications.Web/appsettings.json` (LocalDB by default). Override in user secrets if needed.

### Migrations

The schema is a single **`InitialCreate`** migration. EF tools read **`DefaultConnection`** from `RentalApplications.Web/appsettings.json` (same as the running app).

Reset LocalDB during development:

```bash
dotnet ef database drop --force --project RentalApplications.Data/RentalApplications.Data.csproj --startup-project RentalApplications.Web/RentalApplications.Web.csproj
dotnet ef database update --project RentalApplications.Data/RentalApplications.Data.csproj --startup-project RentalApplications.Web/RentalApplications.Web.csproj
```

Or run the app after a drop; startup applies migrations and seeds automatically.

```bash
dotnet ef migrations add <Name> --project RentalApplications.Data/RentalApplications.Data.csproj --startup-project RentalApplications.Web/RentalApplications.Web.csproj
```

## Seeded accounts

| Role | Email | Password |
|------|-------|----------|
| Property manager | manager@example.com | Password123! |
| Applicant | applicant@example.com | Password123! |

Additional applicant accounts (`applicant2@example.com`, etc.) use the same password. Registration still allows choosing Applicant or Property manager.

## Tests

```bash
dotnet test RentalApplications.sln
```
