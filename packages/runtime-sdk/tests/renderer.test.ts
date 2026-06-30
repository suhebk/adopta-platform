import { describe, expect, it } from "vitest";
import {
  Renderer,
  type ContentBundle,
  type ContentItem,
  type WalkthroughStep
} from "../src";

describe("runtime renderer foundation", () => {
  it("renders tooltip content against a valid data-adopt-id anchor", () => {
    const dom = new FakeDocument();
    const anchor = dom.createHostElement("button", "billing.submit");
    dom.body.appendChild(anchor);
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([tooltipItem()]));

    expect(result.ok).toBe(true);
    if (result.ok) {
      expect(result.mount.renderedItemCount).toBe(1);
      expect(result.mount.skippedItemCount).toBe(0);
    }

    const tooltip = findByAttribute(dom.body, "data-adopta-renderer", "tooltip");
    expect(tooltip).toBeDefined();
    expect(tooltip?.getAttribute("role")).toBe("tooltip");
    expect(tooltip?.getAttribute("aria-label")).toBe("Submit return");
  });

  it("renders callout content as a safe announcement banner", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([calloutItem()]));

    expect(result.ok).toBe(true);
    if (result.ok) {
      expect(result.mount.renderedItemCount).toBe(1);
    }

    const banner = findByAttribute(dom.body, "data-adopta-renderer", "banner");
    expect(banner).toBeDefined();
    expect(banner?.getAttribute("role")).toBe("status");
    expect(banner?.getAttribute("aria-label")).toBe("Announcement");
  });

  it("renders checklist title, body, and ordered steps", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richChecklistItem()]));

    expect(result.ok).toBe(true);
    if (result.ok) {
      expect(result.mount.renderedItemCount).toBe(1);
      expect(result.mount.skippedItemCount).toBe(0);
    }

    const checklist = findByAttribute(dom.body, "data-adopta-renderer", "checklist");
    expect(checklist).toBeDefined();
    expect(checklist?.getAttribute("role")).toBe("region");
    expect(checklist?.getAttribute("aria-label")).toBe("Onboarding checklist");
    expect(findByAttribute(dom.body, "data-adopta-renderer", "checklist-title")?.textContent)
      .toBe("Onboarding checklist");
    expect(findByAttribute(dom.body, "data-adopta-renderer", "checklist-body")?.textContent)
      .toBe("Complete these setup tasks.");

    const steps = findAllByAttribute(dom.body, "data-adopta-renderer", "checklist-step");
    expect(steps).toHaveLength(2);
    expect(steps[0]?.getAttribute("role")).toBe("listitem");
    expect(steps[0]?.getAttribute("data-adopta-checklist-state")).toBe("incomplete");
    expect(steps[1]?.getAttribute("data-adopta-checklist-state")).toBe("incomplete");
  });

  it("renders checklist state as display-only without input controls", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richChecklistItem()]));

    expect(result.ok).toBe(true);
    const stateNodes = findAllByAttribute(dom.body, "data-adopta-renderer", "checklist-step-state");
    expect(stateNodes.map((node) => node.textContent)).toEqual(["Incomplete", "Incomplete"]);
    expect(findByTagName(dom.body, "input")).toHaveLength(0);
    expect(findByTagName(dom.body, "form")).toHaveLength(0);
  });

  it("keeps placeholder-safe checklist content valid but skipped", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([checklistItem()]));

    expect(result.ok).toBe(true);
    if (result.ok) {
      expect(result.mount.renderedItemCount).toBe(0);
      expect(result.mount.skippedItemCount).toBe(1);
      expect(result.itemResults).toEqual([
        expect.objectContaining({
          ok: false,
          code: "unsupported_content_type",
          contentType: "checklist"
        })
      ]);
    }
    expect(findRendererNodes(dom.body)).toHaveLength(0);
  });

  it("renders rich walkthrough content from the first step", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richWalkthroughItem()]));

    expect(result.ok).toBe(true);
    if (result.ok) {
      expect(result.mount.renderedItemCount).toBe(1);
      expect(result.mount.skippedItemCount).toBe(0);
    }

    const walkthrough = findByAttribute(dom.body, "data-adopta-renderer", "walkthrough");
    expect(walkthrough).toBeDefined();
    expect(walkthrough?.getAttribute("role")).toBe("region");
    expect(walkthrough?.getAttribute("aria-label")).toBe("Setup walkthrough");
    expect(findByAttribute(dom.body, "data-adopta-renderer", "walkthrough-step-title")?.textContent)
      .toBe("Open setup");
    expect(findByAttribute(dom.body, "data-adopta-renderer", "walkthrough-step-body")?.textContent)
      .toBe("Open the setup area.");
    expect(findByAttribute(dom.body, "data-adopta-renderer", "walkthrough-progress")?.textContent)
      .toBe("Step 1 of 2");
  });

  it("navigates walkthrough steps with correct control states", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richWalkthroughItem()]));

    expect(result.ok).toBe(true);

    const previous = findByAttribute(dom.body, "aria-label", "Previous walkthrough step");
    const next = findByAttribute(dom.body, "aria-label", "Next walkthrough step");
    expect(previous?.getAttribute("aria-disabled")).toBe("true");
    expect(previous?.getAttribute("data-adopta-control-state")).toBe("disabled");
    expect(next?.getAttribute("aria-disabled")).toBe("false");
    expect(next?.getAttribute("data-adopta-control-state")).toBe("enabled");

    next?.dispatch("click", {});

    expect(findByAttribute(dom.body, "data-adopta-renderer", "walkthrough-step-title")?.textContent)
      .toBe("Confirm setup");
    expect(findByAttribute(dom.body, "data-adopta-renderer", "walkthrough-step-body")?.textContent)
      .toBe("");
    expect(findByAttribute(dom.body, "data-adopta-renderer", "walkthrough-step-body")?.getAttribute("aria-hidden"))
      .toBe("true");
    expect(findByAttribute(dom.body, "data-adopta-renderer", "walkthrough-progress")?.textContent)
      .toBe("Step 2 of 2");
    expect(previous?.getAttribute("aria-disabled")).toBe("false");
    expect(next?.getAttribute("aria-disabled")).toBe("true");

    previous?.dispatch("click", {});

    expect(findByAttribute(dom.body, "data-adopta-renderer", "walkthrough-step-title")?.textContent)
      .toBe("Open setup");
    expect(findByAttribute(dom.body, "data-adopta-renderer", "walkthrough-progress")?.textContent)
      .toBe("Step 1 of 2");
    expect(previous?.getAttribute("aria-disabled")).toBe("true");
    expect(next?.getAttribute("aria-disabled")).toBe("false");
  });

  it("fails safely when a tooltip anchor is missing", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([tooltipItem()]));

    expect(result).toMatchObject({
      ok: false,
      code: "missing_anchor"
    });
    expect(findRendererNodes(dom.body)).toHaveLength(0);
  });

  it("fails safely when a tooltip anchor is duplicated", () => {
    const dom = new FakeDocument();
    dom.body.appendChild(dom.createHostElement("button", "billing.submit"));
    dom.body.appendChild(dom.createHostElement("button", "billing.submit"));

    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([tooltipItem()]));

    expect(result).toMatchObject({
      ok: false,
      code: "duplicate_anchor"
    });
    expect(findRendererNodes(dom.body)).toHaveLength(0);
  });

  it("keeps placeholder-safe walkthrough content valid but skipped", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([walkthroughItem()]));

    expect(result.ok).toBe(true);
    if (result.ok) {
      expect(result.mount.renderedItemCount).toBe(0);
      expect(result.mount.skippedItemCount).toBe(1);
      expect(result.itemResults).toEqual([
        expect.objectContaining({
          ok: false,
          code: "unsupported_content_type",
          contentType: "walkthrough"
        })
      ]);
    }
    expect(findRendererNodes(dom.body)).toHaveLength(0);
  });

  it("fails safely when a walkthrough step anchor is missing", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richWalkthroughItem([{
        id: "missing-anchor",
        title: "Missing anchor step",
        anchor: {
          strategy: "data-adopt-id",
          value: "walkthrough.missing"
        }
      }])]));

    expect(result).toMatchObject({
      ok: false,
      code: "missing_anchor"
    });
    expect(findRendererNodes(dom.body)).toHaveLength(0);
  });

  it("fails safely when a walkthrough step anchor is duplicated", () => {
    const dom = new FakeDocument();
    dom.body.appendChild(dom.createHostElement("button", "walkthrough.duplicate"));
    dom.body.appendChild(dom.createHostElement("button", "walkthrough.duplicate"));

    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richWalkthroughItem([{
        id: "duplicate-anchor",
        title: "Duplicate anchor step",
        anchor: {
          strategy: "data-adopt-id",
          value: "walkthrough.duplicate"
        }
      }])]));

    expect(result).toMatchObject({
      ok: false,
      code: "duplicate_anchor"
    });
    expect(findRendererNodes(dom.body)).toHaveLength(0);
  });

  it("fails safely for an invalid bundle", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render({ ...bundle([]), bundleId: "" });

    expect(result).toMatchObject({
      ok: false,
      code: "invalid_bundle"
    });
    expect(findRendererNodes(dom.body)).toHaveLength(0);
  });

  it("renders content as text instead of raw markup", () => {
    const dom = new FakeDocument();
    const anchor = dom.createHostElement("button", "billing.submit");
    dom.body.appendChild(anchor);
    const rawBody = "<strong>Do not parse me</strong>";
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([{ ...tooltipItem(), body: rawBody }]));

    expect(result.ok).toBe(true);
    const renderedBody = findByAttribute(dom.body, "data-adopta-renderer", "tooltip-body");
    expect(renderedBody?.textContent).toBe(rawBody);
    expect(renderedBody?.children).toHaveLength(0);
    expect(dom.innerMarkupWriteCount).toBe(0);
  });

  it("renders checklist content as text instead of raw markup", () => {
    const dom = new FakeDocument();
    const rawBody = "<strong>Do not parse me</strong>";
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([{
        ...richChecklistItem(),
        body: rawBody,
        checklist: {
          steps: [
            {
              id: "step-1",
              title: "First task",
              body: "<img src=x onerror=alert(1)>"
            }
          ]
        }
      }]));

    expect(result.ok).toBe(true);
    const renderedBody = findByAttribute(dom.body, "data-adopta-renderer", "checklist-body");
    const renderedStepBody = findByAttribute(dom.body, "data-adopta-renderer", "checklist-step-body");
    expect(renderedBody?.textContent).toBe(rawBody);
    expect(renderedStepBody?.textContent).toBe("<img src=x onerror=alert(1)>");
    expect(renderedBody?.children).toHaveLength(0);
    expect(renderedStepBody?.children).toHaveLength(0);
    expect(dom.innerMarkupWriteCount).toBe(0);
  });

  it("renders walkthrough content as text instead of raw markup", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richWalkthroughItem([{
        id: "markup-step",
        title: "<strong>Do not parse me</strong>",
        body: "<img src=x onerror=alert(1)>"
      }])]));

    expect(result.ok).toBe(true);
    const renderedTitle = findByAttribute(dom.body, "data-adopta-renderer", "walkthrough-step-title");
    const renderedBody = findByAttribute(dom.body, "data-adopta-renderer", "walkthrough-step-body");
    expect(renderedTitle?.textContent).toBe("<strong>Do not parse me</strong>");
    expect(renderedBody?.textContent).toBe("<img src=x onerror=alert(1)>");
    expect(renderedTitle?.children).toHaveLength(0);
    expect(renderedBody?.children).toHaveLength(0);
    expect(dom.innerMarkupWriteCount).toBe(0);
  });

  it("does not read form values, field values, or host DOM text", () => {
    const dom = new FakeDocument();
    const sensitiveAnchor = dom.createHostElement("input", "billing.submit", true);
    dom.body.appendChild(sensitiveAnchor);

    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([tooltipItem()]));

    expect(result.ok).toBe(true);
    expect(sensitiveAnchor.sensitiveReadCount).toBe(0);
  });

  it("does not read form values, field values, or host DOM text while rendering checklist", () => {
    const dom = new FakeDocument();
    const sensitiveHostElement = dom.createHostElement("input", "billing.secret", true);
    dom.body.appendChild(sensitiveHostElement);

    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richChecklistItem()]));

    expect(result.ok).toBe(true);
    expect(sensitiveHostElement.sensitiveReadCount).toBe(0);
  });

  it("does not read form values, field values, or host DOM text while rendering walkthrough", () => {
    const dom = new FakeDocument();
    const sensitiveHostElement = dom.createHostElement("input", "walkthrough.sensitive", true);
    dom.body.appendChild(sensitiveHostElement);

    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richWalkthroughItem([{
        id: "sensitive-anchor",
        title: "Anchored step",
        anchor: {
          strategy: "data-adopt-id",
          value: "walkthrough.sensitive"
        }
      }])]));

    expect(result.ok).toBe(true);
    expect(sensitiveHostElement.sensitiveReadCount).toBe(0);
  });

  it("dismisses rendered nodes on Escape and removes listener", () => {
    const dom = new FakeDocument();
    dom.body.appendChild(dom.createHostElement("button", "billing.submit"));
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([tooltipItem()]));

    expect(result.ok).toBe(true);
    expect(findRendererNodes(dom.body)).not.toHaveLength(0);
    expect(dom.listenerCount("keydown")).toBe(1);

    dom.dispatch("keydown", { key: "Escape" });

    expect(findRendererNodes(dom.body)).toHaveLength(0);
    expect(dom.listenerCount("keydown")).toBe(0);
  });

  it("dismisses checklist nodes on Escape and removes listener", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richChecklistItem()]));

    expect(result.ok).toBe(true);
    expect(findByAttribute(dom.body, "data-adopta-renderer", "checklist")).toBeDefined();
    expect(dom.listenerCount("keydown")).toBe(1);

    dom.dispatch("keydown", { key: "Escape" });

    expect(findRendererNodes(dom.body)).toHaveLength(0);
    expect(dom.listenerCount("keydown")).toBe(0);
  });

  it("dismisses checklist nodes from the SDK-owned dismiss control", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richChecklistItem()]));

    expect(result.ok).toBe(true);
    expect(findByAttribute(dom.body, "data-adopta-renderer", "checklist")).toBeDefined();

    const dismiss = findByAttribute(dom.body, "aria-label", "Dismiss checklist guidance");
    expect(dismiss).toBeDefined();

    dismiss?.dispatch("click", {});

    expect(findRendererNodes(dom.body)).toHaveLength(0);
    expect(dom.listenerCount("keydown")).toBe(0);
  });

  it("dismisses walkthrough nodes on Escape and removes listener", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richWalkthroughItem()]));

    expect(result.ok).toBe(true);
    expect(findByAttribute(dom.body, "data-adopta-renderer", "walkthrough")).toBeDefined();
    expect(dom.listenerCount("keydown")).toBe(1);

    dom.dispatch("keydown", { key: "Escape" });

    expect(findRendererNodes(dom.body)).toHaveLength(0);
    expect(dom.listenerCount("keydown")).toBe(0);
  });

  it("dismisses walkthrough nodes from the SDK-owned dismiss control", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richWalkthroughItem()]));

    expect(result.ok).toBe(true);
    expect(findByAttribute(dom.body, "data-adopta-renderer", "walkthrough")).toBeDefined();

    const dismiss = findByAttribute(dom.body, "aria-label", "Dismiss walkthrough guidance");
    expect(dismiss).toBeDefined();

    dismiss?.dispatch("click", {});

    expect(findRendererNodes(dom.body)).toHaveLength(0);
    expect(dom.listenerCount("keydown")).toBe(0);
  });

  it("dismisses rendered nodes from the SDK-owned dismiss control", () => {
    const dom = new FakeDocument();
    dom.body.appendChild(dom.createHostElement("button", "billing.submit"));
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([tooltipItem(), calloutItem()]));

    expect(result.ok).toBe(true);
    expect(findRendererNodes(dom.body).length).toBeGreaterThan(0);

    const dismiss = findByAttribute(dom.body, "aria-label", "Dismiss guidance");
    expect(dismiss).toBeDefined();

    dismiss?.dispatch("click", {});

    expect(findRendererNodes(dom.body)).toHaveLength(0);
    expect(dom.listenerCount("keydown")).toBe(0);
  });

  it("unmount removes all created nodes and event listeners", () => {
    const dom = new FakeDocument();
    dom.body.appendChild(dom.createHostElement("button", "billing.submit"));
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([tooltipItem(), calloutItem()]));

    expect(result.ok).toBe(true);
    if (!result.ok) {
      return;
    }

    expect(findRendererNodes(dom.body).length).toBeGreaterThan(0);
    expect(dom.listenerCount("keydown")).toBe(2);

    result.mount.unmount();

    expect(findRendererNodes(dom.body)).toHaveLength(0);
    expect(dom.listenerCount("keydown")).toBe(0);
  });

  it("unmount removes checklist nodes and listeners", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richChecklistItem()]));

    expect(result.ok).toBe(true);
    if (!result.ok) {
      return;
    }

    expect(findRendererNodes(dom.body).length).toBeGreaterThan(0);
    expect(dom.listenerCount("keydown")).toBe(1);

    result.mount.unmount();

    expect(findRendererNodes(dom.body)).toHaveLength(0);
    expect(dom.listenerCount("keydown")).toBe(0);
  });

  it("unmount removes walkthrough nodes and listeners", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([richWalkthroughItem()]));

    expect(result.ok).toBe(true);
    if (!result.ok) {
      return;
    }

    expect(findRendererNodes(dom.body).length).toBeGreaterThan(0);
    expect(dom.listenerCount("keydown")).toBe(1);

    result.mount.unmount();

    expect(findRendererNodes(dom.body)).toHaveLength(0);
    expect(dom.listenerCount("keydown")).toBe(0);
  });

  it("cleans up partial SDK nodes when rendering fails midway", () => {
    const dom = new FakeDocument();
    const result = new Renderer({ document: dom.asDocument(), root: dom.body.asParentNode() })
      .render(bundle([calloutItem(), tooltipItem()]));

    expect(result).toMatchObject({
      ok: false,
      code: "missing_anchor"
    });
    expect(findRendererNodes(dom.body)).toHaveLength(0);
    expect(dom.listenerCount("keydown")).toBe(0);
  });
});

