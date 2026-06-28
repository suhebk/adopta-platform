import { describe, expect, it } from "vitest";
import {
  AdoptaRuntime,
  createNoopRuntimeLogger,
  initializeAdoptaRuntime,
  withRuntimeErrorBoundary,
  type AdoptaRuntimeOptions
} from "../src";

describe("Adopta runtime contract", () => {
  const validOptions: AdoptaRuntimeOptions = {
    tenantId: "tenant-1",
    applicationId: "app-1"
  };

  it("initializes with a strongly typed no-op result", () => {
    const result = initializeAdoptaRuntime(validOptions);

    expect(result.ok).toBe(true);
    if (result.ok) {
      expect(result.context.tenantId).toBe("tenant-1");
      expect(result.context.applicationId).toBe("app-1");
      expect(result.context.noOp).toBe(true);
      expect(result.warnings).toContain("Runtime initialized in no-op mode.");
    }
  });

  it("returns a safe failure result for invalid configuration", () => {
    const result = AdoptaRuntime.initialize({
      tenantId: " ",
      applicationId: "app-1"
    });

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.error.code).toBe("InvalidConfiguration");
      expect(result.error.message).toBe("A non-empty tenantId is required.");
    }
  });

  it("returns a safe failure result when runtime initialization throws", () => {
    const result = initializeAdoptaRuntime({
      ...validOptions,
      hooks: {
        beforeStart: () => {
          throw new Error("sensitive host error");
        }
      }
    });

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.error.code).toBe("RuntimeInitializationFailed");
      expect(result.error.message).toBe("Adopta runtime initialization failed safely.");
      expect(result.error.message).not.toContain("sensitive host error");
    }
  });

  it("allows explicit non-no-op initialization without rendering behavior", () => {
    const result = initializeAdoptaRuntime({
      ...validOptions,
      noOp: false
    });

    expect(result.ok).toBe(true);
    if (result.ok) {
      expect(result.context.noOp).toBe(false);
      expect(result.warnings).toHaveLength(0);
    }
  });

  it("catches errors through the public error boundary", () => {
    const logger = createNoopRuntimeLogger();
    const result = withRuntimeErrorBoundary(() => {
      throw new Error("host details");
    }, logger);

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.error.code).toBe("RuntimeInitializationFailed");
      expect(result.error.message).not.toContain("host details");
    }
  });
});
