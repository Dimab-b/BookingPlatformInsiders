# Project Instructions

## Project overview

Meeting Room Booking API — ASP.NET Core (.NET 10) Web API with a React (Vite + TS)
client in `client/`. Repository layout:

- `BookingPlatformInsiders/` — API layer (controllers, middleware, Program.cs, `BookingPlatformInsiders.Api.csproj`)
- `BookingPlatformInsiders.Application` — use cases, DTOs, validators, mapping
- `BookingPlatformInsiders.Domain` — entities, invariants, domain exceptions
- `BookingPlatformInsiders.Infrastructure` — EF Core/PostgreSQL, Dapper, Hangfire
- `BookingPlatformInsiders.Tests` — integration tests (xUnit), created before writing tests

Stack: Clean Architecture, CQRS via MediatR, DDD, FluentValidation, AutoMapper,
EF Core + Npgsql (writes), Dapper (reads), Hangfire, xUnit, WebApplicationFactory.

## General workflow

- Inspect the relevant code before making changes.
- For non-trivial tasks, analyze first and propose a plan.
- Implement only the requested scope; do not refactor unrelated code.
- Explain important changes and list modified files.
- Never claim a build or test passed unless it was run.

## Architecture and dependency direction

- Dependencies point inward only:
  `API -> Application -> Domain`, `Infrastructure -> Application, Domain`,
  `Tests -> API, Infrastructure`.
- Domain references nothing (no EF Core, ASP.NET, MediatR, Npgsql, FluentValidation).
- Application must not reference Infrastructure or the API project.
- Infrastructure must not reference the API project.
- Each layer wires itself via its own `DependencyInjection.Add<Layer>()`; the API
  only composes them.
- Controllers stay thin: no business rules, no persistence, no mapping logic.
- Do not introduce new projects, patterns, or abstractions without a concrete reason.
- Domain events are intentionally not used yet. Do not add event infrastructure
  (`IDomainEvent`, dispatchers, outbox) until a concrete requirement appears.

## Domain (DDD)

- Entities are aggregate roots: private setters, private collection initializers,
  private parameterless constructor for EF, public static factory methods for creation.
- Creation example: `Booking.Create(roomId, title, startUtc, endUtc, now)`.
- All invariants are enforced inside the domain, both in factories and in
  state-changing methods:
  - `EndUtc` must be strictly after `StartUtc`;
  - active bookings of the same room must not overlap:
    `existing.StartUtc < candidate.EndUtc && existing.EndUtc > candidate.StartUtc`;
  - a booking may start exactly when another ends (half-open intervals `[Start, End)`);
  - all timestamps are UTC;
  - cancelled bookings never block a slot.
- State transitions go through domain methods (`Cancel()`, `Complete()`), never
  direct property assignment. Invalid transitions throw explicit domain exceptions.
- Transitions are idempotent where safe: repeated `Cancel()`/`Complete()` does not
  throw and produces no additional change.
- Room existence is checked in Application (cross-aggregate rule); overlap is a
  domain rule evaluated against bookings loaded by the repository.
- Aggregates reference other aggregates by `Id` only, never by object reference.
- Time is never read inside the domain via `DateTime.UtcNow`; the current time is
  passed in as a parameter from Application (`IClock`). Domain logic stays deterministic.
- No EF Core attributes on domain entities; persistence configuration lives in
  Infrastructure (`IEntityTypeConfiguration<T>`).
- Domain exceptions derive from an explicit base (`DomainException`) and are mapped
  to ProblemDetails once, at the API boundary.

## Booking statuses

- Single enum `BookingStatus` in Domain with explicit values:
  `Scheduled = 1, Completed = 2, Cancelled = 3`.
- Lifecycle: `Scheduled -> Cancelled` (user request), `Scheduled -> Completed`
  (system only). `Cancelled` and `Completed` are terminal.
