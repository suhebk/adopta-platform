import type { DataAdoptIdAnchorDescriptor } from "./AnchorDescriptor";
import {
  createAnchorFailure,
  type AnchorResolutionResult
} from "./AnchorResolution";

export interface DataAdoptIdResolverOptions {
  readonly root?: ParentNode | null;
}

export class DataAdoptIdResolver {
  public resolve(
    descriptor: DataAdoptIdAnchorDescriptor,
    options: DataAdoptIdResolverOptions = {}
  ): AnchorResolutionResult {
    try {
      if (descriptor.value.trim().length === 0) {
        return createAnchorFailure("invalid_descriptor", "Anchor descriptor is invalid.");
      }

      const root = options.root ?? globalThis.document;
      if (root === undefined || root === null) {
        return createAnchorFailure("dom_unavailable", "DOM is unavailable.");
      }

      const matches = findDataAdoptIdMatches(root, descriptor.value.trim());
      if (matches.length === 0) {
        return createAnchorFailure("missing_anchor", "Anchor was not found.");
      }

      if (matches.length > 1) {
        return createAnchorFailure("duplicate_anchor", "Anchor resolved to multiple elements.");
      }

      const element = matches[0];
      if (element === undefined) {
        return createAnchorFailure("missing_anchor", "Anchor was not found.");
      }

      return {
        ok: true,
        descriptor,
        element
      };
    } catch {
      return createAnchorFailure("resolver_error", "Anchor resolver failed safely.");
    }
  }
}

function findDataAdoptIdMatches(root: ParentNode, value: string): Element[] {
  const matches: Element[] = [];

  root.querySelectorAll("[data-adopt-id]").forEach((element) => {
    if (element.getAttribute("data-adopt-id") === value) {
      matches.push(element);
    }
  });

  return matches;
}

