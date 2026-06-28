import type {
  AdoptaRuntimeInitResult,
  RuntimeFailureCode
} from "./RuntimeResult";
import type { RuntimeLogger } from "./RuntimeLogger";

export function createRuntimeFailureResult(
  code: RuntimeFailureCode,
  message: string,
  warnings: readonly string[] = []
): AdoptaRuntimeInitResult {
  return {
    ok: false,
    error: {
      code,
      message
    },
    warnings
  };
}

export function withRuntimeErrorBoundary(
  operation: () => AdoptaRuntimeInitResult,
  logger?: RuntimeLogger
): AdoptaRuntimeInitResult {
  try {
    return operation();
  } catch {
    logger?.error("Adopta runtime initialization failed safely.", {
      code: "RuntimeInitializationFailed"
    });

    return createRuntimeFailureResult(
      "RuntimeInitializationFailed",
      "Adopta runtime initialization failed safely."
    );
  }
}
