import {
  validateRuntimeEvent,
  type RuntimeEvent,
  type RuntimeEventValidationIssue
} from "./RuntimeEvent";

export interface RuntimeEventEnvelope {
  readonly eventId: string;
  readonly tenantId: string;
  readonly applicationId: string;
  readonly sessionId: string;
  readonly occurredAtUtc: string;
  readonly event: RuntimeEvent;
}

export interface RuntimeEventEnvelopeInput {
  readonly eventId: string;
  readonly tenantId: string;
  readonly applicationId: string;
  readonly sessionId: string;
  readonly event: RuntimeEvent;
  readonly occurredAtUtc?: string;
}

export type RuntimeEventEnvelopeValidationIssueCode =
  | "invalid_event_envelope"
  | "invalid_event_timestamp"
  | RuntimeEventValidationIssue["code"];

export interface RuntimeEventEnvelopeValidationIssue {
  readonly code: RuntimeEventEnvelopeValidationIssueCode;
  readonly path: string;
  readonly message: string;
}

export interface RuntimeEventEnvelopeValidationResult {
  readonly ok: boolean;
  readonly issues: readonly RuntimeEventEnvelopeValidationIssue[];
}

export function createRuntimeEventEnvelope(source: RuntimeEventEnvelopeInput): RuntimeEventEnvelope {
  return {
    eventId: source.eventId,
    tenantId: source.tenantId,
    applicationId: source.applicationId,
    sessionId: source.sessionId,
    occurredAtUtc: source.occurredAtUtc ?? new Date().toISOString(),
    event: source.event
  };
}

export function validateRuntimeEventEnvelope(envelope: unknown): RuntimeEventEnvelopeValidationResult {
  const issues: RuntimeEventEnvelopeValidationIssue[] = [];

  if (!isRecord(envelope)) {
    return failed("invalid_event_envelope", "envelope", "Runtime event envelope is invalid.");
  }

  requireNonBlankString(envelope.eventId, "envelope.eventId", issues);
  requireNonBlankString(envelope.tenantId, "envelope.tenantId", issues);
  requireNonBlankString(envelope.applicationId, "envelope.applicationId", issues);
  requireNonBlankString(envelope.sessionId, "envelope.sessionId", issues);

  if (!isUtcIsoTimestamp(envelope.occurredAtUtc)) {
    issues.push(issue("invalid_event_timestamp", "envelope.occurredAtUtc", "Timestamp must be UTC ISO text."));
  }

  const eventResult = validateRuntimeEvent(envelope.event, "envelope.event");
  issues.push(...eventResult.issues);

  return {
    ok: issues.length === 0,
    issues
  };
}

function isUtcIsoTimestamp(value: unknown): boolean {
  if (typeof value !== "string" || value.trim().length === 0 || !value.endsWith("Z")) {
    return false;
  }

  const parsed = Date.parse(value);
  return !Number.isNaN(parsed);
}

function requireNonBlankString(
  value: unknown,
  path: string,
  issues: RuntimeEventEnvelopeValidationIssue[]
): void {
  if (typeof value !== "string" || value.trim().length === 0) {
    issues.push(issue("invalid_event_envelope", path, "Value must be a non-empty string."));
  }
}

function failed(
  code: RuntimeEventEnvelopeValidationIssueCode,
  path: string,
  message: string
): RuntimeEventEnvelopeValidationResult {
  return {
    ok: false,
    issues: [issue(code, path, message)]
  };
}

function issue(
  code: RuntimeEventEnvelopeValidationIssueCode,
  path: string,
  message: string
): RuntimeEventEnvelopeValidationIssue {
  return {
    code,
    path,
    message
  };
}

function isRecord(value: unknown): value is Readonly<Record<string, unknown>> {
  return typeof value === "object" && value !== null;
}
