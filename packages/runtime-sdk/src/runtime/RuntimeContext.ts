import type { RuntimeChannel, RuntimeEnvironment } from "./RuntimeOptions";

export interface AdoptaRuntimeContext {
  readonly tenantId: string;
  readonly applicationId: string;
  readonly environment: RuntimeEnvironment;
  readonly channel: RuntimeChannel;
  readonly apiBaseUrl?: string;
  readonly startedAtUtc: string;
  readonly noOp: boolean;
}
