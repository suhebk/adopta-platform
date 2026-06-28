import { describe, expect, it } from "vitest";
import {
  validateContentBundle,
  validateContentItem,
  type ContentBundle,
  type ContentItem,
  type TargetingPlaceholder
} from "../src";

describe("content contract", () => {
  const targeting: TargetingPlaceholder = {
    mode: "placeholder",
    segments: ["early-adopters"],
    pageKeys: ["billing"]
  };

  const validItem: ContentItem = {
    id: "item-1",
    type: "tooltip",
    version: "1.0.0",
    title: "Submit return",
    body: "Use this action when the return is ready.",
    anchor: {
      strategy: "data-adopt-id",
      value: "billing.submit"
    },
    targeting
  };

  const validBundle: ContentBundle = {
    bundleId: "bundle-1",
    tenantId: "tenant-1",
    applicationId: "app-1",
    environment: "production",
    channel: "published",
    version: "2026.06.28",
    generatedAtUtc: "2026-06-28T12:00:00Z",
    items: [validItem]
  };

  it("accepts a valid content item", () => {
    expect(validateContentItem(validItem)).toEqual({
      ok: true,
      issues: []
    });
  });

  it("returns typed issues for an invalid content item", () => {
    const result = validateContentItem({
      id: "",
      type: "modal",
      version: "",
      title: ""
    });

    expect(result.ok).toBe(false);
    expect(result.issues.map((issue) => issue.code)).toContain("invalid_content_type");
    expect(result.issues.map((issue) => issue.code)).toContain("invalid_content_item");
  });

  it("accepts a valid content bundle", () => {
    expect(validateContentBundle(validBundle)).toEqual({
      ok: true,
      issues: []
    });
  });

  it("returns typed issues for an invalid content bundle", () => {
    const result = validateContentBundle({
      bundleId: "",
      tenantId: "",
      applicationId: "",
      environment: "prod",
      channel: "live",
      version: "",
      generatedAtUtc: "",
      items: "not-items"
    });

    expect(result.ok).toBe(false);
    expect(result.issues.every((issue) => issue.code === "invalid_content_bundle")).toBe(true);
  });

  it("rejects duplicate content item ids", () => {
    const result = validateContentBundle({
      ...validBundle,
      items: [validItem, { ...validItem, type: "callout" }]
    });

    expect(result.ok).toBe(false);
    expect(result.issues).toContainEqual(
      expect.objectContaining({
        code: "duplicate_content_item_id"
      })
    );
  });

  it("rejects invalid anchor descriptors", () => {
    const result = validateContentItem({
      ...validItem,
      anchor: {
        strategy: "css",
        value: ".submit"
      }
    });

    expect(result.ok).toBe(false);
    expect(result.issues).toContainEqual(
      expect.objectContaining({
        code: "invalid_anchor_descriptor"
      })
    );
  });

  it("validates targeting placeholder shape without evaluating targeting", () => {
    expect(validateContentItem({ ...validItem, targeting }).ok).toBe(true);

    const result = validateContentItem({
      ...validItem,
      targeting: {
        mode: "evaluate-now"
      }
    });

    expect(result).toMatchObject({
      ok: false,
      issues: [
        expect.objectContaining({
          code: "invalid_targeting_placeholder"
        })
      ]
    });
  });
});

