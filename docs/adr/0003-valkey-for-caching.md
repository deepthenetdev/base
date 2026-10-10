# 3. Valkey and HybridCache for caching

**Status:** Accepted · 2026-10-09

## Context

Repeated reads should not hit the database every time, and every API replica must share one cache.
Redis changed license twice (BSD → RSAL/SSPL → AGPL from Redis 8). Valkey is the Linux Foundation's
BSD-licensed fork, wire-compatible with Redis, and the default in major Linux distributions.

## Decision

- Run **Valkey** as the shared cache (`valkey/valkey` image).
- Use .NET's **HybridCache**: a short-lived in-memory copy per instance, with Valkey as the shared
  second level. It also prevents cache stampedes (many requests rebuilding the same entry at once).
- Writes remove affected cache keys immediately (see `TodoEndpoints`).

## Consequences

- No license review needed for hosting.
- Any Redis client works unchanged, so a managed Redis or Valkey service can replace the container.
- Valkey can later carry light job queues (Streams) and the SignalR backplane before a dedicated
  message broker is needed.
