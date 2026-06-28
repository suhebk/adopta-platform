export type AnchorStrategy = "data-adopt-id";

export interface DataAdoptIdAnchorDescriptor {
  readonly strategy: "data-adopt-id";
  readonly value: string;
}

export type AnchorDescriptor = DataAdoptIdAnchorDescriptor;

export type UnknownAnchorDescriptor = Readonly<{
  strategy?: unknown;
  value?: unknown;
}>;

