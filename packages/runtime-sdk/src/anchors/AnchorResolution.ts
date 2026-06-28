import type { AnchorDescriptor } from "./AnchorDescriptor";

export type AnchorResolutionFailureCode =
  | "invalid_descriptor"
  | "dom_unavailable"
  | "missing_anchor"
  | "duplicate_anchor"
  | "unsupported_strategy"
  | "resolver_error";

export interface AnchorResolutionSuccess {
  readonly ok: true;
  readonly descriptor: AnchorDescriptor;
  readonly element: Element;
}

export interface AnchorResolutionFailure {
  readonly ok: false;
  readonly code: AnchorResolutionFailureCode;
  readonly message: string;
}

export type AnchorResolutionResult = AnchorResolutionSuccess | AnchorResolutionFailure;

export function createAnchorFailure(
  code: AnchorResolutionFailureCode,
  message: string
): AnchorResolutionFailure {
  return {
    ok: false,
    code,
    message
  };
}

