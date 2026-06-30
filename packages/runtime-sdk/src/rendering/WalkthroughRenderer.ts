import type { WalkthroughContentItem } from "../content/ContentItem";
import type { WalkthroughStep } from "../content/WalkthroughContent";
import type { RendererContainer } from "./RendererContainer";

export class WalkthroughRenderer {
  public render(
    item: WalkthroughContentItem,
    container: RendererContainer,
    domDocument: Document
  ): void {
    const steps = item.walkthrough?.steps ?? [];
    if (steps.length === 0) {
      return;
    }

    const target = resolveAppendTarget(domDocument);
    let currentStepIndex = 0;

    const surface = container.createElement("section");
    surface.setAttribute("data-adopta-renderer", "walkthrough");
    surface.setAttribute("role", "region");
    surface.setAttribute("aria-label", item.title);

    const title = container.createElement("p");
    title.setAttribute("data-adopta-renderer", "walkthrough-step-title");
    surface.appendChild(title);

    const body = container.createElement("p");
    body.setAttribute("data-adopta-renderer", "walkthrough-step-body");
    surface.appendChild(body);

    const progress = container.createElement("p");
    progress.setAttribute("data-adopta-renderer", "walkthrough-progress");
    surface.appendChild(progress);

    const previous = container.createElement("button");
    previous.setAttribute("type", "button");
    previous.setAttribute("aria-label", "Previous walkthrough step");
    previous.textContent = "Previous";
    surface.appendChild(previous);

    const next = container.createElement("button");
    next.setAttribute("type", "button");
    next.setAttribute("aria-label", "Next walkthrough step");
    next.textContent = "Next";
    surface.appendChild(next);

    const dismiss = container.createElement("button");
    dismiss.setAttribute("type", "button");
    dismiss.setAttribute("aria-label", "Dismiss walkthrough guidance");
    dismiss.textContent = "Dismiss";
    surface.appendChild(dismiss);

    const syncStep = (): void => {
      const currentStep = steps[currentStepIndex];
      if (currentStep === undefined) {
        return;
      }

      renderStep(currentStep, currentStepIndex, steps.length, title, body, progress);
      syncNavigationState(previous, currentStepIndex > 0);
      syncNavigationState(next, currentStepIndex < steps.length - 1);
    };

    const previousListener = (): void => {
      if (currentStepIndex === 0) {
        return;
      }

      currentStepIndex -= 1;
      syncStep();
    };

    const nextListener = (): void => {
      if (currentStepIndex >= steps.length - 1) {
        return;
      }

      currentStepIndex += 1;
      syncStep();
    };

    const dismissListener = (): void => container.unmount();
    const escapeListener = (event: Event): void => {
      if (isEscapeKey(event)) {
        container.unmount();
      }
    };

    syncStep();
    container.addEventListener(previous, "click", previousListener);
    container.addEventListener(next, "click", nextListener);
    container.addEventListener(dismiss, "click", dismissListener);
    container.addEventListener(domDocument, "keydown", escapeListener);
    container.appendNode(target, surface);
  }
}

function renderStep(
  step: WalkthroughStep,
  stepIndex: number,
  stepCount: number,
  title: HTMLElement,
  body: HTMLElement,
  progress: HTMLElement
): void {
  const bodyText = step.body?.trim() ?? "";
  title.textContent = step.title;
  body.textContent = bodyText;
  body.setAttribute("aria-hidden", bodyText.length === 0 ? "true" : "false");
  progress.textContent = `Step ${stepIndex + 1} of ${stepCount}`;
}

function syncNavigationState(button: HTMLElement, enabled: boolean): void {
  button.setAttribute("aria-disabled", enabled ? "false" : "true");
  button.setAttribute("data-adopta-control-state", enabled ? "enabled" : "disabled");
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
