export const rendererPlacementTokens = [
  "auto",
  "top",
  "right",
  "bottom",
  "left",
  "center",
  "inline",
  "banner"
] as const;

export type RendererPlacementToken = (typeof rendererPlacementTokens)[number];

export interface RendererPlacement {
  readonly preferred: RendererPlacementToken;
  readonly fallback?: readonly RendererPlacementToken[];
}

export function isRendererPlacementToken(value: unknown): value is RendererPlacementToken {
  return typeof value === "string" &&
    rendererPlacementTokens.includes(value as RendererPlacementToken);
}