function bundle(items: readonly ContentItem[]): ContentBundle {
  return {
    bundleId: "bundle-1",
    tenantId: "tenant-1",
    applicationId: "application-1",
    environment: "production",
    channel: "published",
    version: "1.0.0",
    generatedAtUtc: "2026-06-29T12:00:00Z",
    items
  };
}

function tooltipItem(): ContentItem {
  return {
    id: "tooltip-1",
    type: "tooltip",
    version: "1.0.0",
    title: "Submit return",
    body: "Use this action when the return is ready.",
    anchor: {
      strategy: "data-adopt-id",
      value: "billing.submit"
    }
  };
}

function calloutItem(): ContentItem {
  return {
    id: "callout-1",
    type: "callout",
    version: "1.0.0",
    title: "Announcement",
    body: "Guidance is available."
  };
}

function checklistItem(): ContentItem {
  return {
    id: "checklist-1",
    type: "checklist",
    version: "1.0.0",
    title: "Checklist placeholder"
  };
}

function richChecklistItem(): ContentItem {
  return {
    id: "checklist-1",
    type: "checklist",
    version: "1.0.0",
    title: "Onboarding checklist",
    body: "Complete these setup tasks.",
    checklist: {
      steps: [
        {
          id: "profile",
          title: "Complete your profile",
          body: "Add the structural profile settings."
        },
        {
          id: "invite",
          title: "Invite a teammate"
        }
      ]
    }
  };
}

