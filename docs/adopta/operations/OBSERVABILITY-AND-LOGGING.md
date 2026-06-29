# Observability and Logging

## Purpose

This document defines safe logging and observability conventions for the Adopta operational-readiness foundation.

## Current Implemented Behaviour

- No production observability pipeline is implemented in this slice.
- No analytics pipeline, Event Hubs, ClickHouse, or external telemetry transport is added.
- No live database health checks are implemented in this slice.
- Persistence readiness remains configuration/model based only.

## Safe Logging Principles

- Log structural events, outcomes, and non-sensitive identifiers only.
- Do not log connection string values, passwords, tokens, headers, raw claims, tenant secrets, host credentials, form values, tax data, HMRC data, property data, or sensitive user-entered values.
- Do not log raw content bodies or runtime DOM text.
- Use stable event names and safe failure categories.
- Prefer tenant-aware correlation using internal tenant identifiers only when already available and safe.

## Persistence Observability

Persistence observability must distinguish configuration state from live database state.

Current readiness contracts can report:

- disabled;
- invalid configuration;
- configured but connectivity not checked.

Future live database checks require explicit approval and must be non-leaky. They must not expose server names, database names, credentials, provider error payloads, connection strings, or tenant secrets.

## Audit Observability

Audit and security audit events should capture safe operational metadata:

- action;
- result;
- safe failure category;
- internal tenant ID where available;
- timestamp;
- safe internal actor ID where available.

Audit events must not capture authorization tokens, request headers, raw claims, email addresses, content bodies, user-entered values, or domain-sensitive values.

## Alerting Direction

Future alerting should prioritize:

- repeated configuration validation failures;
- tenant-isolation guardrail failures;
- unexpected authorization failures;
- audit write failures;
- unusual persistence repository errors;
- deployment or rollback validation failures.

Alert payloads must remain non-sensitive and must not include raw exception payloads when those payloads could contain provider or environment details.
