export const runtimeEventTypes = [
  "runtime.initialized",
  "content.validated",
  "anchor.resolved",
  "anchor.missing",
  "content.suppressed",
  "runtime.error"
] as const;

export type RuntimeEventType = (typeof runtimeEventTypes)[number];

export function isRuntimeEventType(value: unknown): value is RuntimeEventType {
  return typeof value === "string" && runtimeEventTypes.includes(value as RuntimeEventType);
}