function walkthroughItem(): ContentItem {
  return {
    id: "walkthrough-1",
    type: "walkthrough",
    version: "1.0.0",
    title: "Walkthrough placeholder"
  };
}

function richWalkthroughItem(steps: readonly WalkthroughStep[] = [
  {
    id: "open-setup",
    title: "Open setup",
    body: "Open the setup area."
  },
  {
    id: "confirm-setup",
    title: "Confirm setup"
  }
]): ContentItem {
  return {
    id: "walkthrough-1",
    type: "walkthrough",
    version: "1.0.0",
    title: "Setup walkthrough",
    walkthrough: {
      steps
    }
  };
}

function findRendererNodes(root: FakeElement): FakeElement[] {
  return root
    .querySelectorAll("[data-adopta-renderer]")
    .map((element) => element as FakeElement);
}

function findByAttribute(
  root: FakeElement,
  name: string,
  value: string
): FakeElement | undefined {
  return root
    .querySelectorAll(`[${name}]`)
    .find((element) => (element as FakeElement).getAttribute(name) === value) as FakeElement | undefined;
}

function findAllByAttribute(
  root: FakeElement,
  name: string,
  value: string
): FakeElement[] {
  return root
    .querySelectorAll(`[${name}]`)
    .filter((element) => (element as FakeElement).getAttribute(name) === value)
    .map((element) => element as FakeElement);
}

