# Base

A starter for web apps that can grow from a weekend project to a high-traffic product without a rewrite:
a **.NET 10** API, a **React + TypeScript** frontend, **PostgreSQL**, **Valkey** caching, **Docker** and CI.

This is the **Starter tier** of the platform design: one API (a modular monolith), one database, one cache.
Every piece is laid out so the next step up (queues, realtime, extracted services, Kafka, Kubernetes)
is an addition, not a rewrite. See [docs/upgrade-path.md](docs/upgrade-path.md).

## What's inside

```
src/
  Base.Api/                 .NET host: errors, OpenAPI, caching, health, rate limiting, telemetry
  Base.BuildingBlocks/      IModule contract shared by all modules
  Modules/Todos/            Sample business module (domain, EF Core data, REST endpoints)
tests/Base.Api.Tests/       Unit tests + integration tests against real PostgreSQL and Valkey
web/                        React 19 + Vite + TanStack Query, Vitest, ESLint
docker-compose.yml          Local stack: postgres, valkey, api, web, telemetry dashboard
.github/workflows/ci.yml    Build, test and image builds on every push and pull request
docs/                       Architecture decisions and the upgrade path
```

## Run everything with Docker

Requires Docker Desktop (or Docker Engine with Compose).

```bash
cp .env.example .env
docker compose up --build
```

| What | URL |
| --- | --- |
| Web app | http://localhost:3000 |
| API | http://localhost:5080/api/todos |
| API docs (Scalar) | http://localhost:5080/scalar |
| Health | http://localhost:5080/health/ready |
| Traces, metrics, logs | http://localhost:18888 |

## Develop locally

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) and Node.js 24.

```bash
# 1. Start the dependencies
docker compose up -d postgres valkey aspire-dashboard

# 2. Run the API (http://localhost:5080)
dotnet run --project src/Base.Api

# 3. Run the web app (http://localhost:5173) in another terminal
cd web
npm install
npm run dev
```

The Vite dev server forwards `/api` to the API, so there is no CORS to configure.

## Test

```bash
docker compose up -d postgres valkey      # integration tests use these
dotnet test Base.slnx

cd web && npm test
```

Run only the fast unit tests with `dotnet test Base.slnx --filter "Category!=Integration"`.

## Add a feature module

1. Copy `src/Modules/Todos` to `src/Modules/<Name>` and rename the project and namespaces.
2. Give its DbContext its own schema (`HasDefaultSchema("<name>")`).
3. Reference it from `src/Base.Api/Base.Api.csproj`, add it to `Base.slnx`, and add `new <Name>Module()` to the `modules` array in `Program.cs`.

Modules must not reference each other. When two modules need to talk, go through a public contract
(an interface or an event), so either one can later become its own service.

## Database migrations

On first run the schema is created from the model. Once the model starts changing, switch to EF Core migrations:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate \
  --project src/Modules/Todos --startup-project src/Base.Api --context TodosDbContext
```

From then on, migrations are applied at startup when `Database:ApplyMigrationsOnStartup` is `true`
(it is in Development and in docker compose). In production, run them as a separate deploy step.

## Configuration

| Setting | Environment variable | Purpose |
| --- | --- | --- |
| `ConnectionStrings:Postgres` | `ConnectionStrings__Postgres` | PostgreSQL connection string |
| `ConnectionStrings:Valkey` | `ConnectionStrings__Valkey` | Valkey (or any Redis-compatible) endpoint |
| `Database:ApplyMigrationsOnStartup` | `Database__ApplyMigrationsOnStartup` | Create or migrate the schema at startup |
| `RateLimiting:PermitsPerMinute` | `RateLimiting__PermitsPerMinute` | Requests per client per minute (default 600) |
| — | `OTEL_EXPORTER_OTLP_ENDPOINT` | Where to send telemetry; unset disables export |

Never commit real secrets. Use `.env` locally (git-ignored) and your platform's secret store in production.

## Decisions

Architecture decisions are recorded in [docs/adr](docs/adr):

1. [Modular monolith first](docs/adr/0001-modular-monolith.md)
2. [PostgreSQL as the default database](docs/adr/0002-postgresql-default.md)
3. [Valkey and HybridCache for caching](docs/adr/0003-valkey-for-caching.md)
4. [REST by default, other protocols when needed](docs/adr/0004-rest-by-default.md)