- "Active" for schedules and overlap checks means `Status != Cancelled`
  (both `Scheduled` and `Completed` occupy the room's time interval).
- Past/upcoming is always derived from `StartUtc`/`EndUtc`, never from status.
- Clients may only request `Cancelled` via `PATCH`; requesting `Completed` or
  reactivation returns `409 Conflict`.
- Status is persisted as a string (`HasConversion<string>()`, max length 20),
  never as an implicit ordinal.

## Application and CQRS

- CQRS with MediatR: commands and queries are `record`s; the handler lives next to
  the request (same file or same folder).
- Naming: `XxxCommand`/`XxxCommandHandler`, `XxxQuery`/`XxxQueryHandler`.
- Commands return `Guid`/`bool`/`int` (or a small result); queries return DTOs.
- One write = one aggregate: load via repository, call a domain method, save.
- Repositories and `IUnitOfWork` are interfaces in Application, implemented in
  Infrastructure. Never expose `DbContext` or `IQueryable` outside Infrastructure.
- Expected business failures are returned as an explicit outcome (result/error code:
  404 not found, 409 conflict, 422 invalid state) and mapped to ProblemDetails in
  one place; domain exceptions are reserved for broken invariants.
- Validation:
  - FluentValidation validators cover request shape/format (required fields, lengths,
    dates present, `end > start` at the input level);
  - business invariants stay in Domain and are not duplicated in validators;
  - validators run through a MediatR pipeline behavior; failures return `400`
    ProblemDetails.
- Accept and propagate `CancellationToken` in handlers, repositories, and queries.
- Never use `DateTime.UtcNow` in handlers; inject `IClock` (implemented in
  Infrastructure).

## Mapping (AutoMapper)

- Profiles live in Application (`Mapping/`).
- Map Domain -> DTO only. Never map DTOs/commands -> entities; entities are created
  through domain factories.
- No mapping logic in controllers.

## Persistence (EF Core, PostgreSQL, Dapper)

- Writes: EF Core + Npgsql. `AppDbContext` is `internal` to Infrastructure.
- Reads (room list, schedule): Dapper with parameterized SQL; query interfaces are
  defined in Application, implemented in Infrastructure.
- Overlap check and insert/update run inside one transaction; note the
  check-then-write race in README. Optional hardening: PostgreSQL exclusion
  constraint `EXCLUDE USING gist (room_id WITH =, tstzrange(start_utc, end_utc) WITH &&)
  WHERE (status <> 'Cancelled')` with `btree_gist`.
- Add `xmin` as an optimistic concurrency token (`IsRowVersion`) on `Booking`.
- Use `timestamptz` and `DateTime` with `DateTimeKind.Utc` end to end.
- Schedule query for a date `D`: `StartUtc < D+1day && EndUtc > D` (UTC boundaries),
  `Status != Cancelled`, ordered by `StartUtc`.
- Indexes: `(RoomId, StartUtc, EndUtc)` for overlap and schedule queries.
- Seed at least two rooms; seeding must be idempotent.
- Migrations live in Infrastructure; never rewrite migrations that may be applied.
- Never drop, reset, or delete data without explicit approval.

## Background jobs (Hangfire)

- Hangfire 1.8.x, registered only from Infrastructure via `AddInfrastructure`.
- The recurring job is a reconciliation/self-healing job, not business logic:
  it sends an Application command (e.g. `CompleteExpiredBookingsCommand`).
- Job rules:
  - `[DisableConcurrentExecution]` on the job method;
  - cron and timezone from configuration, UTC (`TimeZoneInfo.Utc`);
  - fixed job id registered by an `IHostedService` registrar
    (`IRecurringJobManager.AddOrUpdate`);
  - process in batches (`Take(N)`), never load the whole table;
  - the job is idempotent and safe to run repeatedly;
  - log only counts and ids, never connection strings or secrets.
- Auto-completion loads aggregates, calls `Complete()`, and saves via the unit of
  work. Do not use `ExecuteUpdate` for aggregate state changes.

## API layer

- Endpoints:
  - `GET /api/rooms`
  - `GET /api/rooms/{roomId}/bookings?date=YYYY-MM-DD`
  - `POST /api/bookings`
  - `PATCH /api/bookings/{id}` — status change only, `Cancelled` only
- Consistent errors via ProblemDetails (400/404/409) with `traceId`.
- Use `[ApiController]`, route `api/...`, async actions with `CancellationToken`.
- Swagger/OpenAPI enabled in Development.
- CORS allows only the Vite dev origin, configured from settings.
- HTTPS redirection and authorization only as configured; do not add endpoints
  beyond the assignment scope.

## React client (`client/`)

- Vite + React + TypeScript; no state-management library unless justified.
- One screen: room and date selection, schedule list, create booking, cancel
  booking, and visible API error messages.
- API base URL comes from `VITE_API_URL`; no hardcoded hosts.
- Keep components small and typed; no business-rule logic in the UI.

## Tests and verification

- xUnit integration tests with `WebApplicationFactory`.
- Database strategy for tests is not decided yet (Testcontainers/PostgreSQL vs
  SQLite in-memory). Do not commit to one until it is agreed.
- Tests exercise the HTTP surface end to end: create returns `201` and appears in
  the schedule; overlapping booking returns `409`; adjacent booking
  (`start == previous end`) is accepted; cancel frees the slot but keeps the record
  with `Cancelled` status, and unknown id returns `404`.
- Inspect existing tests before adding new ones; add tests for meaningful behavior
  changes and important failure cases.
- Report the exact commands executed and their actual results.

## Configuration and secrets

- Locally: User Secrets for connection strings; Docker Compose reads `.env`
  (git-ignored).
- `.env.example` documents required variables and is committed.
- Never commit `.env`, secrets, tokens, or credentials.
- Never log passwords, tokens, or connection strings.

## Package constraints

- MediatR 12.x (13.x requires a license).
- Hangfire 1.8.x.
- EF Core / Npgsql version matched to .NET 10.
- Add packages only for the step being implemented.

## Commands

- Run API: `dotnet run --project BookingPlatformInsiders/BookingPlatformInsiders.Api.csproj`
- Test: `dotnet test BookingPlatformInsiders.Tests`
- Add migration (from repo root):
  `dotnet ef migrations add <Name> --project BookingPlatformInsiders.Infrastructure --startup-project BookingPlatformInsiders`
- `BookingPlatformInsiders.slnx` is stale (references a non-existent csproj and
  misses projects). Run commands against project files, not the solution, unless
  the task is to fix the solution file.

## Git safety

- Inspect `git status` before modifying files; do not overwrite unrelated user changes.
- Do not run destructive Git commands without approval.
- Do not commit or push unless explicitly requested.
- Keep commits scoped to the requested change.