function findByTagName(root: FakeElement, tagName: string): FakeElement[] {
  return root.collectByTagName(tagName);
}

class FakeDocument {
  public readonly documentElement = new FakeElement("html", this);
  public readonly body = new FakeElement("body", this);
  public innerMarkupWriteCount = 0;
  private readonly listeners = new Map<string, Set<EventListener>>();

  public constructor() {
    this.documentElement.appendChild(this.body);
  }

  public createElement(tagName: string): HTMLElement {
    return new FakeElement(tagName, this) as unknown as HTMLElement;
  }

  public createHostElement(
    tagName: string,
    dataAdoptId: string,
    sensitive = false
  ): FakeElement {
    const element = new FakeElement(tagName, this, sensitive);
    element.setAttribute("data-adopt-id", dataAdoptId);

    return element;
  }

  public addEventListener(type: string, listener: EventListener): void {
    const listeners = this.listeners.get(type) ?? new Set<EventListener>();
    listeners.add(listener);
    this.listeners.set(type, listeners);
  }

  public removeEventListener(type: string, listener: EventListener): void {
    this.listeners.get(type)?.delete(listener);
  }

  public dispatch(type: string, event: Partial<KeyboardEvent>): void {
    for (const listener of this.listeners.get(type) ?? []) {
      listener(event as Event);
    }
  }

