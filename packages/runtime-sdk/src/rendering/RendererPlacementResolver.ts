import {
  isRendererPlacementToken,
  type RendererPlacementToken
} from "../content/RendererPlacement";
import type { RuntimeExperienceMetadata } from "../content/RuntimeExperienceContent";

export class RendererPlacementResolver {
  public apply(
    surface: HTMLElement,
    metadata: RuntimeExperienceMetadata | undefined,
    defaultPlacement: RendererPlacementToken
  ): void {
    const preferred = resolvePlacementToken(
      metadata?.placement?.preferred,
      defaultPlacement
    );
    const fallback = (metadata?.placement?.fallback ?? [])
      .filter(isRendererPlacementToken);

    surface.setAttribute("data-adopta-placement", preferred);
    surface.setAttribute("data-adopta-placement-fallback", fallback.join(" "));
  }
}

function resolvePlacementToken(
  value: unknown,
  fallback: RendererPlacementToken
): RendererPlacementToken {
  return isRendererPlacementToken(value) ? value : fallback;
}
