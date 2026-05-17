# ATS Backend API

A backend REST API for an Applicant Tracking System (ATS) built with ASP.NET Core 8 and PostgreSQL.

## What it does

- **Job management** — create and list job postings with status and pagination
- **Candidate applications** — candidates apply to jobs; duplicate applications (same email + job) are rejected
- **Stage pipeline** — applications move through a defined workflow: `Applied → Screening → Interview → Offer → Hired/Rejected`; invalid transitions are rejected with a clear error
- **Stage history** — every stage change is recorded as an immutable audit log with who made the change and why
- **Recruiter notes** — team members can attach typed notes (General, Screening, Interview, ReferenceCheck, RedFlag) to any application
- **Scoring** — recruiters score candidates across three independent dimensions: CultureFit, Interview, Assessment (1–5 scale, upsert semantics)

---

## Tech Stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL |
| Testing | xUnit + FluentAssertions + WebApplicationFactory |
| Documentation | Swagger / OpenAPI |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- PostgreSQL running locally (default: `localhost:5432`)

---

## How to Run Locally

### 1. Configure the database password

Non-sensitive config (`Host`, `Port`, `Name`, `Username`) lives in `appsettings.json`.  
The password is stored in .NET User Secrets and never committed to source control.

```bash
dotnet user-secrets set "Database:Password" "your-postgres-password"
```

The defaults in `appsettings.json` are:

```json
"Database": {
  "Host": "localhost",
  "Port": 5432,
  "Name": "ats_db",
  "Username": "postgres"
}
```

Update these values if your local PostgreSQL is configured differently.

### 2. Apply migrations

Migrations run automatically on startup via `db.Database.MigrateAsync()`.  
To apply manually:

```bash
dotnet ef database update
```

### 3. Run the API

```bash
dotnet run
```

The API starts at `http://localhost:5127`.  
Swagger UI is available at: `http://localhost:5127/swagger`

---

## How to Run Tests

The test project uses an in-memory database and requires no running PostgreSQL or Redis instance.

```bash
dotnet test Ats.Api.Tests/Ats.Api.Tests.csproj
```

Tests live in `Ats.Api.Tests/Integration/`. The `CustomWebApplicationFactory` boots the full API in-process with EF Core InMemory, giving real HTTP-level integration coverage without external dependencies.

---

## Seed Data

Three team members are seeded on first startup. Use their IDs as the `X-Team-Member-Id` header when calling any endpoint that requires recruiter identity (stage changes, notes, scores).

| Name | Role | ID |
|---|---|---|
| Alice Nguyen | Recruiter | `11111111-1111-1111-1111-111111111111` |
| Bob Smith | Hiring Manager | `22222222-2222-2222-2222-222222222222` |
| Carol James | Recruiter | `33333333-3333-3333-3333-333333333333` |

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| POST | `/api/jobs` | Create a job posting |
| GET | `/api/jobs` | List jobs (paginated, filterable by status) |
| GET | `/api/jobs/{id}` | Get a job by ID |
| POST | `/api/jobs/{jobId}/applications` | Submit a candidate application |
| GET | `/api/jobs/{jobId}/applications` | List applications for a job (filterable by stage) |
| GET | `/api/applications/{id}` | Full application profile (notes, scores, history) |
| PATCH | `/api/applications/{id}/stage` | Move application to next stage |
| POST | `/api/applications/{id}/notes` | Add a recruiter note |
| GET | `/api/applications/{id}/notes` | List notes for an application |
| PUT | `/api/applications/{id}/scores/{dimension}` | Set or update a score (culture-fit / interview / assessment) |

---

## Assumptions Made

- **No authentication** — team member identity is passed via the `X-Team-Member-Id` request header. In a production system this would be replaced by JWT claims.
- **No frontend** — this is a pure backend API.
- **Redis is configured but not used for caching** — the infrastructure is wired up in `Program.cs` and the `GET /applications/{id}` endpoint is the intended cache target, but active caching was not implemented within the scope of this project.
- **Soft validation only** — there is no role-based access control. Any valid team member can perform any action.
- **CoverLetter is stored as empty string when not provided** — the model does not allow null; this is a minor schema decision.

---

## Design Decisions

**Service layer for business logic**  
Controllers are thin HTTP adapters — they parse requests, call services, and return responses. All validation rules, state machine logic, and database operations live in `ApplicationsService` and `JobsService`. This keeps business logic testable in isolation and prevents controllers from becoming god objects.

**DTOs instead of EF entities**  
EF entities model the database. DTOs model the API contract. Exposing entities directly would leak schema details, cause serialization issues with navigation properties, and make it impossible to evolve the database schema without breaking clients. Every response is explicitly shaped by a DTO.

**StageHistory as an append-only audit log**  
Stage changes are never overwritten — each transition creates a new row recording who changed it, from what stage, to what stage, and why. This is critical in hiring workflows where decisions may be audited or challenged.

**Scores as upsert with separate dimensions**  
Each dimension (`CultureFit`, `Interview`, `Assessment`) is a separate row with a unique index on `(ApplicationId, Dimension)`. A recruiter can update a score at any time and the record reflects the latest value. The unique index prevents duplicate rows at the database level even under concurrent writes.

**Notes attributed to TeamMembers**  
The creator's identity is never trusted from the request body — it always comes from the `X-Team-Member-Id` header. This prevents attribution spoofing.

---

## What I Would Improve With More Time

- **Redis caching** for `GET /applications/{id}` — this endpoint JOINs four tables and is called frequently by recruiters reviewing candidates. It would be invalidated on stage change, note addition, or score update.
- **JWT authentication** to replace the header-based identity system with proper claims and role enforcement.
- **Pagination on notes and stage history** for applications with large histories.
- **Docker Compose setup** to make local onboarding a single `docker compose up` command.
- **More integration tests** — happy path and edge cases for stage transitions, duplicate application rejection, invalid score ranges, and missing headers.
- **CI/CD pipeline** with GitHub Actions running `dotnet test` on every push.
- **Structured logging** with Serilog or the built-in structured logging providers, with request/response logging middleware.
