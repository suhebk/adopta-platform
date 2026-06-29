import type { ContentBundle } from "../content/ContentBundle";
import type { ContentValidationIssue } from "../content/ContentValidation";

export type DeliveryFailureCode =
  | "invalid_request"
  | "unauthorized"
  | "forbidden"
  | "not_found"
  | "network_failure"
  | "timeout"
  | "unexpected_response"
  | "bundle_validation_failed";

export interface DeliveryFailure {
  readonly code: DeliveryFailureCode;
  readonly message: string;
  readonly statusCode?: number;
  readonly issues?: readonly ContentValidationIssue[];
}

export interface DeliverySuccess {
  readonly ok: true;
  readonly bundle: ContentBundle;
  readonly statusCode: number;
}

export interface DeliveryFailureResult {
  readonly ok: false;
  readonly error: DeliveryFailure;
}

export type DeliveryResult = DeliverySuccess | DeliveryFailureResult;

export function createDeliveryFailureResult(
  code: DeliveryFailureCode,
  message: string,
  statusCode?: number,
  issues?: readonly ContentValidationIssue[]
): DeliveryFailureResult {
  const error: DeliveryFailure = {
    code,
    message
  };

  const errorWithStatus =
    statusCode === undefined
      ? error
      : {
          ...error,
          statusCode
        };

  const errorWithIssues =
    issues === undefined
      ? errorWithStatus
      : {
          ...errorWithStatus,
          issues
        };

  return {
    ok: false,
    error: errorWithIssues
  };
}
