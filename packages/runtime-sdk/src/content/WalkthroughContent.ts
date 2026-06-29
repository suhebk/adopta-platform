import type { AnchorDescriptor } from "../anchors/AnchorDescriptor";
import type { RuntimeExperienceMetadata } from "./RuntimeExperienceContent";

export interface WalkthroughStep {
  readonly id: string;
  readonly title: string;
  readonly body?: string;
  readonly anchor?: AnchorDescriptor;
  readonly experience?: RuntimeExperienceMetadata;
}

export interface WalkthroughContent {
  readonly steps: readonly WalkthroughStep[];
}
