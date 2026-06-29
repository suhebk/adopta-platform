import { describe, expect, it } from "vitest";
import type {
  DeliveryTransport,
  DeliveryTransportRequest
} from "@adopta/runtime-sdk";
import {
  createDemoDeliveryTransport,
  runRuntimeDemo
} from "../src/main";
import { demoContentBundle } from "../src/demoContent";

describe("runtime demo end-to-end wiring", () => {
  it("loads the fixture through mock delivery transport and renders supported guidance", async () => {
    const dom = createDemoDocument();
    const requests: DeliveryTransportRequest[] = [];
    const transport: DeliveryTransport = async (request) => {
      requests.push(request);
      return createDemoDeliveryTransport()(request);
    };

    const result = await runRuntimeDemo({
      document: dom.asDocument(),
      apiBaseUrl: "/demo-runtime",
      transport
    });

    expect(result.ok).toBe(true);
    expect(requests).toHaveLength(1);
    expect(requests[0]?.url).toBe(
      "/demo-runtime/runtime/delivery/bundles/runtime-demo-application?environment=development&channel=preview"
    );
    expect(findByAttribute(dom.body, "data-adopta-renderer", "tooltip")).toBeDefined();
    expect(findByAttribute(dom.body, "data-adopta-renderer", "banner")).toBeDefined();
    expect(readStatusMessages(dom)).toEqual([
      "Runtime initialised: yes",
      "Delivery bundle loaded: yes",
      "Guidance rendered: 2",
      "Unsupported placeholders skipped: 2"
    ]);
  });

  it("supports explicit unmount from the demo control", async () => {
    const dom = createDemoDocument();
    const result = await runRuntimeDemo({
      document: dom.asDocument(),
      transport: createDemoDeliveryTransport()
    });

    expect(result.ok).toBe(true);
    expect(findRendererNodes(dom.body).length).toBeGreaterThan(0);

    const unmount = dom.getElementById("adopta-demo-unmount") as FakeElement | null;
    expect(unmount).not.toBeNull();
    unmount?.dispatch("click", {});

    expect(findRendererNodes(dom.body)).toHaveLength(0);
    expect(readStatusMessages(dom).at(-1)).toBe("Rendered guidance unmounted: yes");
  });

  it("fails safely and cleans up when a required anchor is missing", async () => {
    const dom = createDemoDocument({ omitAnchor: "demo.billing.submit" });
    const result = await runRuntimeDemo({
      document: dom.asDocument(),
      transport: createDemoDeliveryTransport()
    });

    expect(result).toMatchObject({
      ok: false,
      errorCode: "missing_anchor"
    });
    expect(findRendererNodes(dom.body)).toHaveLength(0);
    expect(readStatusMessages(dom)).toContain("Guidance rendered: no (missing_anchor)");
  });

  it("fails safely and cleans up when an anchor is duplicated", async () => {
    const dom = createDemoDocument({ duplicateAnchor: "demo.billing.submit" });
    const result = await runRuntimeDemo({
      document: dom.asDocument(),
      transport: createDemoDeliveryTransport()
    });

    expect(result).toMatchObject({
      ok: false,
      errorCode: "duplicate_anchor"
    });
    expect(findRendererNodes(dom.body)).toHaveLength(0);
    expect(readStatusMessages(dom)).toContain("Guidance rendered: no (duplicate_anchor)");
  });

  it("does not read host text or field values while rendering", async () => {
    const dom = createDemoDocument({ sensitiveAnchor: "demo.billing.submit" });
    const result = await runRuntimeDemo({
      document: dom.asDocument(),
      transport: createDemoDeliveryTransport()
    });

    expect(result.ok).toBe(true);
    expect(dom.sensitiveReadCount).toBe(0);
  });
});

interface DemoDocumentOptions {
  readonly omitAnchor?: string;
  readonly duplicateAnchor?: string;
  readonly sensitiveAnchor?: string;
}

function createDemoDocument(options: DemoDocumentOptions = {}): FakeDocument {
  const dom = new FakeDocument();
  const anchors = [
    "demo.nav.overview",
    "demo.billing.submit",
    "demo.billing.review",
    "demo.checklist.open",
    "demo.walkthrough.start"
  ];

  for (const anchor of anchors) {
    if (anchor === options.omitAnchor) {
      continue;
    }

    const tagName = anchor === "demo.billing.submit" ? "button" : "section";
    dom.body.appendChild(
      dom.createHostElement(tagName, anchor, anchor === options.sensitiveAnchor)
    );
  }

  if (options.duplicateAnchor !== undefined) {
    dom.body.appendChild(dom.createHostElement("button", options.duplicateAnchor));
  }

  const unmount = dom.createHostElement("button", "demo.guidance.unmount");
  unmount.setAttribute("id", "adopta-demo-unmount");
  dom.body.appendChild(unmount);

  const status = dom.createHostElement("ul", "demo.diagnostics.status");
  status.setAttribute("id", "adopta-demo-status");
  dom.body.appendChild(status);

  expect(demoContentBundle.items).toHaveLength(4);

  return dom;
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

function readStatusMessages(dom: FakeDocument): string[] {
  const status = dom.getElementById("adopta-demo-status") as FakeElement | null;
  return status?.children.map((child) => child.textContent) ?? [];
}

class FakeDocument {
  public readonly documentElement = new FakeElement("html", this);
  public readonly body = new FakeElement("body", this);
  private readonly listeners = new Map<string, Set<EventListener>>();
  public sensitiveReadCount = 0;

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

  public querySelectorAll(selector: string): Element[] {
    return this.documentElement.querySelectorAll(selector);
  }

  public getElementById(id: string): HTMLElement | null {
    return this.documentElement.findById(id) as unknown as HTMLElement ?? null;
  }

  public addEventListener(type: string, listener: EventListener): void {
    const listeners = this.listeners.get(type) ?? new Set<EventListener>();
    listeners.add(listener);
    this.listeners.set(type, listeners);
  }

  public removeEventListener(type: string, listener: EventListener): void {
    this.listeners.get(type)?.delete(listener);
  }

  public asDocument(): Document {
    return this as unknown as Document;
  }
}

class FakeElement {
  public readonly children: FakeElement[] = [];
  public parentNode: FakeElement | null = null;
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
      this.owner.sensitiveReadCount += 1;
      throw new Error("sensitive text read");
    }

    return this.text;
  }

  public set textContent(value: string | null) {
    this.text = value ?? "";
  }

  public get value(): string {
    if (this.sensitive) {
      this.owner.sensitiveReadCount += 1;
      throw new Error("sensitive value read");
    }

    return "";
  }

  public set innerHTML(_: string) {
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

  public replaceChildren(...nodes: Node[]): void {
    for (const child of this.children) {
      child.parentNode = null;
    }

    this.children.length = 0;
    for (const node of nodes) {
      this.appendChild(node);
    }
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

  public findById(id: string): FakeElement | undefined {
    if (this.getAttribute("id") === id) {
      return this;
    }

    for (const child of this.children) {
      const match = child.findById(id);
      if (match !== undefined) {
        return match;
      }
    }

    return undefined;
  }

  private collectByAttribute(attribute: string): FakeElement[] {
    const current = this.attributes.has(attribute) ? [this] : [];
    return [
      ...current,
      ...this.children.flatMap((child) => child.collectByAttribute(attribute))
    ];
  }
}
