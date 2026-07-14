# Cher Ami Server

Back-end REST API for [Cher Ami](https://www.thecherami.com) — a social platform where members share posts within private **Circles**, and each Circle's posts are compiled into monthly magazine **Issues** delivered to its recipients.

Built with **ASP.NET Core 9.0** and [FastEndpoints](https://fast-endpoints.com/), hosted on Azure.

## Features

- **Multi-provider authentication** — Sign in with Apple, Google, or email/password, issuing JWTs signed with a key stored in Azure Key Vault.
- **Circles** — Create and manage private groups, invite members via invite codes, and manage recipients.
- **Posts & Issues** — Members upload posts (text + images) that are gathered into monthly issues.
- **Magazine publishing** — A Quartz background job renders each issue into a print-ready PDF magazine with [QuestPDF](https://www.questpdf.com/).
- **Subscriptions & billing** — Stripe integration for subscription pricing, payment methods, and webhooks.
- **Notifications & email** — Push notifications via OneSignal, transactional email via SendGrid.
- **Moderation** — User blocking, and post/user reporting.

## Tech stack

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core 9.0 (.NET 9) |
| API style | FastEndpoints (no controllers) |
| Data access | Entity Framework Core 9 — SQL Server (Azure SQL), PostgreSQL, and SQLite providers |
| Identity & auth | ASP.NET Core Identity + JWT bearer tokens (FastEndpoints.Security) |
| Secrets | Azure Key Vault |
| Storage | Azure Blob Storage (images), Tables, Queues, File Shares |
| Scheduling | Quartz.NET |
| PDF generation | QuestPDF |
| Payments | Stripe |
| Email | SendGrid |
| Push notifications | OneSignal |
| Logging | Serilog (console + Azure App Service sinks) |
| Image processing | SixLabors.ImageSharp |
| API docs | Swashbuckle / Swagger |

## Project structure

The project lives at the repository root (`CherAmiAPI.csproj`).

```
Endpoints/         API endpoints, one class per route, grouped by feature:
                   Auth (Apple, Google, Email), Circles, Config, Issues, Media,
                   PaymentMethods, Posts, Recipients, Stripe, Users, Website
Entities/          EF Core entities (User, Circle, Recipient, Post, Issue,
                   Subscription, Notification, Block, Report, ...)
Contexts/          ApplicationDbContext plus environment-specific contexts
                   (AzureSQLProductionContext, AzureSQLStagingContext)
Services/          Domain services (images, invite codes, keys, OneSignal, ...)
Interfaces/        Service abstractions
BackgroundJobs/    Quartz jobs (magazine publishing, subscription updates, ...)
Exceptions/        Custom exceptions mapped to HTTP responses
Components/        QuestPDF layout components for magazine rendering
Migrations/        EF Core migrations per database target
Shared/            Mappers and shared helpers
Assets/            Fonts and images bundled for PDF generation
Program.cs         Composition root: DI, auth, CORS, Quartz, Swagger
ExceptionHandler.cs Global error handler returning RFC 7807 problem details
```

### Key patterns

- **FastEndpoints** — every endpoint is a class inheriting `Endpoint<TRequest, TResponse>`; route, auth policy, and validation are declared inside the class.
- **Soft deletes** — entities are soft-deleted and filtered out automatically via EF Core global query filters.
- **Global error handling** — custom exceptions in `/Exceptions/` are translated to appropriate HTTP status codes with problem-details bodies.
- **Environment-specific DB contexts** — the environment name selects which context (and therefore which database) is registered at startup.

## Getting started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Access to the Azure Key Vault used for secrets (configuration is loaded from Key Vault at startup via `DefaultAzureCredential` — sign in with `az login` or Visual Studio using an account that has vault access)
- `KEY_VAULT_URI` available in configuration (environment variable or app setting)

> **Note:** the `Development` environment is intentionally unsupported — the app throws at startup unless `ASPNETCORE_ENVIRONMENT` is `Staging` or `Production`, which select the corresponding Azure SQL database.

### Build & run

```bash
dotnet restore
dotnet build
dotnet run          # Web profile: https://localhost:5001, http://localhost:5000
```

Swagger UI is available at `/swagger`.

Launch profiles are defined in `Properties/launchSettings.json`: **IIS Express** (local IIS) and **Web** (Kestrel on ports 5001/5000).

### Database migrations

```bash
dotnet ef migrations add <MigrationName>   # create a migration
dotnet ef database update                  # apply pending migrations
```

Migrations are kept per database target under `Migrations/`.

## Conventions

### Code

Naming conventions should follow [these rules](https://dzone.com/articles/naming-conventions-from-uncle-bobs-clean-code-phil).

Layout and language practices should observe [Microsoft's guidelines](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions#layout-conventions).

### Unit tests

Unit tests should follow [these conventions](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices).

All tests should be internal to allow for compiler agreement with tests that take objects marked as internal as parameters.<br>
This also acts to protect all tests from unintended outside use.

### Pull requests

All changes land through pull requests with formal review.
