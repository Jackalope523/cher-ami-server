# Cher Ami API

Back-end REST API for **[Cher Ami](https://www.thecherami.com)**, a social platform that turns private group sharing into a physical keepsake. Members post text and photos inside private **Circles**; each Circle's posts are automatically compiled into a monthly **Issue** and rendered as a print-ready magazine delivered to that Circle's recipients.

Built with **ASP.NET Core 9.0 (.NET 9)** and hosted on Azure.

> **What this project demonstrates:** a production, cloud-hosted API with real payment processing, multi-provider auth, third-party integrations, background job scheduling, and a layered architecture designed for testability. It is a full backend system, not a demo.

---

## What it does

| Capability | Detail |
|---|---|
| **Multi-provider authentication** | Sign in with Apple, Google, or email/password — all issuing JWTs signed with a private key stored in Azure Key Vault. |
| **Circles** | Create and manage private groups, invite members with rerollable invite codes, and manage the list of postal recipients. |
| **Posts & Issues** | Members upload posts (text + images); posts are gathered into monthly issues per Circle. |
| **Magazine publishing** | A scheduled background job renders each issue into a print-ready PDF magazine. |
| **Subscriptions & billing** | Full Stripe integration — subscription pricing, saved payment methods, and webhook-driven state updates. |
| **Notifications & email** | Push notifications via OneSignal, transactional email via SendGrid. |
| **Trust & safety** | User blocking plus post and user reporting for moderation. |

---

## Tech stack

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core 9.0 (.NET 9) |
| API style | [FastEndpoints](https://fast-endpoints.com/) — one class per endpoint, no controllers |
| Data access | Entity Framework Core 9 (SQL Server / Azure SQL, PostgreSQL, and SQLite providers) |
| Identity & auth | ASP.NET Core Identity + JWT bearer tokens |
| Secrets | Azure Key Vault |
| Cloud storage | Azure Blob Storage, Tables, Queues, and File Shares |
| Scheduling | Quartz.NET background jobs |
| PDF generation | QuestPDF |
| Payments | Stripe |
| Email | SendGrid |
| Push notifications | OneSignal |
| Image processing | SixLabors.ImageSharp |
| Logging | Serilog (console + Azure App Service sinks) |
| API docs | Swagger / Swashbuckle |
| Testing | xUnit, Moq, and NSubstitute across service, unit, and integration test projects |

---

## Architecture

The codebase is organized into a clean, layered flow that keeps business logic independent of both the web framework and the database:

```
Endpoint  →  Service  →  Repository  →  EF Core / Azure SQL
 (HTTP)      (business)   (data access)
```

- **Endpoints** parse claims and route values, call a service, and shape the HTTP response. They contain **no** database access.
- **Services** hold the business flows and depend on repository *interfaces*, so they can be unit-tested against mocked data and mocked third-party integrations (image storage, push, Stripe).
- **Repositories** own all EF Core queries. Their methods are named after the *domain question* they answer (`ShareCommonCircleAsync`) rather than the query mechanics — the calling code reads like the business rule it implements.

### Project layout

```
src/CherAmiAPI/
  Endpoints/       One class per route, grouped by feature (Auth, Circles,
                   Issues, Posts, Recipients, PaymentMethods, Stripe, Users, …)
  Services/        Domain/business logic (auth, billing, circles, posts, images, …)
  Repositories/    EF Core data access behind interfaces
  Interfaces/      Abstractions that make services and integrations mockable
  Entities/        EF Core entities (User, Circle, Recipient, Post, Issue,
                   Subscription, Notification, Block, Report, …)
  Contexts/        ApplicationDbContext + environment-specific contexts
  BackgroundJobs/  Quartz jobs (magazine publishing, subscription updates, …)
  Components/      QuestPDF layout components for magazine rendering
  Exceptions/      Custom exceptions mapped to HTTP responses
  Migrations/      EF Core migrations per database target
  Program.cs       Composition root: DI, auth, CORS, Quartz, Swagger

tests/
  CherAmiAPI.Tests/             Service-layer tests (xUnit + NSubstitute)
  CherAmiAPI.UnitTests/         Unit tests (xUnit + Moq)
  CherAmiAPI.IntegrationTests/  Repository / end-to-end integration tests
```
