# Meeting Room Booking API

ASP.NET Core (.NET 10) Web API for booking meeting rooms, built with Clean
Architecture, CQRS (MediatR), DDD aggregates, EF Core + PostgreSQL for writes,
Dapper for reads and Hangfire for background reconciliation. Includes a minimal
React (Vite + TypeScript) client and Swagger UI for manual checks.

## Features

- List meeting rooms with pagination (`totalCount`, `pageNumber`, `pageSize`).
- Show a room's active bookings for a selected UTC date, ordered by start time
  (bookings overlapping any part of the day are included).
- Create a booking with validated title and time range.
- Cancel a booking: the record is kept with status `Cancelled`, the time slot is
  freed.
- Consistent error responses (`ProblemDetails` + machine-readable `code` + `traceId`).
- Background auto-completion of expired bookings via Hangfire.
- Swagger UI in Development.
- Seeded rooms (`Aurora`, `Boreal`), idempotent on every startup.

## Stack

| Area | Technology |
|---|---|
| Runtime | .NET 10 / ASP.NET Core |
| Architecture | Clean Architecture (`Domain`, `Application`, `Infrastructure`, `API`) |
| CQRS | MediatR 12.x |
| Validation | FluentValidation 11.x + MediatR pipeline behavior |
| Writes | EF Core 10 + Npgsql, migrations |
| Reads | Dapper (parameterized SQL) |
| Database | PostgreSQL 17 |
| Background jobs | Hangfire 1.8.x + Hangfire.PostgreSql (schema `hangfire`) |
| API docs | Swashbuckle 10 (Swagger UI) |
| Frontend | React 19 + Vite 6 + TypeScript |
| Containers | Docker, Docker Compose |

## Project structure

```
BookingPlatformInsiders/              API: controllers, ProblemDetails handler, Program.cs
BookingPlatformInsiders.Application/  use cases (MediatR), DTOs, validators, abstractions
BookingPlatformInsiders.Domain/       entities, invariants, statuses, domain exceptions
BookingPlatformInsiders.Infrastructure/ EF Core, Dapper queries, Hangfire, seeding, migrations
client/                               React + Vite + TypeScript UI
Dockerfile, docker-compose.yml        local API + PostgreSQL stack
```

Dependency direction:

```
API -> Application -> Domain
API -> Infrastructure -> Application, Domain
```

`BookingPlatformInsiders.slnx` is stale (references a non-existent project and
misses some projects). Run commands against project files, not the solution.

## Domain model and business rules

`Room` and `Booking` are separate aggregate roots; `Booking` references the room
only by `RoomId`.

**Booking statuses:** `Scheduled = 1`, `Completed = 2`, `Cancelled = 3`
(persisted as strings). Lifecycle:

- `Scheduled -> Cancelled` (client request via `PATCH .../cancel`)
- `Scheduled -> Completed` (system only, background job)
- `Cancelled` and `Completed` are terminal.

**Invariants:**

1. `EndUtc > StartUtc`; all timestamps are UTC.
2. `StartUtc > now` at creation (bookings cannot start in the past).
3. Active bookings of the same room cannot overlap:
   `existing.StartUtc < new.EndUtc && existing.EndUtc > new.StartUtc`.
4. Intervals are half-open `[Start, End)`; a booking may start exactly when
   another ends (`start == previous end` is allowed).
5. "Active" means `Status != Cancelled`; cancelled bookings do not block slots
   and are not returned in schedules. `Completed` bookings still occupy their
   historical interval.
6. Cancellation/auto-completion are idempotent; cancelled records are kept.
7. A selected date `D` means the UTC day `[D 00:00Z, D+1 00:00Z)`; a booking is
   included when `StartUtc < D+1 day && EndUtc > D`.

**Database guarantees:**

- Unique room name, case-insensitive: `UNIQUE INDEX ... (lower("Name"))`.
- Overlap protection independent of application code:
  `EXCLUDE USING gist ("RoomId" WITH =, tstzrange("StartUtc", "EndUtc", '[)') WITH &&) WHERE ("Status" <> 'Cancelled')`.
