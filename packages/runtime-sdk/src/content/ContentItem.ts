import type { AnchorDescriptor } from "../anchors/AnchorDescriptor";
import type { ChecklistContent } from "./ChecklistContent";
import type { RuntimeExperienceMetadata } from "./RuntimeExperienceContent";
import type { TargetingPlaceholder } from "./TargetingPlaceholder";
import type { WalkthroughContent } from "./WalkthroughContent";

export interface ContentItemBase {
  readonly id: string;
  readonly version: string;
  readonly title: string;
  readonly body?: string;
  readonly anchor?: AnchorDescriptor;
  readonly targeting?: TargetingPlaceholder;
  readonly experience?: RuntimeExperienceMetadata;
}

export interface TooltipContentItem extends ContentItemBase {
  readonly type: "tooltip";
}

export interface CalloutContentItem extends ContentItemBase {
  readonly type: "callout";
}

export interface ChecklistContentItem extends ContentItemBase {
  readonly type: "checklist";
  readonly checklist?: ChecklistContent;
}

export interface WalkthroughContentItem extends ContentItemBase {
  readonly type: "walkthrough";
  readonly walkthrough?: WalkthroughContent;
}

export type ContentItem =
  | TooltipContentItem
  | CalloutContentItem
  | ChecklistContentItem
  | WalkthroughContentItem;

