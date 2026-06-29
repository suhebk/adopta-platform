import type { ChecklistContentItem } from "../content/ContentItem";
import type { ChecklistStep } from "../content/ChecklistContent";
import type { RendererContainer } from "./RendererContainer";

export class ChecklistRenderer {
  public render(
    item: ChecklistContentItem,
    container: RendererContainer,
    domDocument: Document
  ): void {
    const target = resolveAppendTarget(domDocument);
    const surface = container.createElement("section");
    surface.setAttribute("data-adopta-renderer", "checklist");
    surface.setAttribute("role", "region");
    surface.setAttribute("aria-label", item.title);

    const title = container.createElement("p");
    title.setAttribute("data-adopta-renderer", "checklist-title");
    title.textContent = item.title;
    surface.appendChild(title);

    if (item.body !== undefined && item.body.trim().length > 0) {
      const body = container.createElement("p");
      body.setAttribute("data-adopta-renderer", "checklist-body");
      body.textContent = item.body;
      surface.appendChild(body);
    }

    const steps = item.checklist?.steps ?? [];
    if (steps.length > 0) {
      const list = container.createElement("ol");
      list.setAttribute("data-adopta-renderer", "checklist-steps");
      list.setAttribute("role", "list");

      for (const step of steps) {
        list.appendChild(renderStep(step, container));
      }

      surface.appendChild(list);
    } else {
      const emptyState = container.createElement("p");
      emptyState.setAttribute("data-adopta-renderer", "checklist-empty-state");
      emptyState.textContent = "No checklist steps are available.";
      surface.appendChild(emptyState);
    }

    const dismiss = container.createElement("button");
    dismiss.setAttribute("type", "button");
    dismiss.setAttribute("aria-label", "Dismiss checklist guidance");
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

function renderStep(step: ChecklistStep, container: RendererContainer): HTMLElement {
  const item = container.createElement("li");
  item.setAttribute("data-adopta-renderer", "checklist-step");
  item.setAttribute("role", "listitem");
  item.setAttribute("data-adopta-checklist-state", "incomplete");

  const title = container.createElement("span");
  title.setAttribute("data-adopta-renderer", "checklist-step-title");
  title.textContent = step.title;
  item.appendChild(title);

  if (step.body !== undefined && step.body.trim().length > 0) {
    const body = container.createElement("span");
    body.setAttribute("data-adopta-renderer", "checklist-step-body");
    body.textContent = step.body;
    item.appendChild(body);
  }

  const state = container.createElement("span");
  state.setAttribute("data-adopta-renderer", "checklist-step-state");
  state.textContent = "Incomplete";
  item.appendChild(state);

  return item;
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
