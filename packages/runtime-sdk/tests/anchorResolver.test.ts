import { describe, expect, it } from "vitest";
import {
  AnchorResolver,
  DataAdoptIdResolver,
  type AnchorDescriptor
} from "../src";

describe("anchor resolution", () => {
  const descriptor: AnchorDescriptor = {
    strategy: "data-adopt-id",
    value: "billing.submit"
  };

  it("resolves exactly one data-adopt-id anchor", () => {
    const root = createRoot('<button data-adopt-id="billing.submit">Submit</button>');
    const result = new DataAdoptIdResolver().resolve(descriptor, { root });

    expect(result.ok).toBe(true);
    if (result.ok) {
      expect(result.element.getAttribute("data-adopt-id")).toBe("billing.submit");
    }
  });

  it("returns missing_anchor when no matching anchor exists", () => {
    const root = createRoot('<button data-adopt-id="billing.cancel">Cancel</button>');
    const result = new DataAdoptIdResolver().resolve(descriptor, { root });

    expect(result).toMatchObject({
      ok: false,
      code: "missing_anchor"
    });
  });

  it("returns duplicate_anchor when more than one matching anchor exists", () => {
    const root = createRoot(
      '<button data-adopt-id="billing.submit">Submit</button><a data-adopt-id="billing.submit">Submit</a>'
    );
    const result = new DataAdoptIdResolver().resolve(descriptor, { root });

    expect(result).toMatchObject({
      ok: false,
      code: "duplicate_anchor"
    });
  });

  it("returns invalid_descriptor for a blank data-adopt-id value", () => {
    const root = createRoot('<button data-adopt-id="billing.submit">Submit</button>');
    const result = new AnchorResolver().resolve(
      {
        strategy: "data-adopt-id",
        value: " "
      },
      { root }
    );

    expect(result).toMatchObject({
      ok: false,
      code: "invalid_descriptor"
    });
  });

  it("returns dom_unavailable when no DOM root is available", () => {
    const originalDocument = globalThis.document;
    Reflect.deleteProperty(globalThis, "document");

    try {
      const result = new DataAdoptIdResolver().resolve(descriptor);

      expect(result).toMatchObject({
        ok: false,
        code: "dom_unavailable"
      });
    } finally {
      Object.defineProperty(globalThis, "document", {
        configurable: true,
        value: originalDocument
      });
    }
  });

  it("returns unsupported_strategy for non-data-adopt-id descriptors", () => {
    const root = createRoot('<button data-adopt-id="billing.submit">Submit</button>');
    const result = new AnchorResolver().resolve(
      {
        strategy: "css",
        value: "[data-adopt-id='billing.submit']"
      },
      { root }
    );

    expect(result).toMatchObject({
      ok: false,
      code: "unsupported_strategy"
    });
  });

  it("routes valid descriptors through the anchor resolver orchestration", () => {
    const root = createRoot('<button data-adopt-id="billing.submit">Submit</button>');
    const result = new AnchorResolver().resolve(descriptor, { root });

    expect(result.ok).toBe(true);
  });

  it("does not mutate the host DOM while resolving anchors", () => {
    const root = createRoot('<button data-adopt-id="billing.submit" class="primary">Submit</button>');
    const before = root.innerHTML;

    new AnchorResolver().resolve(descriptor, { root });

    expect(root.innerHTML).toBe(before);
  });

  it("returns resolver_error when the DOM query fails unexpectedly", () => {
    const brokenRoot = {
      querySelectorAll: () => {
        throw new Error("host DOM failure");
      }
    } as unknown as ParentNode;

    const result = new AnchorResolver().resolve(descriptor, { root: brokenRoot });

    expect(result).toMatchObject({
      ok: false,
      code: "resolver_error"
    });
  });
});

function createRoot(html: string): HTMLElement {
  const elements = Array.from(html.matchAll(/<[^>]+data-adopt-id="([^"]+)"[^>]*>/g)).map(
    (match) =>
      ({
        getAttribute: (name: string): string | null =>
          name === "data-adopt-id" ? match[1] ?? null : null
      }) as Element
  );

  return {
    innerHTML: html,
    querySelectorAll: (selector: string): Element[] =>
      selector === "[data-adopt-id]" ? elements : []
  } as unknown as HTMLElement;
}
