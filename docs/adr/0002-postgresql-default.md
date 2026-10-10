# 2. PostgreSQL as the default database

**Status:** Accepted · 2026-10-09

## Context

Apps need relational integrity for most business data, and sometimes flexible documents.
Running a separate document database doubles operational work and, for a hosting business,
self-hosting MongoDB brings SSPL license obligations.

## Decision

Use **PostgreSQL** for all data by default, through EF Core and the Npgsql provider.
Semi-structured data goes in `jsonb` columns.

## Consequences

- One database to back up, monitor and secure.
- Scales a long way: read replicas and partitioned tables at the Growth tier, then sharding
  (for example Citus) by tenant or user id at the Scale tier.
- Choose a document store only when data is truly schema-less and deeply nested; prefer a managed
  service (MongoDB Atlas) or a Postgres-backed compatible option (FerretDB) over self-hosting MongoDB.
