# 4. REST by default, other protocols when needed

**Status:** Accepted · 2026-10-09

## Decision

Public APIs are **REST** (ASP.NET Core minimal APIs) with an OpenAPI document at `/openapi/v1.json`.
Other protocols are added only when a requirement calls for them:

| Protocol | Add it when | Implementation |
| --- | --- | --- |
| REST | Default for CRUD and public APIs | Minimal APIs + OpenAPI |
| Server-Sent Events | One-way live updates (notifications, progress) | Built into ASP.NET Core |
| WebSockets | Two-way realtime (chat, collaboration) | SignalR, Valkey backplane |
| GraphQL | Several clients need different shapes of the same data | Hot Chocolate as a BFF |
| gRPC | Service-to-service calls after a module is extracted | Grpc.AspNetCore |

## Consequences

- Simple to learn, debug and cache; every client can call it.
- Errors are always RFC 9457 problem details, so the frontend handles them in one place.
