import type { AnchorDescriptor } from "../anchors/AnchorDescriptor";
import type { RuntimeExperienceMetadata } from "./RuntimeExperienceContent";

export interface ChecklistStep {
  readonly id: string;
  readonly title: string;
  readonly body?: string;
  readonly anchor?: AnchorDescriptor;
  readonly experience?: RuntimeExperienceMetadata;
}

export type ChecklistItem = ChecklistStep;

export interface ChecklistContent {
  readonly steps: readonly ChecklistStep[];
}