  public listenerCount(type: string): number {
    return this.listeners.get(type)?.size ?? 0;
  }

  public asDocument(): Document {
    return this as unknown as Document;
  }
}

class FakeElement {
  public readonly children: FakeElement[] = [];
  public parentNode: FakeElement | null = null;
  public sensitiveReadCount = 0;
  private readonly attributes = new Map<string, string>();
  private readonly listeners = new Map<string, Set<EventListener>>();
  private text = "";

  public constructor(
    public readonly tagName: string,
    private readonly owner: FakeDocument,
    private readonly sensitive = false
  ) {}

  public get parentElement(): FakeElement | null {
    return this.parentNode;
  }

  public get textContent(): string {
    if (this.sensitive) {
      this.sensitiveReadCount += 1;
      throw new Error("sensitive text read");
    }

    return this.text;
  }

  public set textContent(value: string | null) {
    this.text = value ?? "";
  }

  public get value(): string {
    if (this.sensitive) {
      this.sensitiveReadCount += 1;
      throw new Error("sensitive value read");
    }

    return "";
  }

  public set innerHTML(_: string) {
    this.owner.innerMarkupWriteCount += 1;
    throw new Error("raw markup write");
  }

  public setAttribute(name: string, value: string): void {
    this.attributes.set(name, value);
  }

