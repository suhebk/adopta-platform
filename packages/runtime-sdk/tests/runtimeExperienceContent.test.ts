import { describe, expect, it } from "vitest";
import {
  validateContentItem,
  type ChecklistContentItem,
  type ContentItem,
  type RuntimeExperienceMetadata,
  type WalkthroughContentItem
} from "../src";

describe("runtime experience content contracts", () => {
  const metadata: RuntimeExperienceMetadata = {
    placement: {
      preferred: "right",
      fallback: ["bottom", "inline"]
    },
    dismissBehavior: ["dismiss-button", "escape-key"],
    theme: {
      tone: "info",
      density: "comfortable",
      emphasis: "standard"
    }
  };

  it("accepts a valid checklist contract", () => {
    const item: ChecklistContentItem = {
      ...baseItem("checklist"),
      checklist: {
        steps: [
          {
            id: "step-1",
            title: "Confirm details",
            body: "Complete this step when the section is ready.",
            anchor: {
              strategy: "data-adopt-id",
              value: "billing.confirm"
            },
            experience: metadata
          },
          {
            id: "step-2",
            title: "Submit return"
          }
        ]
      }
    };

    expect(validateContentItem(item)).toEqual({
      ok: true,
      issues: []
    });
  });

  it("rejects invalid checklist content safely", () => {
    const item: ChecklistContentItem = {
      ...baseItem("checklist"),
      checklist: {
        steps: [
          {
            id: "step-1",
            title: ""
          },
          {
            id: "step-1",
            title: "Duplicate step",
            body: 123,
            anchor: {
              strategy: "css",
              value: ".billing"
            }
          } as unknown as ChecklistContentItem["checklist"] extends { steps: readonly (infer T)[] } ? T : never
        ]
      }
    };

    const result = validateContentItem(item);

    expect(result.ok).toBe(false);
    expect(result.issues).toEqual(
      expect.arrayContaining([
        expect.objectContaining({ code: "invalid_checklist_content" }),
        expect.objectContaining({ code: "duplicate_checklist_step_id" }),
        expect.objectContaining({ code: "invalid_anchor_descriptor" })
      ])
    );
  });

  it("accepts a valid walkthrough contract", () => {
    const item: WalkthroughContentItem = {
      ...baseItem("walkthrough"),
      walkthrough: {
        steps: [
          {
            id: "intro",
            title: "Start here",
            experience: metadata
          },
          {
            id: "submit",
            title: "Submit",
            body: "Use this step when the return is ready.",
            anchor: {
              strategy: "data-adopt-id",
              value: "billing.submit"
            },
            experience: {
              placement: {
                preferred: "bottom"
              },
              dismissBehavior: ["dismiss-button"]
            }
          }
        ]
      }
    };

    expect(validateContentItem(item)).toEqual({
      ok: true,
      issues: []
    });
  });

  it("rejects invalid walkthrough content safely", () => {
    const item: WalkthroughContentItem = {
      ...baseItem("walkthrough"),
      walkthrough: {
        steps: [
          {
            id: "",
            title: ""
          },
          {
            id: "intro",
            title: "Intro"
          },
          {
            id: "intro",
            title: "Duplicate intro"
          }
        ]
      }
    };

    const result = validateContentItem(item);

    expect(result.ok).toBe(false);
    expect(result.issues).toEqual(
      expect.arrayContaining([
        expect.objectContaining({ code: "invalid_walkthrough_content" }),
        expect.objectContaining({ code: "duplicate_walkthrough_step_id" })
      ])
    );
  });

  it("validates placement values", () => {
    const result = validateContentItem({
      ...baseItem("tooltip"),
      experience: {
        placement: {
          preferred: "screen-x-100",
          fallback: ["bottom"]
        }
      }
    });

    expect(result).toMatchObject({
      ok: false,
      issues: [
        expect.objectContaining({
          code: "invalid_renderer_placement"
        })
      ]
    });
  });

  it("validates dismiss behaviour values", () => {
    const result = validateContentItem({
      ...baseItem("callout"),
      experience: {
        dismissBehavior: ["dismiss-button", "dismiss-button"]
      }
    });

    expect(result).toMatchObject({
      ok: false,
      issues: [
        expect.objectContaining({
          code: "invalid_dismiss_behavior"
        })
      ]
    });
  });

  it("validates theme values", () => {
    const result = validateContentItem({
      ...baseItem("callout"),
      experience: {
        theme: {
          tone: "raw-css-red",
          density: "comfortable"
        }
      }
    });

    expect(result).toMatchObject({
      ok: false,
      issues: [
        expect.objectContaining({
          code: "invalid_renderer_theme"
        })
      ]
    });
  });

  it("validates runtime experience metadata shape", () => {
    const result = validateContentItem({
      ...baseItem("tooltip"),
      experience: "not-metadata"
    });

    expect(result).toMatchObject({
      ok: false,
      issues: [
        expect.objectContaining({
          code: "invalid_runtime_experience_metadata"
        })
      ]
    });
  });

  it("does not echo sensitive marker values in validation messages", () => {
    const marker = ["Bear", "er"].join("");
    const sensitiveValue = `${marker} runtime-marker`;
    const result = validateContentItem({
      ...baseItem("walkthrough"),
      walkthrough: {
        steps: [
          {
            id: "step-1",
            title: "",
            body: 123,
            experience: {
              theme: {
                tone: sensitiveValue
              }
            }
          }
        ]
      }
    });
    const messages = result.issues.map((issue) => issue.message).join(" ");

    expect(result.ok).toBe(false);
    expect(messages).not.toContain(marker);
    expect(messages).not.toContain("runtime-marker");
  });
});

function baseItem(type: ContentItem["type"]): ContentItem {
  return {
    id: `${type}-item`,
    type,
    version: "1.0.0",
    title: "Runtime guidance"
  } as ContentItem;
}
