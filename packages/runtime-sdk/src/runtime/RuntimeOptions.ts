import type { AnchorDescriptor } from "../anchors/AnchorDescriptor";
import type { RuntimeLogger } from "./RuntimeLogger";

export type RuntimeEnvironment = "development" | "test" | "production";

export type RuntimeChannel = "preview" | "published";

export interface RuntimeLifecycleHooks {
  readonly beforeStart?: () => void;
}

export interface AdoptaRuntimeOptions {
  readonly tenantId: string;
  readonly applicationId: string;
  readonly apiBaseUrl?: string;
  readonly environment?: RuntimeEnvironment;
  readonly channel?: RuntimeChannel;
  readonly logger?: RuntimeLogger;
  readonly hooks?: RuntimeLifecycleHooks;
  readonly anchors?: readonly AnchorDescriptor[];
  readonly noOp?: boolean;
}