  public getAttribute(name: string): string | null {
    return this.attributes.get(name) ?? null;
  }

  public appendChild<T extends Node>(node: T): T {
    const child = node as unknown as FakeElement;
    child.parentNode = this;
    this.children.push(child);

    return node;
  }

  public removeChild<T extends Node>(node: T): T {
    const child = node as unknown as FakeElement;
    const index = this.children.indexOf(child);
    if (index >= 0) {
      this.children.splice(index, 1);
      child.parentNode = null;
    }

    return node;
  }

  public addEventListener(type: string, listener: EventListener): void {
    const listeners = this.listeners.get(type) ?? new Set<EventListener>();
    listeners.add(listener);
    this.listeners.set(type, listeners);
  }

  public removeEventListener(type: string, listener: EventListener): void {
    this.listeners.get(type)?.delete(listener);
  }

  public dispatch(type: string, event: Partial<Event>): void {
    for (const listener of this.listeners.get(type) ?? []) {
      listener(event as Event);
    }
  }

  public querySelectorAll(selector: string): Element[] {
    const attribute = /^\[([^\]]+)\]$/.exec(selector)?.[1];
    if (attribute === undefined) {
      return [];
    }

    return this.collectByAttribute(attribute).map((element) => element as unknown as Element);
  }

  public asParentNode(): ParentNode {
    return this as unknown as ParentNode;
  }

  private collectByAttribute(attribute: string): FakeElement[] {
    const current = this.attributes.has(attribute) ? [this] : [];
    return [
      ...current,
      ...this.children.flatMap((child) => child.collectByAttribute(attribute))
    ];
  }

  public collectByTagName(tagName: string): FakeElement[] {
    const normalizedTagName = tagName.toLowerCase();
    const current = this.tagName.toLowerCase() === normalizedTagName ? [this] : [];
    return [
      ...current,
      ...this.children.flatMap((child) => child.collectByTagName(normalizedTagName))
    ];
  }
}
