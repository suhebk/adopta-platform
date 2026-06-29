import { describe, expect, it } from "vitest";
import {
  DeliveryClient,
  type DeliveryFailureCode,
  type DeliveryResult,
  type DeliveryTransport,
  type DeliveryTransportRequest,
  type RuntimeLogger,
  type RuntimeSafeLogMetadata
} from "../src";

const applicationId = "11111111-1111-1111-1111-111111111111";

describe("DeliveryClient", () => {
  it("builds the expected delivery URL for a valid request", async () => {
    const requests: DeliveryTransportRequest[] = [];
    const transport: DeliveryTransport = async (request) => {
      requests.push(request);
      return jsonResponse(404, {
        succeeded: false,
        status: "not_found",
        bundle: null,
        issues: []
      });
    };
    const client = new DeliveryClient({
      apiBaseUrl: "/adopta-api/",
      transport
    });

    await client.getBundle({
      applicationId,
      environment: "production",
      channel: "published"
    });

    expect(requests).toEqual([
      expect.objectContaining({
        method: "GET",
        url: `/adopta-api/runtime/delivery/bundles/${applicationId}?environment=production&channel=published`
      })
    ]);
  });

  it("returns a validated content bundle for a successful API response", async () => {
    const client = new DeliveryClient({
      apiBaseUrl: "/adopta-api",
      expectedTenantId: "tenant-1",
      transport: async () => jsonResponse(200, buildApiResponse())
    });

    const result = await client.getBundle({
      applicationId,
      environment: "production",
      channel: "published"
    });

    expect(result.ok).toBe(true);
    if (result.ok) {
      expect(result.statusCode).toBe(200);
      expect(result.bundle).toMatchObject({
        bundleId: "bundle-1",
        tenantId: "tenant-1",
        applicationId,
        environment: "production",
        channel: "published",
        version: "1.0.0"
      });
      expect(result.bundle.items).toEqual([
        expect.objectContaining({
          id: "billing.submit",
          type: "tooltip",
          anchor: {
            strategy: "data-adopt-id",
            value: "billing.submit"
          }
        })
      ]);
      expect(result.bundle.items[0]?.body).toBeUndefined();
    }
  });

  it("fails invalid requests before calling transport", async () => {
    let calls = 0;
    const client = new DeliveryClient({
      apiBaseUrl: "/adopta-api",
      transport: async () => {
        calls += 1;
        return jsonResponse(200, buildApiResponse());
      }
    });

    const result = await client.getBundle({
      applicationId: " ",
      environment: "production",
      channel: "published"
    });

    expectFailure(result, "invalid_request");
    expect(calls).toBe(0);
  });

  it("maps 401 to unauthorized", async () => {
    const result = await clientForStatus(401).getBundle(validRequest());

    expectFailure(result, "unauthorized", 401);
  });

  it("maps 403 to forbidden", async () => {
    const result = await clientForStatus(403).getBundle(validRequest());

    expectFailure(result, "forbidden", 403);
  });

  it("maps 404 to not_found", async () => {
    const result = await clientForStatus(404).getBundle(validRequest());

    expectFailure(result, "not_found", 404);
  });

  it("maps thrown transport errors to network_failure", async () => {
    const client = new DeliveryClient({
      apiBaseUrl: "/adopta-api",
      transport: async () => {
        throw new Error("host transport detail");
      }
    });

    const result = await client.getBundle(validRequest());

    expectFailure(result, "network_failure");
    expectErrorMessageExcludes(result, ["host transport detail"]);
  });

  it("maps timeout or cancellation safely", async () => {
    const controller = new AbortController();
    controller.abort();
    const client = new DeliveryClient({
      apiBaseUrl: "/adopta-api",
      transport: async (request) => {
        expect(request.signal?.aborted).toBe(true);
        throw new Error("aborted by host");
      }
    });

    const result = await client.getBundle({
      ...validRequest(),
      signal: controller.signal
    });

    expectFailure(result, "timeout");
    expectErrorMessageExcludes(result, ["aborted by host"]);
  });

  it("maps malformed responses to unexpected_response", async () => {
    const client = new DeliveryClient({
      apiBaseUrl: "/adopta-api",
      transport: async () => jsonResponse(200, { succeeded: true })
    });

    const result = await client.getBundle(validRequest());

    expectFailure(result, "unexpected_response", 200);
  });

  it("maps invalid returned content to bundle_validation_failed", async () => {
    const client = new DeliveryClient({
      apiBaseUrl: "/adopta-api",
      transport: async () =>
        jsonResponse(200, buildApiResponse({
          content: {
            ...buildContentBundle(),
            items: "not-items"
          }
        }))
    });

    const result = await client.getBundle(validRequest());

    expectFailure(result, "bundle_validation_failed", 200);
    if (!result.ok) {
      expect(result.error.issues).toEqual([
        expect.objectContaining({
          code: "invalid_content_bundle",
          path: "bundle.items"
        })
      ]);
    }
  });

  it("keeps result messages and logger metadata free of sensitive markers", async () => {
    const logs: Array<{ readonly message: string; readonly metadata?: RuntimeSafeLogMetadata }> = [];
    const logger = captureLogger(logs);
    const client = new DeliveryClient({
      apiBaseUrl: "/adopta-api",
      logger,
      transport: async () =>
        jsonResponse(200, buildApiResponse({
          content: {
            ...buildContentBundle(),
            tenantId: "other-tenant"
          }
        })),
      expectedTenantId: "tenant-1"
    });

    const result = await client.getBundle(validRequest());
    const serialized = JSON.stringify({
      result,
      logs
    }).toLowerCase();

    expectFailure(result, "bundle_validation_failed", 200);
    expect(serialized).not.toContain("bearer");
    expect(serialized).not.toContain("password");
    expect(serialized).not.toContain("header");
    expect(serialized).not.toContain("claim");
    expect(serialized).not.toContain("connection");
    expect(serialized).not.toContain("hmrc");
    expect(serialized).not.toContain("property");
    expect(serialized).not.toContain("secret");
    expect(serialized).not.toContain("token");
  });
});

