# 1. Modular monolith first

**Status:** Accepted · 2026-10-09

## Context

Most new apps start with a handful of users and a small team. Microservices from day one add network
calls, distributed failures, separate deployments and data consistency problems before they solve anything.
But some apps will grow to millions of users, and we don't want a rewrite when that happens.

## Decision

Start with one deployable API made of independent **modules** (`src/Modules/*`). Each module:

- implements `IModule` and owns its endpoints, services and data;
- stores its tables in its own PostgreSQL schema through its own `DbContext`;
- never references another module's internals.

## Consequences

- One process to run, debug and deploy; in-process calls are fast and simple.
- Module boundaries match future service boundaries. Extracting a module means moving its folder into
  a new service and replacing in-process calls with gRPC or events; the rest of the app is unchanged.
- Boundaries are enforced by convention and code review today. Add architecture tests
  (for example NetArchTest) when the team grows.
