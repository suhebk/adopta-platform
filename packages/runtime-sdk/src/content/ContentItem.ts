import type { AnchorDescriptor } from "../anchors/AnchorDescriptor";
import type { TargetingPlaceholder } from "./TargetingPlaceholder";

export interface ContentItemBase {
  readonly id: string;
  readonly version: string;
  readonly title: string;
  readonly body?: string;
  readonly anchor?: AnchorDescriptor;
  readonly targeting?: TargetingPlaceholder;
}

export interface TooltipContentItem extends ContentItemBase {
  readonly type: "tooltip";
}

export interface CalloutContentItem extends ContentItemBase {
  readonly type: "callout";
}

export interface ChecklistContentItem extends ContentItemBase {
  readonly type: "checklist";
}

export interface WalkthroughContentItem extends ContentItemBase {
  readonly type: "walkthrough";
}

export type ContentItem =
  | TooltipContentItem
  | CalloutContentItem
  | ChecklistContentItem
  | WalkthroughContentItem;

