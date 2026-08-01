# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet restore                                    # Restore NuGet packages
dotnet build CherAmiAPI.sln                       # Build the API and all test projects
dotnet run --project src/CherAmiAPI               # Run the API (https://localhost:5001)
dotnet test CherAmiAPI.sln                        # Run all test projects
dotnet ef migrations add <MigrationName> --project src/CherAmiAPI  # Create a new EF Core migration
dotnet ef database update --project src/CherAmiAPI                 # Apply pending migrations
```

Swagger UI is available at `/swagger` when running locally.

## Architecture

**CherAmiAPI** is an ASP.NET Core 9.0 REST API for a social community/gathering platform. It uses the **FastEndpoints** library instead of Controllers — each endpoint is a class in `/Endpoints/`, organized by feature domain (Auth, Circles, Posts, Recipients, Users, Stripe, Media, etc.).

### Key patterns

- **FastEndpoints**: Each endpoint inherits from `Endpoint<TRequest, TResponse>`. Route, auth policy, and validation are declared inside the endpoint class.
- **Endpoints → services → repositories**: Endpoints contain no EF Core access — they parse claims/route values, call a service class in `/Services/`, and map/send the response. Services hold the business flows and depend on repository interfaces (in `/Interfaces/`, implemented in `/Repositories/`) for all EF Core queries. Repository methods are named after the domain question (`ShareCommonCircleAsync`), not the query mechanics. Services are concrete classes registered scoped in `Program.cs`; repositories are the mockable seam for tests.
- **Transactions are repository-internal and database-only**: a multi-entity atomic flow is a single repository method committing with one `SaveChangesAsync` (e.g. `CircleRepository.CreateCircleAsync`). Never wrap external I/O (blob storage, Stripe, OneSignal) in a database transaction. Services instead order the calls so the database never references a blob that wasn't written: upload the blob before recording its path, and create feed-visible rows soft-deleted, finalizing them only after the upload succeeds.
- **Stripe SDK name collisions**: Stripe.net declares `BillingService`, `CustomerService`, `PaymentMethodService`, etc. Files that use both the `Stripe` and `CherAmiAPI.Services` namespaces need a using alias (see `Program.cs`).
- **EF Core + multiple DB contexts**: `ApplicationDbContext` is the main context; `AzureSQLProductionContext` and `AzureSQLStagingContext` extend it for environment-specific configuration. Supports SQL Server, PostgreSQL, and SQLite.
- **Soft deletes**: Entities use a soft-delete pattern enforced via EF Core query filters — deleted records are filtered out automatically.
- **JWT authentication**: Tokens are signed with a key fetched from Azure Key Vault via `IKeyService`. Multi-provider login: Apple ID, Google, and email/password.
- **Azure-heavy**: Blob Storage (images), Tables, Queues, Key Vault secrets, and File Shares are all used. Local development requires Azure Key Vault access.
- **Background jobs**: Quartz scheduler is wired up in `Program.cs`, though most jobs are commented out. Jobs live in `/BackgroundJobs/`.
- **Global error handling**: `ExceptionHandler.cs` maps custom exception types (in `/Exceptions/`) to HTTP status codes and returns RFC 7807 problem details.

### External integrations

| Service | Purpose |
|---|---|
| Stripe | Subscription billing |
| SendGrid | Transactional email |
| OneSignal | Push notifications |
| QuestPDF | PDF generation (magazines) |
| Azure Key Vault | Secret management |
| Azure Blob Storage | Image/media storage |

### Code conventions

Per the project README, the codebase follows Uncle Bob's Clean Code naming principles and Microsoft C# coding conventions. Internal tests are preferred over public ones (compiler protection). Pull requests require formal review.

### Environment notes

- `Development` environment is not supported — the app expects either `Staging` or `Production` (Azure-hosted).
- Launch profiles are defined in `Properties/launchSettings.json`: `IIS Express` (local IIS) and `Web` (direct .NET on ports 5001/5000).
- The solution uses a `src` + `tests` layout: the API project is `src/CherAmiAPI/`; test projects live under `tests/` — `CherAmiAPI.Tests` (xUnit + NSubstitute, service-layer tests), `CherAmiAPI.UnitTests` (xUnit v3 + Moq), and `CherAmiAPI.IntegrationTests`. Tests cover the service layer by mocking the repository and integration interfaces (`IImageService`, `IOneSignalService`, `IKeyService`, `ILoginTokenService`); the repository layer is reserved for integration tests. Stripe SDK service classes are substituted directly (their methods are virtual). `UserManager<User>` is substituted with the 9-argument constructor incantation.
