import type { RuntimeLogger } from "./RuntimeLogger";

export type RuntimeEnvironment = "development" | "test" | "production";

export type RuntimeChannel = "preview" | "published";

export interface RuntimeLifecycleHooks {
  readonly beforeStart?: () => void;
}

// Type placeholder only. Element anchoring behavior is intentionally deferred.
export interface RuntimeAnchorPlaceholder {
  readonly key: string;
}

export interface AdoptaRuntimeOptions {
  readonly tenantId: string;
  readonly applicationId: string;
  readonly apiBaseUrl?: string;
  readonly environment?: RuntimeEnvironment;
  readonly channel?: RuntimeChannel;
  readonly logger?: RuntimeLogger;
  readonly hooks?: RuntimeLifecycleHooks;
  readonly anchors?: readonly RuntimeAnchorPlaceholder[];
  readonly noOp?: boolean;
}
