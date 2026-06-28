import type { AdoptaRuntimeContext } from "./RuntimeContext";
import type { AdoptaRuntimeOptions } from "./RuntimeOptions";
import type { AdoptaRuntimeInitResult } from "./RuntimeResult";
import { createNoopRuntimeLogger } from "./RuntimeLogger";
import { createRuntimeFailureResult, withRuntimeErrorBoundary } from "./RuntimeErrorBoundary";

export type AdoptaRuntimeInitializer = (options: AdoptaRuntimeOptions) => AdoptaRuntimeInitResult;

export class AdoptaRuntime {
  public static initialize(options: AdoptaRuntimeOptions): AdoptaRuntimeInitResult {
    const logger = options.logger ?? createNoopRuntimeLogger();

    return withRuntimeErrorBoundary(() => {
      const validationError = validateOptions(options);
      if (validationError !== undefined) {
        logger.warn("Adopta runtime configuration rejected.", {
          code: "InvalidConfiguration"
        });

        return createRuntimeFailureResult("InvalidConfiguration", validationError);
      }

      options.hooks?.beforeStart?.();

      const apiBaseUrl = normalizeOptionalString(options.apiBaseUrl);
      const context: AdoptaRuntimeContext = {
        tenantId: options.tenantId.trim(),
        applicationId: options.applicationId.trim(),
        environment: options.environment ?? "production",
        channel: options.channel ?? "published",
        startedAtUtc: new Date().toISOString(),
        noOp: options.noOp ?? true
      };
      const contextWithApiBaseUrl =
        apiBaseUrl === undefined
          ? context
          : {
              ...context,
              apiBaseUrl
            };

      logger.info("Adopta runtime initialized.", {
        noOp: context.noOp,
        environment: context.environment,
        channel: context.channel
      });

      return {
        ok: true,
        context: contextWithApiBaseUrl,
        warnings: context.noOp ? ["Runtime initialized in no-op mode."] : []
      };
    }, logger);
  }
}

export const initializeAdoptaRuntime: AdoptaRuntimeInitializer = AdoptaRuntime.initialize;

function validateOptions(options: AdoptaRuntimeOptions): string | undefined {
  if (isBlank(options.tenantId)) {
    return "A non-empty tenantId is required.";
  }

  if (isBlank(options.applicationId)) {
    return "A non-empty applicationId is required.";
  }

  if (options.apiBaseUrl !== undefined && isBlank(options.apiBaseUrl)) {
    return "apiBaseUrl must be omitted or non-empty.";
  }

  if (options.anchors?.some((anchor) => isBlank(anchor.key)) === true) {
    return "Anchor placeholders must use non-empty keys.";
  }

  return undefined;
}

function isBlank(value: string): boolean {
  return value.trim().length === 0;
}

function normalizeOptionalString(value: string | undefined): string | undefined {
  const normalized = value?.trim();
  return normalized === undefined || normalized.length === 0 ? undefined : normalized;
}
