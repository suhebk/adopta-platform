import type { RuntimeLogger } from "../runtime/RuntimeLogger";
import type { DeliveryTransport } from "./DeliveryTransport";

export interface DeliveryClientOptions {
  readonly apiBaseUrl: string;
  readonly expectedTenantId?: string;
  readonly timeoutMs?: number;
  readonly logger?: RuntimeLogger;
  readonly transport?: DeliveryTransport;
}
