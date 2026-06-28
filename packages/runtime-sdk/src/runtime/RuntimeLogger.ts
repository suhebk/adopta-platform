export type RuntimeLogLevel = "debug" | "info" | "warn" | "error";

export type RuntimeSafeLogMetadata = Readonly<Record<string, string | number | boolean | null | undefined>>;

export interface RuntimeLogger {
  readonly debug: (message: string, metadata?: RuntimeSafeLogMetadata) => void;
  readonly info: (message: string, metadata?: RuntimeSafeLogMetadata) => void;
  readonly warn: (message: string, metadata?: RuntimeSafeLogMetadata) => void;
  readonly error: (message: string, metadata?: RuntimeSafeLogMetadata) => void;
}

export function createNoopRuntimeLogger(): RuntimeLogger {
  const log = (): void => {
    return;
  };

  return {
    debug: log,
    info: log,
    warn: log,
    error: log
  };
}
