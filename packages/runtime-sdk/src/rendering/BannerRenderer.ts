import type { CalloutContentItem } from "../content/ContentItem";
import type { RendererContainer } from "./RendererContainer";
import { RendererPlacementResolver } from "./RendererPlacementResolver";
import { RendererThemeResolver } from "./RendererThemeResolver";

export class BannerRenderer {
  public constructor(
    private readonly placementResolver: RendererPlacementResolver = new RendererPlacementResolver(),
    private readonly themeResolver: RendererThemeResolver = new RendererThemeResolver()
  ) {}

  public render(
    item: CalloutContentItem,
    container: RendererContainer,
    domDocument: Document
  ): void {
    const target = resolveAppendTarget(domDocument);
    const surface = container.createElement("section");
    surface.setAttribute("data-adopta-renderer", "banner");
    surface.setAttribute("role", "status");
    surface.setAttribute("aria-label", item.title);
    this.placementResolver.apply(surface, item.experience, "banner");
    this.themeResolver.apply(surface, item.experience);

    const title = container.createElement("p");
    title.setAttribute("data-adopta-renderer", "banner-title");
    title.textContent = item.title;
    surface.appendChild(title);

    if (item.body !== undefined && item.body.trim().length > 0) {
      const body = container.createElement("p");
      body.setAttribute("data-adopta-renderer", "banner-body");
      body.textContent = item.body;
      surface.appendChild(body);
    }

    const dismiss = container.createElement("button");
    dismiss.setAttribute("type", "button");
    dismiss.setAttribute("aria-label", "Dismiss guidance");
    dismiss.textContent = "Dismiss";
    surface.appendChild(dismiss);

    const dismissListener = (): void => container.unmount();
    const escapeListener = (event: Event): void => {
      if (isEscapeKey(event)) {
        container.unmount();
      }
    };

    container.addEventListener(dismiss, "click", dismissListener);
    container.addEventListener(domDocument, "keydown", escapeListener);
    container.appendNode(target, surface);
  }
}

function resolveAppendTarget(domDocument: Document): Node {
  if (domDocument.body !== null) {
    return domDocument.body;
  }

  if (domDocument.documentElement !== null) {
    return domDocument.documentElement;
  }

  throw new Error("append_target_unavailable");
}

function isEscapeKey(event: Event): boolean {
  return "key" in event && event.key === "Escape";
}
