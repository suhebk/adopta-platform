import { isContentType } from "./ContentType";
import type { ContentBundle } from "./ContentBundle";
import type { ContentItem } from "./ContentItem";

export type ContentValidationIssueCode =
  | "invalid_content_item"
  | "invalid_content_type"
  | "invalid_content_bundle"
  | "invalid_anchor_descriptor"
  | "invalid_targeting_placeholder"
  | "duplicate_content_item_id";

export interface ContentValidationIssue {
  readonly code: ContentValidationIssueCode;
  readonly path: string;
  readonly message: string;
}

export interface ContentValidationResult {
  readonly ok: boolean;
  readonly issues: readonly ContentValidationIssue[];
}

export function validateContentItem(item: unknown, path = "item"): ContentValidationResult {
  const issues: ContentValidationIssue[] = [];

  if (!isRecord(item)) {
    return failed("invalid_content_item", path, "Content item is invalid.");
  }

  requireNonBlankString(item.id, `${path}.id`, "invalid_content_item", issues);
  requireNonBlankString(item.version, `${path}.version`, "invalid_content_item", issues);
  requireNonBlankString(item.title, `${path}.title`, "invalid_content_item", issues);

  if (!isContentType(item.type)) {
    issues.push(issue("invalid_content_type", `${path}.type`, "Content type is invalid."));
  }

  if (item.body !== undefined && typeof item.body !== "string") {
    issues.push(issue("invalid_content_item", `${path}.body`, "Content body must be a string when present."));
  }

  if (item.anchor !== undefined && !isValidAnchorDescriptor(item.anchor)) {
    issues.push(issue("invalid_anchor_descriptor", `${path}.anchor`, "Anchor descriptor is invalid."));
  }

  if (item.targeting !== undefined && !isValidTargetingPlaceholder(item.targeting)) {
    issues.push(issue("invalid_targeting_placeholder", `${path}.targeting`, "Targeting placeholder is invalid."));
  }

  return {
    ok: issues.length === 0,
    issues
  };
}

export function validateContentBundle(bundle: unknown): ContentValidationResult {
  const issues: ContentValidationIssue[] = [];

  if (!isRecord(bundle)) {
    return failed("invalid_content_bundle", "bundle", "Content bundle is invalid.");
  }

  requireNonBlankString(bundle.bundleId, "bundle.bundleId", "invalid_content_bundle", issues);
  requireNonBlankString(bundle.tenantId, "bundle.tenantId", "invalid_content_bundle", issues);
  requireNonBlankString(bundle.applicationId, "bundle.applicationId", "invalid_content_bundle", issues);
  requireOneOf(bundle.environment, ["development", "test", "production"], "bundle.environment", issues);
  requireOneOf(bundle.channel, ["preview", "published"], "bundle.channel", issues);
  requireNonBlankString(bundle.version, "bundle.version", "invalid_content_bundle", issues);
  requireNonBlankString(bundle.generatedAtUtc, "bundle.generatedAtUtc", "invalid_content_bundle", issues);

  if (!Array.isArray(bundle.items)) {
    issues.push(issue("invalid_content_bundle", "bundle.items", "Content bundle items must be an array."));
  } else {
    const seenIds = new Set<string>();
    bundle.items.forEach((item, index) => {
      const itemResult = validateContentItem(item, `bundle.items[${index}]`);
      issues.push(...itemResult.issues);

      if (isRecord(item) && typeof item.id === "string" && item.id.trim().length > 0) {
        const normalizedId = item.id.trim();
        if (seenIds.has(normalizedId)) {
          issues.push(
            issue("duplicate_content_item_id", `bundle.items[${index}].id`, "Content item id is duplicated.")
          );
        }

        seenIds.add(normalizedId);
      }
    });
  }

  return {
    ok: issues.length === 0,
    issues
  };
}

function isValidAnchorDescriptor(value: unknown): boolean {
  return (
    isRecord(value) &&
    value.strategy === "data-adopt-id" &&
    typeof value.value === "string" &&
    value.value.trim().length > 0
  );
}

function isValidTargetingPlaceholder(value: unknown): boolean {
  if (!isRecord(value) || value.mode !== "placeholder") {
    return false;
  }

  return isOptionalStringArray(value.segments) && isOptionalStringArray(value.pageKeys);
}

function isOptionalStringArray(value: unknown): boolean {
  return (
    value === undefined ||
    (Array.isArray(value) && value.every((item) => typeof item === "string" && item.trim().length > 0))
  );
}

function requireNonBlankString(
  value: unknown,
  path: string,
  code: ContentValidationIssueCode,
  issues: ContentValidationIssue[]
): void {
  if (typeof value !== "string" || value.trim().length === 0) {
    issues.push(issue(code, path, "Value must be a non-empty string."));
  }
}

function requireOneOf(
  value: unknown,
  allowed: readonly string[],
  path: string,
  issues: ContentValidationIssue[]
): void {
  if (typeof value !== "string" || !allowed.includes(value)) {
    issues.push(issue("invalid_content_bundle", path, "Value is outside the allowed contract set."));
  }
}

function failed(
  code: ContentValidationIssueCode,
  path: string,
  message: string
): ContentValidationResult {
  return {
    ok: false,
    issues: [issue(code, path, message)]
  };
}

function issue(
  code: ContentValidationIssueCode,
  path: string,
  message: string
): ContentValidationIssue {
  return {
    code,
    path,
    message
  };
}

function isRecord(value: unknown): value is Readonly<Record<string, unknown>> {
  return typeof value === "object" && value !== null;
}

