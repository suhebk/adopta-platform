export const contentTypes = ["tooltip", "callout", "checklist", "walkthrough"] as const;

export type ContentType = (typeof contentTypes)[number];

export function isContentType(value: unknown): value is ContentType {
  return typeof value === "string" && contentTypes.includes(value as ContentType);
}

