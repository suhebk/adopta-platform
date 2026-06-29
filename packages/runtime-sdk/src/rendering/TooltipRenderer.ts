import type { TooltipContentItem } from "../content/ContentItem";
import type { RendererContainer } from "./RendererContainer";

export class TooltipRenderer {
  public render(
    item: TooltipContentItem,
    anchor: Element,
    container: RendererContainer,
    domDocument: Document
  ): void {
    const surface = container.createElement("aside");
    surface.setAttribute("data-adopta-renderer", "tooltip");
    surface.setAttribute("role", "tooltip");
    surface.setAttribute("aria-label", item.title);

    const title = container.createElement("p");
    title.setAttribute("data-adopta-renderer", "tooltip-title");
    title.textContent = item.title;
    surface.appendChild(title);

    if (item.body !== undefined && item.body.trim().length > 0) {
      const body = container.createElement("p");
      body.setAttribute("data-adopta-renderer", "tooltip-body");
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
    container.appendNode(resolveAnchorParent(anchor), surface);
  }
}

function resolveAnchorParent(anchor: Element): Node {
  if (anchor.parentNode === null) {
    throw new Error("anchor_parent_unavailable");
  }

  return anchor.parentNode;
}

function isEscapeKey(event: Event): boolean {
  return "key" in event && event.key === "Escape";
}
