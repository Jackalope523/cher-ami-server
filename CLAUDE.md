# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet restore                        # Restore NuGet packages
dotnet build CherAmiAPI.csproj        # Build (name the csproj — a stray .proj file breaks bare `dotnet build`)
dotnet run --project CherAmiAPI       # Run the API (https://localhost:5001)
```

Swagger UI is available at `/swagger` when running locally.

## Migrations

**There are three migration histories, not one.** Getting this wrong is how a
deploy breaks.

| Folder | Context | State |
|---|---|---|
| `Migrations/` | `ApplicationDbContext` | local/dev only, last touched November 2025 |
| `Migrations/AzureSQLStaging/` | `AzureSQLStagingContext` | **the deployed staging history** |
| `Migrations/AzureSQLProduction/` | `AzureSQLProductionContext` | **the deployed production history** |

When adding a column, write the same migration into `AzureSQLStaging` **and**
`AzureSQLProduction` with an identical timestamp ID, and update both snapshots.
Leave the base `Migrations/` folder alone — adding to it produces a third
migration nobody applies. One logical change stays one migration per deployment
target. `Add Post PhotoDate`, `Add User Onboarding Flags` and `Add Circle
LastPhotoPushAt` are the pattern to copy.

Generate against **both** contexts and read both results before committing:

```bash
dotnet ef migrations add "<Name>" --project CherAmiAPI.csproj --startup-project CherAmiAPI.csproj --context AzureSQLStagingContext --output-dir Migrations/AzureSQLStaging
```

Both `--project` and `--startup-project` are required because the stray `.proj`
file in the repository root makes the folder ambiguous. The command logs
`Error: Value cannot be null. (Parameter 'uriString')` for the Key Vault URI and
then continues without the application service provider — that is expected, and
it is still enough to diff the model.

**A migration that comes out carrying objects you didn't change is that folder's
history telling you it has drifted.** Staging drifted this way between February
and July 2026; `20260918150000_Add Circle LastPhotoPushAt` repairs it with
guarded `IF COL_LENGTH(...) IS NULL` SQL.

## Architecture

**CherAmiAPI** is an ASP.NET Core 9.0 REST API for a social community/gathering platform. It uses the **FastEndpoints** library instead of Controllers — each endpoint is a class in `/Endpoints/`, organized by feature domain (Auth, Circles, Posts, Recipients, Users, Stripe, Media, etc.).

### Key patterns

- **FastEndpoints**: Each endpoint inherits from `Endpoint<TRequest, TResponse>`. Route, auth policy, and validation are declared inside the endpoint class.
- **EF Core + multiple DB contexts**: `ApplicationDbContext` is the main context; `AzureSQLProductionContext` and `AzureSQLStagingContext` extend it for environment-specific configuration. Supports SQL Server, PostgreSQL, and SQLite.
- **Soft deletes**: Entities use a soft-delete pattern enforced via EF Core query filters — deleted records are filtered out automatically.
- **JWT authentication**: Tokens are signed with a key fetched from Azure Key Vault via `IKeyService`. Multi-provider login: Apple ID, Google, and email/password.
- **Azure-heavy**: Blob Storage (images), Tables, Queues, Key Vault secrets, and File Shares are all used. Local development requires Azure Key Vault access.
- **Background jobs**: Quartz scheduler is wired up in `Program.cs`; one-off migration jobs stay commented out. Three are scheduled and live: `PublishMagazinesJob` (1st, 05:05 UTC), `IssueRemindersJob` (daily, 17:00 UTC) and `PhotoActivityJob` (every 15 min). All three need **Always On** on the App Service or they won't fire while the app is idle. Jobs live in `/BackgroundJobs/`.
- **Global error handling**: `ExceptionHandler.cs` maps custom exception types (in `/Exceptions/`) to HTTP status codes and returns RFC 7807 problem details.

### External integrations

| Service | Purpose |
|---|---|
| Stripe | Subscription billing |
| OneSignal | Push notifications **and all email** — transactional and lifecycle, sent as templates (`OneSignalService`) |
| QuestPDF | PDF generation (magazines) |
| Azure Key Vault | Secret management |
| Azure Blob Storage | Image/media storage |

### Code conventions

Per the project README, the codebase follows Uncle Bob's Clean Code naming principles and Microsoft C# coding conventions. Internal tests are preferred over public ones (compiler protection). Pull requests require formal review.

### Version gating

`GetConfigEndpoint` returns `MinimumVersion`; the app refuses to run below it.
**Minor versions denote minimums** — bump to `1.1.0` when a release raises the
floor, and leave the minimum alone for patch releases. The app compares parts
numerically (`isVersionBelow` in `lib/utility.ts`), because `'1.0.10' < '1.0.9'`
is true as a string.

The deprecated `Version` field is frozen at `1.0.5` so that any build still
running the old equality check can't be walled. Remove it once nothing below
1.0.10 is in the wild.

### Environment notes

- `Development` environment is not supported — the app expects either `Staging` or `Production` (Azure-hosted).
- Launch profiles are defined in `Properties/launchSettings.json`: `IIS Express` (local IIS) and `Web` (direct .NET on ports 5001/5000).
- There are no automated test projects in the repository.
