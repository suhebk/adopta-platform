import type { AnchorResolutionFailureCode } from "../anchors/AnchorResolution";
import type { ContentValidationIssue } from "../content/ContentValidation";

export type RendererFailureCode =
  | "invalid_bundle"
  | "dom_unavailable"
  | "missing_anchor"
  | "duplicate_anchor"
  | "anchor_resolution_failed"
  | "unsupported_content_type"
  | "render_error";

export interface RendererMount {
  readonly renderedItemCount: number;
  readonly skippedItemCount: number;
  readonly unmount: () => void;
}

export interface RendererItemSuccess {
  readonly ok: true;
  readonly itemId: string;
  readonly contentType: string;
}

export interface RendererItemSkipped {
  readonly ok: false;
  readonly itemId: string;
  readonly contentType: string;
  readonly code: "unsupported_content_type";
  readonly message: string;
}

export interface RendererItemFailure {
  readonly ok: false;
  readonly itemId: string;
  readonly contentType: string;
  readonly code: Exclude<RendererFailureCode, "invalid_bundle" | "dom_unavailable">;
  readonly message: string;
  readonly anchorCode?: AnchorResolutionFailureCode;
}

export type RendererItemResult =
  | RendererItemSuccess
  | RendererItemSkipped
  | RendererItemFailure;

export interface RendererSuccess {
  readonly ok: true;
  readonly mount: RendererMount;
  readonly itemResults: readonly RendererItemResult[];
}

export interface RendererFailure {
  readonly ok: false;
  readonly code: RendererFailureCode;
  readonly message: string;
  readonly itemResults: readonly RendererItemResult[];
  readonly validationIssues?: readonly ContentValidationIssue[];
}

export type RendererResult = RendererSuccess | RendererFailure;

export function createRendererFailure(
  code: RendererFailureCode,
  message: string,
  itemResults: readonly RendererItemResult[] = [],
  validationIssues?: readonly ContentValidationIssue[]
): RendererFailure {
  const failure: RendererFailure = {
    ok: false,
    code,
    message,
    itemResults
  };

  return validationIssues === undefined
    ? failure
    : {
        ...failure,
        validationIssues
      };
}
