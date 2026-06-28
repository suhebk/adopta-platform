import type { AdoptaRuntimeContext } from "./RuntimeContext";

export type RuntimeFailureCode =
  | "InvalidConfiguration"
  | "RuntimeInitializationFailed";

export interface RuntimeFailure {
  readonly code: RuntimeFailureCode;
  readonly message: string;
}

export interface RuntimeSuccess {
  readonly ok: true;
  readonly context: AdoptaRuntimeContext;
  readonly warnings: readonly string[];
}

export interface RuntimeFailureResult {
  readonly ok: false;
  readonly error: RuntimeFailure;
  readonly warnings: readonly string[];
}

export type AdoptaRuntimeInitResult = RuntimeSuccess | RuntimeFailureResult;
