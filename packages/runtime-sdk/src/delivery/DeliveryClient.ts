import type { ContentBundle } from "../content/ContentBundle";
import { validateContentBundle } from "../content/ContentValidation";
import { createNoopRuntimeLogger, type RuntimeLogger } from "../runtime/RuntimeLogger";
import type { RuntimeChannel, RuntimeEnvironment } from "../runtime/RuntimeOptions";
import type { DeliveryClientOptions } from "./DeliveryClientOptions";
import type { DeliveryRequest } from "./DeliveryRequest";
import {
  createDeliveryFailureResult,
  type DeliveryFailureCode,
  type DeliveryFailureResult,
  type DeliveryResult
} from "./DeliveryResult";
import {
  createFetchDeliveryTransport,
  type DeliveryTransport,
  type DeliveryTransportResponse
} from "./DeliveryTransport";

const DEFAULT_TIMEOUT_MS = 10_000;

export class DeliveryClient {
  private readonly logger: RuntimeLogger;
  private readonly transport: DeliveryTransport;

  public constructor(private readonly options: DeliveryClientOptions) {
    this.logger = options.logger ?? createNoopRuntimeLogger();
    this.transport = options.transport ?? createFetchDeliveryTransport();
  }

  public async getBundle(request: DeliveryRequest): Promise<DeliveryResult> {
    const invalidRequest = validateDeliveryRequest(this.options, request);
    if (invalidRequest !== undefined) {
      this.logger.warn("Adopta delivery request rejected.", {
        code: invalidRequest.error.code
      });

      return invalidRequest;
    }

    const boundary = createSignalBoundary(request.signal, this.options.timeoutMs ?? DEFAULT_TIMEOUT_MS);

    try {
      const transportRequest = boundary.signal === undefined
        ? {
            url: buildDeliveryUrl(this.options.apiBaseUrl, request),
            method: "GET" as const
          }
        : {
            url: buildDeliveryUrl(this.options.apiBaseUrl, request),
            method: "GET" as const,
            signal: boundary.signal
          };

      const response = await this.transport(transportRequest);

      return await mapResponse(response, this.options.expectedTenantId, this.logger);
    } catch {
      const failureCode: DeliveryFailureCode = boundary.signal?.aborted === true
        ? "timeout"
        : "network_failure";

      this.logger.warn("Adopta delivery request failed safely.", {
        code: failureCode
      });

      return failure(
        failureCode,
        failureCode === "timeout"
          ? "Delivery request timed out or was cancelled."
          : "Delivery request failed safely."
      );
    } finally {
      boundary.dispose();
    }
  }
}

function validateDeliveryRequest(
  options: DeliveryClientOptions,
  request: DeliveryRequest
): DeliveryFailureResult | undefined {
  if (isBlank(options.apiBaseUrl)) {
    return failure("invalid_request", "Delivery client configuration is invalid.");
  }

  if (options.expectedTenantId !== undefined && isBlank(options.expectedTenantId)) {
    return failure("invalid_request", "Delivery client configuration is invalid.");
  }

  if (
    options.timeoutMs !== undefined &&
    (!Number.isFinite(options.timeoutMs) || options.timeoutMs <= 0)
  ) {
    return failure("invalid_request", "Delivery client configuration is invalid.");
  }

  if (isBlank(request.applicationId)) {
    return failure("invalid_request", "Delivery request is invalid.");
  }

  if (!isRuntimeEnvironment(request.environment)) {
    return failure("invalid_request", "Delivery request is invalid.");
  }

  if (!isRuntimeChannel(request.channel)) {
    return failure("invalid_request", "Delivery request is invalid.");
  }

  return undefined;
}

async function mapResponse(
  response: DeliveryTransportResponse,
  expectedTenantId: string | undefined,
  logger: RuntimeLogger
): Promise<DeliveryResult> {
  if (response.status === 401) {
    return failure("unauthorized", "Delivery request was not authorized.", response.status);
  }

  if (response.status === 403) {
    return failure("forbidden", "Delivery request was forbidden.", response.status);
  }

  if (response.status === 404) {
    return failure("not_found", "Delivery bundle was not found.", response.status);
  }

  if (response.status < 200 || response.status >= 300) {
    return failure("unexpected_response", "Delivery response was unexpected.", response.status);
  }

  let payload: unknown;
  try {
    payload = await response.json();
  } catch {
    return failure("unexpected_response", "Delivery response was unexpected.", response.status);
  }

  const bundle = normalizeDeliveryBundle(payload);
  if (bundle === undefined) {
    return failure("unexpected_response", "Delivery response was unexpected.", response.status);
  }

  if (
    expectedTenantId !== undefined &&
    bundle.tenantId !== expectedTenantId.trim()
  ) {
    return failure("bundle_validation_failed", "Delivery bundle validation failed.", response.status);
  }

  const validation = validateContentBundle(bundle);
  if (!validation.ok) {
    logger.warn("Adopta delivery bundle validation failed.", {
      code: "bundle_validation_failed",
      issueCount: validation.issues.length
    });

    return failure(
      "bundle_validation_failed",
      "Delivery bundle validation failed.",
      response.status,
      validation.issues
    );
  }

  return {
    ok: true,
    bundle,
    statusCode: response.status
  };
}

