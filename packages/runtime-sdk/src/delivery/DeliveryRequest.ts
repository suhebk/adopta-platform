import type { RuntimeChannel, RuntimeEnvironment } from "../runtime/RuntimeOptions";

export interface DeliveryRequest {
  readonly applicationId: string;
  readonly environment: RuntimeEnvironment;
  readonly channel: RuntimeChannel;
  readonly signal?: AbortSignal;
}
