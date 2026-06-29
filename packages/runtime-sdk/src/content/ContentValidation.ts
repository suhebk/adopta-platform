import { isContentType } from "./ContentType";
import type { ContentBundle } from "./ContentBundle";
import type { ContentItem } from "./ContentItem";
import { isRendererPlacementToken } from "./RendererPlacement";
import { isDismissBehaviorToken } from "./RuntimeExperienceContent";
import {
  isRendererThemeDensity,
  isRendererThemeEmphasis,
  isRendererThemeTone
} from "./RendererTheme";

export type ContentValidationIssueCode =
  | "invalid_content_item"
  | "invalid_content_type"
  | "invalid_content_bundle"
  | "invalid_anchor_descriptor"
  | "invalid_targeting_placeholder"
  | "invalid_runtime_experience_metadata"
  | "invalid_checklist_content"
  | "duplicate_checklist_step_id"
  | "invalid_walkthrough_content"
  | "duplicate_walkthrough_step_id"
  | "invalid_renderer_placement"
  | "invalid_dismiss_behavior"
  | "invalid_renderer_theme"
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

  if (item.experience !== undefined) {
    issues.push(...validateRuntimeExperienceMetadata(item.experience, `${path}.experience`));
  }

  if (item.type === "checklist" && item.checklist !== undefined) {
    issues.push(...validateChecklistContent(item.checklist, `${path}.checklist`));
  }

  if (item.type === "walkthrough" && item.walkthrough !== undefined) {
    issues.push(...validateWalkthroughContent(item.walkthrough, `${path}.walkthrough`));
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

function validateChecklistContent(value: unknown, path: string): ContentValidationIssue[] {
  const issues: ContentValidationIssue[] = [];

  if (!isRecord(value) || !Array.isArray(value.steps)) {
    return [issue("invalid_checklist_content", path, "Checklist content is invalid.")];
  }

  validateSteps(
    value.steps,
    path,
    "invalid_checklist_content",
    "duplicate_checklist_step_id",
    issues
  );

  return issues;
}

function validateWalkthroughContent(value: unknown, path: string): ContentValidationIssue[] {
  const issues: ContentValidationIssue[] = [];

  if (!isRecord(value) || !Array.isArray(value.steps)) {
    return [issue("invalid_walkthrough_content", path, "Walkthrough content is invalid.")];
  }

  validateSteps(
    value.steps,
    path,
    "invalid_walkthrough_content",
    "duplicate_walkthrough_step_id",
    issues
  );

  return issues;
}

function validateSteps(
  steps: readonly unknown[],
  path: string,
  invalidCode: Extract<ContentValidationIssueCode, "invalid_checklist_content" | "invalid_walkthrough_content">,
  duplicateCode: Extract<ContentValidationIssueCode, "duplicate_checklist_step_id" | "duplicate_walkthrough_step_id">,
  issues: ContentValidationIssue[]
): void {
  if (steps.length === 0) {
    issues.push(issue(invalidCode, `${path}.steps`, "Content steps are required."));
    return;
  }

  const seenIds = new Set<string>();
  steps.forEach((step, index) => {
    const stepPath = `${path}.steps[${index}]`;
    if (!isRecord(step)) {
      issues.push(issue(invalidCode, stepPath, "Content step is invalid."));
      return;
    }

    requireNonBlankString(step.id, `${stepPath}.id`, invalidCode, issues);
    requireNonBlankString(step.title, `${stepPath}.title`, invalidCode, issues);

    if (step.body !== undefined && typeof step.body !== "string") {
      issues.push(issue(invalidCode, `${stepPath}.body`, "Content step body must be a string when present."));
    }

    if (step.anchor !== undefined && !isValidAnchorDescriptor(step.anchor)) {
      issues.push(issue("invalid_anchor_descriptor", `${stepPath}.anchor`, "Anchor descriptor is invalid."));
    }

    if (step.experience !== undefined) {
      issues.push(...validateRuntimeExperienceMetadata(step.experience, `${stepPath}.experience`));
    }

    if (typeof step.id === "string" && step.id.trim().length > 0) {
      const normalizedId = step.id.trim();
      if (seenIds.has(normalizedId)) {
        issues.push(issue(duplicateCode, `${stepPath}.id`, "Content step id is duplicated."));
      }

      seenIds.add(normalizedId);
    }
  });
}

function validateRuntimeExperienceMetadata(
  value: unknown,
  path: string
): ContentValidationIssue[] {
  const issues: ContentValidationIssue[] = [];

  if (!isRecord(value)) {
    return [
      issue(
        "invalid_runtime_experience_metadata",
        path,
        "Runtime experience metadata is invalid."
      )
    ];
  }

  if (value.placement !== undefined) {
    issues.push(...validateRendererPlacement(value.placement, `${path}.placement`));
  }

  if (value.dismissBehavior !== undefined) {
    issues.push(...validateDismissBehavior(value.dismissBehavior, `${path}.dismissBehavior`));
  }

  if (value.theme !== undefined) {
    issues.push(...validateRendererTheme(value.theme, `${path}.theme`));
  }

  return issues;
}

function validateRendererPlacement(value: unknown, path: string): ContentValidationIssue[] {
  if (!isRecord(value) || !isRendererPlacementToken(value.preferred)) {
    return [issue("invalid_renderer_placement", path, "Renderer placement is invalid.")];
  }

  if (
    value.fallback !== undefined &&
    (!Array.isArray(value.fallback) || !value.fallback.every(isRendererPlacementToken))
  ) {
    return [issue("invalid_renderer_placement", `${path}.fallback`, "Renderer placement fallback is invalid.")];
  }

  return [];
}

function validateDismissBehavior(value: unknown, path: string): ContentValidationIssue[] {
  if (!Array.isArray(value) || value.length === 0 || !value.every(isDismissBehaviorToken)) {
    return [issue("invalid_dismiss_behavior", path, "Dismiss behavior is invalid.")];
  }

  const seen = new Set<string>();
  for (const behavior of value) {
    if (seen.has(behavior)) {
      return [issue("invalid_dismiss_behavior", path, "Dismiss behavior is invalid.")];
    }

    seen.add(behavior);
  }

  return [];
}

function validateRendererTheme(value: unknown, path: string): ContentValidationIssue[] {
  if (!isRecord(value)) {
    return [issue("invalid_renderer_theme", path, "Renderer theme is invalid.")];
  }

  const issues: ContentValidationIssue[] = [];

  if (value.tone !== undefined && !isRendererThemeTone(value.tone)) {
    issues.push(issue("invalid_renderer_theme", `${path}.tone`, "Renderer theme is invalid."));
  }

  if (value.density !== undefined && !isRendererThemeDensity(value.density)) {
    issues.push(issue("invalid_renderer_theme", `${path}.density`, "Renderer theme is invalid."));
  }

  if (value.emphasis !== undefined && !isRendererThemeEmphasis(value.emphasis)) {
    issues.push(issue("invalid_renderer_theme", `${path}.emphasis`, "Renderer theme is invalid."));
  }

  return issues;
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

