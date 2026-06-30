import { AnchorResolver } from "../anchors/AnchorResolver";
import type { AnchorResolutionFailure } from "../anchors/AnchorResolution";
import type { ContentBundle } from "../content/ContentBundle";
import type {
  CalloutContentItem,
  ChecklistContentItem,
  ContentItem,
  TooltipContentItem,
  WalkthroughContentItem
} from "../content/ContentItem";
import { validateContentBundle } from "../content/ContentValidation";
import { createNoopRuntimeLogger } from "../runtime/RuntimeLogger";
import { BannerRenderer } from "./BannerRenderer";
import { ChecklistRenderer } from "./ChecklistRenderer";
import { RendererContainer } from "./RendererContainer";
import type { RendererOptions } from "./RendererOptions";
import {
  createRendererFailure,
  type RendererFailure,
  type RendererFailureCode,
  type RendererItemFailure,
  type RendererItemResult,
  type RendererResult
} from "./RendererResult";
import { TooltipRenderer } from "./TooltipRenderer";
import { WalkthroughRenderer } from "./WalkthroughRenderer";
import { RendererPlacementResolver } from "./RendererPlacementResolver";
import { RendererThemeResolver } from "./RendererThemeResolver";

export class Renderer {
  private readonly anchorResolver: AnchorResolver;
  private readonly tooltipRenderer: TooltipRenderer;
  private readonly bannerRenderer: BannerRenderer;
  private readonly checklistRenderer: ChecklistRenderer;
  private readonly walkthroughRenderer: WalkthroughRenderer;

  public constructor(private readonly options: RendererOptions = {}) {
    this.anchorResolver = options.anchorResolver ?? new AnchorResolver();
    const placementResolver = new RendererPlacementResolver();
    const themeResolver = new RendererThemeResolver();
    this.tooltipRenderer = new TooltipRenderer(placementResolver, themeResolver);
    this.bannerRenderer = new BannerRenderer(placementResolver, themeResolver);
    this.checklistRenderer = new ChecklistRenderer(placementResolver, themeResolver);
    this.walkthroughRenderer = new WalkthroughRenderer(placementResolver, themeResolver);
  }

  public render(bundle: ContentBundle): RendererResult {
    const logger = this.options.logger ?? createNoopRuntimeLogger();
    const validation = validateContentBundle(bundle);
    if (!validation.ok) {
      logger.warn("Adopta renderer rejected invalid bundle.", {
        code: "invalid_bundle",
        issueCount: validation.issues.length
      });

      return createRendererFailure(
        "invalid_bundle",
        "Renderer failed safely.",
        [],
        validation.issues
      );
    }

    const domDocument = resolveDocument(this.options.document);
    if (domDocument === undefined) {
      return createRendererFailure("dom_unavailable", "Renderer failed safely.");
    }

    const container = new RendererContainer(domDocument);
    const itemResults: RendererItemResult[] = [];

    try {
      for (const item of bundle.items) {
        const itemResult = this.renderItem(item, container, domDocument);
        itemResults.push(itemResult);

        if (!itemResult.ok && itemResult.code !== "unsupported_content_type") {
          container.unmount();
          return createRendererFailure(
            itemResult.code,
            "Renderer failed safely.",
            itemResults
          );
        }
      }

      const renderedItemCount = itemResults.filter((result) => result.ok).length;
      const skippedItemCount = itemResults.filter((result) =>
        !result.ok && result.code === "unsupported_content_type").length;

      return {
        ok: true,
        mount: {
          renderedItemCount,
          skippedItemCount,
          unmount: () => container.unmount()
        },
        itemResults
      };
    } catch {
      container.unmount();

      return createRendererFailure(
        "render_error",
        "Renderer failed safely.",
        itemResults
      );
    }
  }

  private renderItem(
    item: ContentItem,
    container: RendererContainer,
    domDocument: Document
  ): RendererItemResult {
    if (item.type === "tooltip") {
      return this.renderTooltip(item, container, domDocument);
    }

    if (item.type === "callout") {
      this.bannerRenderer.render(item as CalloutContentItem, container, domDocument);
      return success(item);
    }

    if (item.type === "checklist") {
      const checklistItem = item as ChecklistContentItem;
      if (checklistItem.checklist === undefined || checklistItem.checklist.steps.length === 0) {
        return unsupported(item);
      }

      this.checklistRenderer.render(checklistItem, container, domDocument);
      return success(item);
    }

    if (item.type === "walkthrough") {
      return this.renderWalkthrough(item, container, domDocument);
    }

    return unsupported(item);
  }

  private renderTooltip(
    item: TooltipContentItem,
    container: RendererContainer,
    domDocument: Document
  ): RendererItemResult {
    if (item.anchor === undefined) {
      return failure(item, "missing_anchor", "Anchor was not found.");
    }

    const anchorOptions = this.options.root === undefined
      ? {}
      : {
          root: this.options.root
        };
    const anchorResult = this.anchorResolver.resolve(item.anchor, anchorOptions);

    if (!anchorResult.ok) {
      return anchorFailure(item, anchorResult);
    }

    this.tooltipRenderer.render(item, anchorResult.element, container, domDocument);
    return success(item);
  }

  private renderWalkthrough(
    item: WalkthroughContentItem,
    container: RendererContainer,
    domDocument: Document
  ): RendererItemResult {
    if (item.walkthrough === undefined || item.walkthrough.steps.length === 0) {
      return unsupported(item);
    }

    const anchorOptions = this.options.root === undefined
      ? {}
      : {
          root: this.options.root
        };

    for (const step of item.walkthrough.steps) {
      if (step.anchor === undefined) {
        continue;
      }

      const anchorResult = this.anchorResolver.resolve(step.anchor, anchorOptions);
      if (!anchorResult.ok) {
        return anchorFailure(item, anchorResult);
      }
    }

    this.walkthroughRenderer.render(item, container, domDocument);
    return success(item);
  }
}

function resolveDocument(domDocument: Document | null | undefined): Document | undefined {
  if (domDocument === null) {
    return undefined;
  }

  if (domDocument !== undefined) {
    return domDocument;
  }

  return globalThis.document;
}

function success(item: ContentItem): RendererItemResult {
  return {
    ok: true,
    itemId: item.id,
    contentType: item.type
  };
}

function unsupported(item: ContentItem): RendererItemResult {
  return {
    ok: false,
    itemId: item.id,
    contentType: item.type,
    code: "unsupported_content_type",
    message: "Content type is unsupported by the renderer foundation."
  };
}

function anchorFailure(
  item: ContentItem,
  anchorResult: AnchorResolutionFailure
): RendererItemFailure {
  const code = mapAnchorFailure(anchorResult);

  return {
    ok: false,
    itemId: item.id,
    contentType: item.type,
    code,
    message: "Renderer failed safely.",
    anchorCode: anchorResult.code
  };
}

function failure(
  item: ContentItem,
  code: RendererFailureCode,
  message: string
): RendererItemFailure {
  return {
    ok: false,
    itemId: item.id,
    contentType: item.type,
    code: code as Exclude<RendererFailureCode, "invalid_bundle" | "dom_unavailable">,
    message
  };
}

function mapAnchorFailure(anchorResult: AnchorResolutionFailure): RendererItemFailure["code"] {
  if (anchorResult.code === "missing_anchor") {
    return "missing_anchor";
  }

  if (anchorResult.code === "duplicate_anchor") {
    return "duplicate_anchor";
  }

  return "anchor_resolution_failed";
}