function validRequest() {
  return {
    applicationId,
    environment: "production" as const,
    channel: "published" as const
  };
}

function clientForStatus(status: number): DeliveryClient {
  return new DeliveryClient({
    apiBaseUrl: "/adopta-api",
    transport: async () =>
      jsonResponse(status, {
        succeeded: false,
        status: "failed",
        bundle: null,
        issues: []
      })
  });
}

function jsonResponse(status: number, payload: unknown) {
  return {
    status,
    json: async () => payload
  };
}

function buildApiResponse(overrides: { readonly content?: unknown } = {}) {
  return {
    succeeded: true,
    status: "found",
    bundle: {
      tenantId: "tenant-1",
      applicationId,
      environment: "production",
      channel: 1,
      content: overrides.content ?? buildContentBundle()
    },
    issues: []
  };
}

function buildContentBundle() {
  return {
    bundleId: "bundle-1",
    tenantId: "tenant-1",
    applicationId,
    environment: "production",
    channel: "Published",
    version: "1.0.0",
    generatedAtUtc: "2026-06-29T12:00:00Z",
    items: [
      {
        id: "billing.submit",
        type: 0,
        version: "1.0.0",
        title: "Submit return",
        anchor: {
          strategy: "DataAdoptId",
          value: "billing.submit"
        },
        targeting: {
          mode: "placeholder",
          segments: [],
          pageKeys: []
        }
      }
    ]
  };
}

function expectFailure(
  result: DeliveryResult,
  code: DeliveryFailureCode,
  statusCode?: number
): void {
  expect(result.ok).toBe(false);
  if (!result.ok) {
    expect(result.error.code).toBe(code);
    if (statusCode !== undefined) {
      expect(result.error.statusCode).toBe(statusCode);
    }
  }
}

function expectErrorMessageExcludes(result: DeliveryResult, values: readonly string[]): void {
  expect(result.ok).toBe(false);
  if (!result.ok) {
    for (const value of values) {
      expect(result.error.message).not.toContain(value);
    }
  }
}

function captureLogger(
  logs: Array<{ readonly message: string; readonly metadata?: RuntimeSafeLogMetadata }>
): RuntimeLogger {
  const capture = (message: string, metadata?: RuntimeSafeLogMetadata): void => {
    logs.push(metadata === undefined ? { message } : { message, metadata });
  };

  return {
    debug: capture,
    info: capture,
    warn: capture,
    error: capture
  };
}
