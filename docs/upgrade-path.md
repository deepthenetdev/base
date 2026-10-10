# Upgrade path

This repo is the **Starter tier**. Each step below is triggered by a real need, not a calendar.

| Tier | Typical scale | Add | Trigger |
| --- | --- | --- | --- |
| Starter (this repo) | Up to ~10k monthly users | Modular monolith, PostgreSQL, Valkey, Docker Compose | — |
| Growth | ~10k to 1M | Worker service + RabbitMQ (or Valkey Streams) with an outbox table; SignalR for realtime; PostgreSQL HA + read replica | Slow work in requests; users need live updates; DB CPU regularly above ~60% |
| Scale | ~1M to 100M | Extract busy modules into services (gRPC inside); Kafka for domain events; GraphQL BFF; Kubernetes (Gateway API, HPA, KEDA); partitioned tables | One module dominates load or deploys; several teams; many consumers of the same events |
| Hyperscale | 100M to billions | Cell-based architecture across regions; sharded data; global CDN and edge caching | A single region or database can't hold the load or the failure blast radius |

## What already prepares you

- **Module boundaries** (`IModule`, one schema per module) are future service boundaries.
- **Stateless API**: all shared state is in PostgreSQL and Valkey, so you can run many replicas behind a load balancer today.
- **Health endpoints** (`/health/live`, `/health/ready`) are what Kubernetes probes expect.
- **OpenTelemetry** is already emitting traces, metrics and logs; point `OTEL_EXPORTER_OTLP_ENDPOINT` at any collector.
- **Containers** are non-root and small, ready for any container platform.

## Next likely steps

- [ ] Add EF Core migrations (see README) once the schema starts changing.
- [ ] Add authentication (OIDC: Keycloak, Entra ID or Auth0) and protect the API.
- [ ] Publish images to a registry from CI and deploy to a managed container platform.
- [ ] Add a background worker and a queue when the first slow job appears.
