import { describe, expect, it } from "vitest";
import { runtimeAccessibilityContract, runtimePerformanceBudgets } from "../src";

describe("runtime accessibility and budget contracts", () => {
  it("defines baseline accessibility expectations", () => {
    expect(runtimeAccessibilityContract.keyboardOperabilityRequired).toBe(true);
    expect(runtimeAccessibilityContract.focusManagementRequired).toBe(true);
    expect(runtimeAccessibilityContract.reducedMotionRespected).toBe(true);
    expect(runtimeAccessibilityContract.ariaLabellingRequired).toBe(true);
    expect(runtimeAccessibilityContract.minimumContrastRatio).toBeGreaterThanOrEqual(4.5);
    expect(runtimeAccessibilityContract.escapeDismissRequired).toBe(true);
  });

  it("defines conservative positive runtime budgets", () => {
    expect(runtimePerformanceBudgets.coreSdkGzipBytes).toBeGreaterThan(0);
    expect(runtimePerformanceBudgets.coreSdkGzipBytes).toBeLessThanOrEqual(102_400);
    expect(runtimePerformanceBudgets.synchronousInitMilliseconds).toBeGreaterThan(0);
    expect(runtimePerformanceBudgets.synchronousInitMilliseconds).toBeLessThanOrEqual(50);
    expect(runtimePerformanceBudgets.anchorResolutionBatchMilliseconds).toBeGreaterThan(0);
    expect(runtimePerformanceBudgets.anchorResolutionBatchMilliseconds).toBeLessThanOrEqual(50);
    expect(runtimePerformanceBudgets.maxInMemoryEventQueueLength).toBeGreaterThan(0);
    expect(runtimePerformanceBudgets.maxInMemoryEventQueueLength).toBeLessThanOrEqual(100);
  });
});
