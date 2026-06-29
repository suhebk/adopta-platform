import type { RendererPlacement } from "./RendererPlacement";
import type { RendererTheme } from "./RendererTheme";

export const dismissBehaviorTokens = [
  "dismiss-button",
  "escape-key",
  "outside-click",
  "auto-timeout"
] as const;

export type DismissBehaviorToken = (typeof dismissBehaviorTokens)[number];

export interface RuntimeExperienceMetadata {
  readonly placement?: RendererPlacement;
  readonly dismissBehavior?: readonly DismissBehaviorToken[];
  readonly theme?: RendererTheme;
}

export function isDismissBehaviorToken(value: unknown): value is DismissBehaviorToken {
  return typeof value === "string" &&
    dismissBehaviorTokens.includes(value as DismissBehaviorToken);
}