function normalizeDeliveryBundle(payload: unknown): ContentBundle | undefined {
  if (!isRecord(payload) || payload.succeeded !== true || !isRecord(payload.bundle)) {
    return undefined;
  }

  const content = payload.bundle.content;
  if (!isRecord(content)) {
    return undefined;
  }

  const environment = normalizeEnvironment(content.environment);
  const channel = normalizeChannel(content.channel);
  const items = Array.isArray(content.items)
    ? content.items.map(normalizeContentItem)
    : content.items;

  return {
    ...content,
    environment,
    channel,
    items
  } as ContentBundle;
}

function normalizeContentItem(value: unknown): unknown {
  if (!isRecord(value)) {
    return value;
  }

  const normalizedType = normalizeContentType(value.type);
  const normalizedAnchor = normalizeAnchor(value.anchor);

  return {
    ...value,
    type: normalizedType,
    anchor: normalizedAnchor
  };
}

function normalizeAnchor(value: unknown): unknown {
  if (!isRecord(value)) {
    return value;
  }

  const strategy = typeof value.strategy === "string"
    ? toKebabCase(value.strategy)
    : value.strategy;

  return {
    ...value,
    strategy
  };
}

function normalizeContentType(value: unknown): unknown {
  if (typeof value === "number") {
    return ["tooltip", "callout", "checklist", "walkthrough"][value] ?? value;
  }

  if (typeof value === "string") {
    const normalized = value.trim().toLowerCase();
    return ["tooltip", "callout", "checklist", "walkthrough"].includes(normalized)
      ? normalized
      : value;
  }

  return value;
}

function normalizeEnvironment(value: unknown): unknown {
  return typeof value === "string" ? value.trim().toLowerCase() : value;
}

function normalizeChannel(value: unknown): unknown {
  if (typeof value === "number") {
    return ["preview", "published"][value] ?? value;
  }

  if (typeof value === "string") {
    const normalized = value.trim().toLowerCase();
    return normalized === "preview" || normalized === "published"
      ? normalized
      : value;
  }

  return value;
}

function buildDeliveryUrl(apiBaseUrl: string, request: DeliveryRequest): string {
  const normalizedBase = apiBaseUrl.trim().replace(/\/+$/, "");
  const applicationId = encodeURIComponent(request.applicationId.trim());
  const environment = encodeURIComponent(request.environment);
  const channel = encodeURIComponent(request.channel);

  return `${normalizedBase}/runtime/delivery/bundles/${applicationId}?environment=${environment}&channel=${channel}`;
}

function createSignalBoundary(
  signal: AbortSignal | undefined,
  timeoutMs: number
): { readonly signal?: AbortSignal; readonly dispose: () => void } {
  if (typeof AbortController === "undefined") {
    return signal === undefined
      ? {
          dispose: noop
        }
      : {
          signal,
          dispose: noop
        };
  }

  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), timeoutMs);
  const abort = (): void => controller.abort();

  if (signal?.aborted === true) {
    controller.abort();
  } else {
    signal?.addEventListener("abort", abort, { once: true });
  }

  return {
    signal: controller.signal,
    dispose: () => {
      clearTimeout(timeoutId);
      signal?.removeEventListener("abort", abort);
    }
  };
}

function failure(
  code: DeliveryFailureCode,
  message: string,
  statusCode?: number,
  issues?: Parameters<typeof createDeliveryFailureResult>[3]
): DeliveryFailureResult {
  return createDeliveryFailureResult(code, message, statusCode, issues);
}

function isRuntimeEnvironment(value: unknown): value is RuntimeEnvironment {
  return value === "development" || value === "test" || value === "production";
}

function isRuntimeChannel(value: unknown): value is RuntimeChannel {
  return value === "preview" || value === "published";
}

function isBlank(value: string): boolean {
  return value.trim().length === 0;
}

function isRecord(value: unknown): value is Readonly<Record<string, unknown>> {
  return typeof value === "object" && value !== null;
}

function toKebabCase(value: string): string {
  return value
    .trim()
    .replace(/([a-z])([A-Z])/g, "$1-$2")
    .toLowerCase();
}

function noop(): void {
  return;
}
