import type { AnchorResolver } from "../anchors/AnchorResolver";
import type { RuntimeLogger } from "../runtime/RuntimeLogger";

export interface RendererOptions {
  readonly root?: ParentNode | null;
  readonly document?: Document | null;
  readonly anchorResolver?: AnchorResolver;
  readonly logger?: RuntimeLogger;
}
