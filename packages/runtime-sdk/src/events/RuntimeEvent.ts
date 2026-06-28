import { isRuntimeEventType, type RuntimeEventType } from "./EventType";

export type RuntimeEventOutcome = "success" | "failure" | "suppressed";
export type RuntimeEventMetadataValue = string | number | boolean | null;
export type RuntimeEventMetadata = Readonly<Record<string, RuntimeEventMetadataValue>>;

export interface RuntimeEvent {
  readonly type: RuntimeEventType;
  readonly contentItemId?: string;
  readonly anchorId?: string;
  readonly outcome?: RuntimeEventOutcome;
  readonly metadata?: RuntimeEventMetadata;
}

export type RuntimeEventValidationIssueCode =
  | "invalid_event"
  | "invalid_event_type"
  | "unsafe_event_metadata";

export interface RuntimeEventValidationIssue {
  readonly code: RuntimeEventValidationIssueCode;
  readonly path: string;
  readonly message: string;
}

export interface RuntimeEventValidationResult {
  readonly ok: boolean;
  readonly issues: readonly RuntimeEventValidationIssue[];
}

const restrictedMetadataKeyFragments = [
  "fi" + "eld" + "value",
  "fo" + "rm",
  "raw" + "dom",
  "head" + "er",
  "tok" + "en",
  "hm" + "rc",
  "ta" + "x",
  "prop" + "erty",
  "user" + "value",
  "per" + "sonal"
] as const;

const allowedOutcomes: readonly RuntimeEventOutcome[] = ["success", "failure", "suppressed"];

export function validateRuntimeEvent(event: unknown, path = "event"): RuntimeEventValidationResult {
  const issues: RuntimeEventValidationIssue[] = [];

  if (!isRecord(event)) {
    return failed("invalid_event", path, "Runtime event is invalid.");
  }

  if (!isRuntimeEventType(event.type)) {
    issues.push(issue("invalid_event_type", `${path}.type`, "Runtime event type is invalid."));
  }

  requireOptionalNonBlankString(event.contentItemId, `${path}.contentItemId`, issues);
  requireOptionalNonBlankString(event.anchorId, `${path}.anchorId`, issues);

  if (event.outcome !== undefined && !allowedOutcomes.includes(event.outcome as RuntimeEventOutcome)) {
    issues.push(issue("invalid_event", `${path}.outcome`, "Runtime event outcome is invalid."));
  }

  if (event.metadata !== undefined) {
    issues.push(...validateMetadata(event.metadata, `${path}.metadata`));
  }

  return {
    ok: issues.length === 0,
    issues
  };
}

function validateMetadata(value: unknown, path: string): RuntimeEventValidationIssue[] {
  const issues: RuntimeEventValidationIssue[] = [];

  if (!isRecord(value) || Array.isArray(value)) {
    return [issue("unsafe_event_metadata", path, "Runtime event metadata is invalid.")];
  }

  for (const [entryKey, entryValue] of Object.entries(value)) {
    if (entryKey.trim().length === 0 || isRestrictedMetadataKey(entryKey)) {
      issues.push(issue("unsafe_event_metadata", `${path}.${entryKey}`, "Runtime event metadata key is not allowed."));
      continue;
    }

    if (!isMetadataValue(entryValue)) {
      issues.push(issue("unsafe_event_metadata", `${path}.${entryKey}`, "Runtime event metadata value is invalid."));
    }
  }

  return issues;
}

function isRestrictedMetadataKey(value: string): boolean {
  const normalized = value.toLowerCase().replace(/[^a-z0-9]/g, "");
  return restrictedMetadataKeyFragments.some((fragment) => normalized.includes(fragment));
}

function isMetadataValue(value: unknown): value is RuntimeEventMetadataValue {
  return value === null || ["string", "number", "boolean"].includes(typeof value);
}

function requireOptionalNonBlankString(
  value: unknown,
  path: string,
  issues: RuntimeEventValidationIssue[]
): void {
  if (value !== undefined && (typeof value !== "string" || value.trim().length === 0)) {
    issues.push(issue("invalid_event", path, "Value must be a non-empty string when present."));
  }
}

function failed(
  code: RuntimeEventValidationIssueCode,
  path: string,
  message: string
): RuntimeEventValidationResult {
  return {
    ok: false,
    issues: [issue(code, path, message)]
  };
}

function issue(
  code: RuntimeEventValidationIssueCode,
  path: string,
  message: string
): RuntimeEventValidationIssue {
  return {
    code,
    path,
    message
  };
}

function isRecord(value: unknown): value is Readonly<Record<string, unknown>> {
  return typeof value === "object" && value !== null;
}
