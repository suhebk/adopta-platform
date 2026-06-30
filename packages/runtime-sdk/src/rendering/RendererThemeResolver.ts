import {
  isRendererThemeDensity,
  isRendererThemeEmphasis,
  isRendererThemeTone,
  type RendererThemeDensity,
  type RendererThemeEmphasis,
  type RendererThemeTone
} from "../content/RendererTheme";
import type { RuntimeExperienceMetadata } from "../content/RuntimeExperienceContent";

const defaultTone: RendererThemeTone = "neutral";
const defaultDensity: RendererThemeDensity = "comfortable";
const defaultEmphasis: RendererThemeEmphasis = "standard";

export class RendererThemeResolver {
  public apply(
    surface: HTMLElement,
    metadata: RuntimeExperienceMetadata | undefined
  ): void {
    const theme = metadata?.theme;
    const tone = isRendererThemeTone(theme?.tone) ? theme.tone : defaultTone;
    const density = isRendererThemeDensity(theme?.density)
      ? theme.density
      : defaultDensity;
    const emphasis = isRendererThemeEmphasis(theme?.emphasis)
      ? theme.emphasis
      : defaultEmphasis;

    surface.setAttribute("data-adopta-theme-tone", tone);
    surface.setAttribute("data-adopta-theme-density", density);
    surface.setAttribute("data-adopta-theme-emphasis", emphasis);
  }
}
