export const rendererThemeToneTokens = [
  "neutral",
  "info",
  "success",
  "warning",
  "critical"
] as const;

export const rendererThemeDensityTokens = [
  "comfortable",
  "compact"
] as const;

export const rendererThemeEmphasisTokens = [
  "subtle",
  "standard",
  "strong"
] as const;

export type RendererThemeTone = (typeof rendererThemeToneTokens)[number];
export type RendererThemeDensity = (typeof rendererThemeDensityTokens)[number];
export type RendererThemeEmphasis = (typeof rendererThemeEmphasisTokens)[number];

export interface RendererTheme {
  readonly tone?: RendererThemeTone;
  readonly density?: RendererThemeDensity;
  readonly emphasis?: RendererThemeEmphasis;
}

export function isRendererThemeTone(value: unknown): value is RendererThemeTone {
  return typeof value === "string" &&
    rendererThemeToneTokens.includes(value as RendererThemeTone);
}

export function isRendererThemeDensity(value: unknown): value is RendererThemeDensity {
  return typeof value === "string" &&
    rendererThemeDensityTokens.includes(value as RendererThemeDensity);
}

export function isRendererThemeEmphasis(value: unknown): value is RendererThemeEmphasis {
  return typeof value === "string" &&
    rendererThemeEmphasisTokens.includes(value as RendererThemeEmphasis);
}