- Optimistic concurrency on bookings via PostgreSQL `xmin` (`IsRowVersion`).
- Check constraints for capacity, time range and status values.

## API reference

Base URL locally: `http://localhost:5178` (API profile) or `http://localhost:8080`
(Docker). Swagger UI: `/swagger` in Development.

### `GET /api/rooms`

Query: `pageNumber` (default 1), `pageSize` (default 50, max 100).

```json
{
  "items": [
    {
      "id": "1496108d-27a8-48e1-b064-4cbff51b7fbe",
      "name": "Aurora",
      "capacity": 8,
      "location": "Floor 1 - East wing",
      "description": "Boardroom with video conferencing."
    }
  ],
  "totalCount": 2,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

### `GET /api/rooms/{roomId}/bookings?date=YYYY-MM-DD`

Active bookings for the UTC day, ordered by `StartUtc`; supports pagination.

```json
{
  "items": [
    {
      "id": "0ce7f58a-0b62-40dd-b187-58c942785257",
      "roomId": "1496108d-27a8-48e1-b064-4cbff51b7fbe",
      "title": "Sprint planning",
      "startUtc": "2026-11-18T10:00:00Z",
      "endUtc": "2026-11-18T11:00:00Z",
      "status": "Scheduled"
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

Errors: `404 room.not_found`; missing/invalid `date` -> `400`.

### `POST /api/bookings`

```json
{
  "roomId": "1496108d-27a8-48e1-b064-4cbff51b7fbe",
  "title": "Sprint planning",
  "startUtc": "2026-11-18T10:00:00Z",
  "endUtc": "2026-11-18T11:00:00Z"
}
```

- `201 Created` + created booking; no `Location` header (there is no
  `GET /api/bookings/{id}` endpoint).
- `400 validation.failed` (title/ids/date range), `400 booking.start_in_past`.
- `404 room.not_found`.
- `409 booking.slot_taken` (overlap, including the database race guard).

Times are accepted as ISO 8601 with an offset (`Z` or `+02:00`) and normalized
to UTC.

### `PATCH /api/bookings/{id}/cancel`

No body.

- `204 No Content` вЂ” cancelled (idempotent; repeated calls return `204`).
- `404 booking.not_found`.
- `409 booking.cannot_cancel` вЂ” booking is already `Completed`.
- `409 concurrency.conflict` вЂ” concurrent modification; retry or refresh.

### Error format

All errors use RFC 7807 `ProblemDetails` with `application/problem+json`:

```json
{
  "title": "The room already has a booking that overlaps the requested time.",
  "status": 409,
  "code": "booking.slot_taken",
  "traceId": "0HNQ3UJTPPIVA:00000003"
}
```

Validation errors additionally contain `"errors": { "Title": ["..."] }`.
`code` is a stable machine-readable identifier (`room.not_found`,
`booking.slot_taken`, `booking.start_in_past`, `booking.cannot_cancel`,
`validation.failed`, `concurrency.conflict`, `server.error`). Clients should
branch on `code`, not on the title text.

## Configuration

| Key | Purpose | Default |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string | empty locally (User Secrets), env in Docker |
| `Database:MigrateOnStartup` | Apply EF migrations on startup | `false` (Docker Compose sets `true`) |
| `Hangfire:SchemaName` | Hangfire schema | `hangfire` |
| `BackgroundJobs:AutoComplete:Cron` | Auto-complete cron (UTC) | `*/5 * * * *` |
| `BackgroundJobs:AutoComplete:BatchSize` | Rows per batch | `100` |
| `BackgroundJobs:AutoComplete:MaxBatches` | Max batches per run | `10` |
| `Cors:AllowedOrigins` | Allowed frontend origins | `http://localhost:5173` |
| `Jwt:Issuer` | JWT issuer | `BookingPlatformInsiders` |
| `Jwt:Audience` | JWT audience | `BookingPlatformInsiders.Client` |
| `Jwt:AccessTokenMinutes` | Access-token lifetime | `15` |
| `Jwt:RefreshTokenDays` | Refresh-token lifetime | `7` |
| `Jwt:SigningKey` | HS256 signing key (32+ bytes, **secret only**) | — |
| `Auth:RefreshCookieName` | Refresh-token cookie name | `refresh_token` |
| `Auth:RefreshCookiePath` | Cookie path scope | `/api/auth` |
| `Auth:RequireSecureCookies` | Set the `Secure` cookie flag | `false` locally, `true` in production |
| `Auth:RefreshCookieSameSite` | Cookie `SameSite` mode | `Lax` |
| `VITE_API_URL` (client) | API base URL | `http://localhost:5178` |

Secrets are never committed:

- locally: .NET User Secrets for `ConnectionStrings:DefaultConnection` and
  `Jwt:SigningKey`;
- Docker Compose: `.env` (git-ignored, template in `.env.example`);
- environment variables use the `__` separator, e.g.
  `ConnectionStrings__DefaultConnection`, `Jwt__SigningKey`.

## Running with Docker Compose (recommended)

Requirements: Docker Desktop.

```powershell
Copy-Item .env.example .env    # adjust dev credentials if needed
docker compose up -d --build
```

- API: `http://localhost:8080`
- Swagger UI: `http://localhost:8080/swagger`
- PostgreSQL: `localhost:5433` (host port; internal `db:5432`)
- On startup the API applies migrations (`Database__MigrateOnStartup=true`),
  seeds rooms and registers the Hangfire recurring job.
- Hangfire tables live in the `hangfire` schema of the same database.

Run the frontend separately (see below) with `VITE_API_URL=http://localhost:8080`.

Stop the stack (data volume is preserved):

```powershell
docker compose down
```

Note: the Hangfire dashboard (`/hangfire`) uses the default local-only filter;
through Docker port mapping the request originates from the Docker network, so
the dashboard may be unavailable from the host.

## Running locally (without Docker for the API)

1. Start PostgreSQL:

```powershell
docker compose up -d db
```

2. Store the connection string in User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=booking;Username=booking;Password=booking_dev_password" --project BookingPlatformInsiders
```

3. Apply migrations:

```powershell
dotnet ef database update --project BookingPlatformInsiders.Infrastructure --startup-project BookingPlatformInsiders
```

4. Run the API:

```powershell
dotnet run --project BookingPlatformInsiders/BookingPlatformInsiders.Api.csproj
```

API: `http://localhost:5178`, Swagger: `http://localhost:5178/swagger`.
Rooms are seeded automatically on startup (idempotent).

5. Run the React client:

```powershell
cd client
npm install
Copy-Item .env.example .env   # optional if the default URL is correct
npm run dev
```

Client: `http://localhost:5173`. It supports room/date selection, schedule
viewing, creating bookings (UTC time inputs), cancelling bookings and displays
API errors (`code` + validation messages).

## Background job

Hangfire recurring job `bookings-autocomplete` (Infrastructure, registered by a
hosted service; cron and timezone from configuration, UTC) sends
`CompleteExpiredBookingsCommand` via MediatR:

- loads `Scheduled` bookings with `EndUtc <= now` in batches (`BATCH_SIZE`,
  max `MAX_BATCHES` per run);
- calls the domain method `Booking.Complete(now)`;
- saves via `IUnitOfWork`; the operation is idempotent and protected with
  `[DisableConcurrentExecution]`.

## Database and migrations

Migrations live in `BookingPlatformInsiders.Infrastructure/Migrations`:

- `InitialCreate` вЂ” tables, check constraints, indexes, FK;
- `AddRoomNameUniqueIndex` вЂ” case-insensitive unique room name;
- `AddBookingOverlapConstraint` вЂ” `btree_gist` + exclusion constraint.

Add a new migration (startup project is the API, which loads configuration and
User Secrets):

```powershell
dotnet ef migrations add <Name> --project BookingPlatformInsiders.Infrastructure --startup-project BookingPlatformInsiders
```

Never rewrite migrations that may already be applied; never reset or delete
data without explicit approval.
