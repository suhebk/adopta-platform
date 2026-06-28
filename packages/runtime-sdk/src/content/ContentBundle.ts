import type { RuntimeChannel, RuntimeEnvironment } from "../runtime/RuntimeOptions";
import type { ContentItem } from "./ContentItem";

export interface ContentBundle {
  readonly bundleId: string;
  readonly tenantId: string;
  readonly applicationId: string;
  readonly environment: RuntimeEnvironment;
  readonly channel: RuntimeChannel;
  readonly version: string;
  readonly generatedAtUtc: string;
  readonly items: readonly ContentItem[];
}

