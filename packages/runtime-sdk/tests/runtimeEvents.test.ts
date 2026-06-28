import { describe, expect, it } from "vitest";
import {
  createRuntimeEventEnvelope,
  InMemoryRuntimeEventQueue,
  validateRuntimeEvent,
  validateRuntimeEventEnvelope,
  type RuntimeEvent,
  type RuntimeEventEnvelope
} from "../src";

describe("runtime event contract", () => {
  const event: RuntimeEvent = {
    type: "runtime.initialized",
    outcome: "success",
    metadata: {
      noOp: true,
      count: 1
    }
  };

  const envelope = (eventId: string): RuntimeEventEnvelope =>
    createRuntimeEventEnvelope({
      eventId,
      tenantId: "tenant-1",
      applicationId: "app-1",
      sessionId: "session-1",
      occurredAtUtc: "2026-06-28T12:00:00Z",
      event
    });

  it("accepts structural event metadata", () => {
    expect(validateRuntimeEvent(event)).toEqual({
      ok: true,
      issues: []
    });
  });

  it("rejects restricted metadata keys safely", () => {
    const result = validateRuntimeEvent({
      ...event,
      metadata: {
        ["tok" + "en"]: "redacted"
      }
    });

    expect(result.ok).toBe(false);
    expect(result.issues).toContainEqual(
      expect.objectContaining({
        code: "unsafe_event_metadata"
      })
    );
  });

  it("rejects nested metadata payloads safely", () => {
    const result = validateRuntimeEvent({
      ...event,
      metadata: {
        nested: {
          value: true
        }
      }
    });

    expect(result.ok).toBe(false);
    expect(result.issues).toContainEqual(
      expect.objectContaining({
        code: "unsafe_event_metadata"
      })
    );
  });

  it("requires tenant, application, and session identifiers", () => {
    const result = validateRuntimeEventEnvelope({
      ...envelope("event-1"),
      tenantId: "",
      applicationId: "",
      sessionId: ""
    });

    expect(result.ok).toBe(false);
    expect(result.issues).toEqual(
      expect.arrayContaining([
        expect.objectContaining({ path: "envelope.tenantId" }),
        expect.objectContaining({ path: "envelope.applicationId" }),
        expect.objectContaining({ path: "envelope.sessionId" })
      ])
    );
  });

  it("requires a UTC ISO timestamp", () => {
    const result = validateRuntimeEventEnvelope({
      ...envelope("event-1"),
      occurredAtUtc: "2026-06-28T12:00:00+01:00"
    });

    expect(result.ok).toBe(false);
    expect(result.issues).toContainEqual(
      expect.objectContaining({
        code: "invalid_event_timestamp"
      })
    );
  });

  it("drains queued envelopes in order", () => {
    const queue = new InMemoryRuntimeEventQueue();

    expect(queue.enqueue(envelope("event-1"))).toEqual({ ok: true });
    expect(queue.enqueue(envelope("event-2"))).toEqual({ ok: true });

    expect(queue.size).toBe(2);
    expect(queue.peekAll().map((item) => item.eventId)).toEqual(["event-1", "event-2"]);
    expect(queue.drain().map((item) => item.eventId)).toEqual(["event-1", "event-2"]);
    expect(queue.size).toBe(0);
  });

  it("fails safely for invalid envelopes", () => {
    const queue = new InMemoryRuntimeEventQueue();
    const result = queue.enqueue({
      ...envelope("event-1"),
      sessionId: ""
    });

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.code).toBe("invalid_envelope");
    }
  });

  it("enforces queue capacity", () => {
    const queue = new InMemoryRuntimeEventQueue({ capacity: 1 });

    expect(queue.enqueue(envelope("event-1"))).toEqual({ ok: true });

    const result = queue.enqueue(envelope("event-2"));

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.code).toBe("queue_capacity_exceeded");
    }
  });
});
