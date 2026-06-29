import {
  DeliveryClient,
  initializeAdoptaRuntime,
  Renderer,
  type DeliveryTransport,
  type RendererMount
} from "@adopta/runtime-sdk";
import { demoContentBundle } from "./demoContent";

const DEMO_API_BASE_URL = "/adopta-demo-api";

export interface RuntimeDemoOptions {
  readonly document?: Document;
  readonly transport?: DeliveryTransport;
  readonly apiBaseUrl?: string;
}

export type RuntimeDemoRunResult =
  | RuntimeDemoRunSuccess
  | RuntimeDemoRunFailure;

export interface RuntimeDemoRunSuccess {
  readonly ok: true;
  readonly messages: readonly string[];
  readonly mount: RendererMount;
  readonly renderedItemCount: number;
  readonly skippedItemCount: number;
}

export interface RuntimeDemoRunFailure {
  readonly ok: false;
  readonly messages: readonly string[];
  readonly errorCode: string;
}

let currentMount: RendererMount | undefined;

export function createDemoDeliveryTransport(): DeliveryTransport {
  return async () => ({
    status: 200,
    json: async (): Promise<unknown> => ({
      succeeded: true,
      status: "found",
      bundle: {
        tenantId: demoContentBundle.tenantId,
        applicationId: demoContentBundle.applicationId,
        environment: demoContentBundle.environment,
        channel: demoContentBundle.channel,
        version: demoContentBundle.version,
        content: demoContentBundle
      },
      issues: []
    })
  });
}

export async function runRuntimeDemo(
  options: RuntimeDemoOptions = {}
): Promise<RuntimeDemoRunResult> {
  const domDocument = options.document ?? resolveGlobalDocument();
  if (domDocument === undefined) {
    return {
      ok: false,
      messages: ["Runtime demo unavailable: DOM is unavailable."],
      errorCode: "dom_unavailable"
    };
  }

  currentMount?.unmount();
  currentMount = undefined;

  const statusList = domDocument.getElementById("adopta-demo-status");
  const messages: string[] = [];
  const runtimeResult = initializeAdoptaRuntime({
    tenantId: demoContentBundle.tenantId,
    applicationId: demoContentBundle.applicationId,
    environment: demoContentBundle.environment,
    channel: demoContentBundle.channel,
    noOp: true
  });

  messages.push(`Runtime initialised: ${runtimeResult.ok ? "yes" : "no"}`);
  if (!runtimeResult.ok) {
    updateStatus(domDocument, statusList, messages);

    return {
      ok: false,
      messages,
      errorCode: runtimeResult.error.code
    };
  }

  const deliveryClient = new DeliveryClient({
    apiBaseUrl: options.apiBaseUrl ?? DEMO_API_BASE_URL,
    expectedTenantId: demoContentBundle.tenantId,
    transport: options.transport ?? createDemoDeliveryTransport()
  });
  const deliveryResult = await deliveryClient.getBundle({
    applicationId: demoContentBundle.applicationId,
    environment: demoContentBundle.environment,
    channel: demoContentBundle.channel
  });

  if (!deliveryResult.ok) {
    messages.push(`Delivery bundle loaded: no (${deliveryResult.error.code})`);
    updateStatus(domDocument, statusList, messages);

    return {
      ok: false,
      messages,
      errorCode: deliveryResult.error.code
    };
  }

  messages.push("Delivery bundle loaded: yes");

  const renderer = new Renderer({
    document: domDocument,
    root: domDocument
  });
  const renderResult = renderer.render(deliveryResult.bundle);

  if (!renderResult.ok) {
    messages.push(`Guidance rendered: no (${renderResult.code})`);
    updateStatus(domDocument, statusList, messages);

    return {
      ok: false,
      messages,
      errorCode: renderResult.code
    };
  }

  currentMount = renderResult.mount;
  messages.push(`Guidance rendered: ${renderResult.mount.renderedItemCount}`);
  messages.push(`Unsupported placeholders skipped: ${renderResult.mount.skippedItemCount}`);
  bindUnmountControl(domDocument, statusList, messages);
  updateStatus(domDocument, statusList, messages);

  return {
    ok: true,
    messages,
    mount: renderResult.mount,
    renderedItemCount: renderResult.mount.renderedItemCount,
    skippedItemCount: renderResult.mount.skippedItemCount
  };
}

function bindUnmountControl(
  domDocument: Document,
  statusList: Element | null,
  messages: readonly string[]
): void {
  const unmountControl = domDocument.getElementById("adopta-demo-unmount");
  unmountControl?.addEventListener("click", () => {
    currentMount?.unmount();
    currentMount = undefined;
    updateStatus(domDocument, statusList, [
      ...messages,
      "Rendered guidance unmounted: yes"
    ]);
  });
}

function updateStatus(
  domDocument: Document,
  statusList: Element | null,
  messages: readonly string[]
): void {
  if (statusList === null) {
    return;
  }

  statusList.replaceChildren(
    ...messages.map((message) => {
      const item = domDocument.createElement("li");
      item.textContent = message;
      return item;
    })
  );
}

function resolveGlobalDocument(): Document | undefined {
  return typeof document === "undefined" ? undefined : document;
}

const autoDocument = resolveGlobalDocument();
if (autoDocument !== undefined) {
  void runRuntimeDemo({ document: autoDocument });
}
