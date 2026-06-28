import { runtimePerformanceBudgets } from "../runtime/RuntimeBudget";
import {
  validateRuntimeEventEnvelope,
  type RuntimeEventEnvelope,
  type RuntimeEventEnvelopeValidationIssue
} from "./EventEnvelope";

export interface RuntimeEventQueueOptions {
  readonly capacity?: number;
}

export type RuntimeEventQueueFailureCode = "invalid_envelope" | "queue_capacity_exceeded";

export interface RuntimeEventQueueSuccess {
  readonly ok: true;
}

export interface RuntimeEventQueueFailure {
  readonly ok: false;
  readonly code: RuntimeEventQueueFailureCode;
  readonly message: string;
  readonly issues?: readonly RuntimeEventEnvelopeValidationIssue[];
}

export type RuntimeEventQueueResult = RuntimeEventQueueSuccess | RuntimeEventQueueFailure;

export class InMemoryRuntimeEventQueue {
  private readonly capacity: number;
  private readonly envelopes: RuntimeEventEnvelope[] = [];

  public constructor(options: RuntimeEventQueueOptions = {}) {
    this.capacity = Math.max(1, Math.floor(options.capacity ?? runtimePerformanceBudgets.maxInMemoryEventQueueLength));
  }

  public get size(): number {
    return this.envelopes.length;
  }

  public enqueue(envelope: RuntimeEventEnvelope): RuntimeEventQueueResult {
    const validation = validateRuntimeEventEnvelope(envelope);

    if (!validation.ok) {
      return {
        ok: false,
        code: "invalid_envelope",
        message: "Runtime event envelope was rejected safely.",
        issues: validation.issues
      };
    }

    if (this.envelopes.length >= this.capacity) {
      return {
        ok: false,
        code: "queue_capacity_exceeded",
        message: "Runtime event queue capacity was reached."
      };
    }

    this.envelopes.push(envelope);

    return {
      ok: true
    };
  }

  public peekAll(): readonly RuntimeEventEnvelope[] {
    return [...this.envelopes];
  }

  public drain(): readonly RuntimeEventEnvelope[] {
    const drained = [...this.envelopes];
    this.envelopes.length = 0;
    return drained;
  }
}
