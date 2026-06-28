import type { AnchorDescriptor, UnknownAnchorDescriptor } from "./AnchorDescriptor";
import {
  createAnchorFailure,
  type AnchorResolutionResult
} from "./AnchorResolution";
import { DataAdoptIdResolver } from "./DataAdoptIdResolver";

export interface AnchorResolverOptions {
  readonly root?: ParentNode | null;
}

export class AnchorResolver {
  private readonly dataAdoptIdResolver: DataAdoptIdResolver;

  public constructor(dataAdoptIdResolver = new DataAdoptIdResolver()) {
    this.dataAdoptIdResolver = dataAdoptIdResolver;
  }

  public resolve(
    descriptor: AnchorDescriptor | UnknownAnchorDescriptor,
    options: AnchorResolverOptions = {}
  ): AnchorResolutionResult {
    try {
      const validationResult = validateDescriptor(descriptor);
      if (!validationResult.ok) {
        return validationResult;
      }

      if (validationResult.descriptor.strategy === "data-adopt-id") {
        return this.dataAdoptIdResolver.resolve(validationResult.descriptor, options);
      }

      return createAnchorFailure("unsupported_strategy", "Anchor strategy is unsupported.");
    } catch {
      return createAnchorFailure("resolver_error", "Anchor resolver failed safely.");
    }
  }
}

interface DescriptorValidationSuccess {
  readonly ok: true;
  readonly descriptor: AnchorDescriptor;
}

type DescriptorValidationResult = DescriptorValidationSuccess | AnchorResolutionResult;

function validateDescriptor(
  descriptor: AnchorDescriptor | UnknownAnchorDescriptor
): DescriptorValidationResult {
  if (descriptor === undefined || descriptor === null || typeof descriptor !== "object") {
    return createAnchorFailure("invalid_descriptor", "Anchor descriptor is invalid.");
  }

  if (typeof descriptor.strategy !== "string" || descriptor.strategy.trim().length === 0) {
    return createAnchorFailure("invalid_descriptor", "Anchor descriptor is invalid.");
  }

  if (descriptor.strategy !== "data-adopt-id") {
    return createAnchorFailure("unsupported_strategy", "Anchor strategy is unsupported.");
  }

  if (typeof descriptor.value !== "string" || descriptor.value.trim().length === 0) {
    return createAnchorFailure("invalid_descriptor", "Anchor descriptor is invalid.");
  }

  return {
    ok: true,
    descriptor: {
      strategy: "data-adopt-id",
      value: descriptor.value.trim()
    }
  };
}

