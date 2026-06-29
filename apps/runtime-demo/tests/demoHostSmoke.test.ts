import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import { describe, expect, it } from "vitest";
import {
  AnchorResolver,
  validateContentBundle
} from "@adopta/runtime-sdk";
import { demoContentBundle } from "../src/demoContent";

const demoHtml = readFileSync(resolve(__dirname, "../index.html"), "utf8");
const expectedAnchors = [
  "demo.nav.overview",
  "demo.billing.submit",
  "demo.billing.review",
  "demo.checklist.open",
  "demo.walkthrough.start",
  "demo.guidance.unmount",
  "demo.diagnostics.status"
] as const;

describe("runtime demo host", () => {
  it("contains expected data-adopt-id anchors", () => {
    for (const anchor of expectedAnchors) {
      expect(demoHtml).toContain(`data-adopt-id="${anchor}"`);
    }
  });

  it("validates the local content fixture", () => {
    expect(validateContentBundle(demoContentBundle)).toEqual({
      ok: true,
      issues: []
    });
  });

  it("resolves demo content anchors against a lightweight DOM root", () => {
    const root = createRoot(demoHtml);
    const resolver = new AnchorResolver();

    for (const item of demoContentBundle.items) {
      expect(item.anchor).toBeDefined();
      const result = resolver.resolve(item.anchor!, { root });
      expect(result.ok).toBe(true);
    }
  });

  it("returns a safe failure for a missing demo anchor", () => {
    const result = new AnchorResolver().resolve(
      {
        strategy: "data-adopt-id",
        value: "demo.missing"
      },
      { root: createRoot(demoHtml) }
    );

    expect(result).toMatchObject({
      ok: false,
      code: "missing_anchor"
    });
  });

  it("does not introduce value collection controls", () => {
    const disallowedControlPattern = new RegExp(
      `<(${["in" + "put", "text" + "area", "fo" + "rm"].join("|")})\\b`,
      "i"
    );

    expect(demoHtml).not.toMatch(disallowedControlPattern);
  });
});

function createRoot(html: string): ParentNode {
  const elements = Array.from(html.matchAll(/<[^>]+data-adopt-id="([^"]+)"[^>]*>/g)).map(
    (match) =>
      ({
        getAttribute: (name: string): string | null =>
          name === "data-adopt-id" ? match[1] ?? null : null
      }) as Element
  );

  return {
    querySelectorAll: (selector: string): Element[] =>
      selector === "[data-adopt-id]" ? elements : []
  } as unknown as ParentNode;
}
