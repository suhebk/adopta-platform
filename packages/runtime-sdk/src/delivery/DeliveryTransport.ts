export interface DeliveryTransportRequest {
  readonly url: string;
  readonly method: "GET";
  readonly signal?: AbortSignal;
}

export interface DeliveryTransportResponse {
  readonly status: number;
  readonly json: () => Promise<unknown>;
}

export type DeliveryTransport = (request: DeliveryTransportRequest) => Promise<DeliveryTransportResponse>;

export function createFetchDeliveryTransport(fetchImplementation?: typeof fetch): DeliveryTransport {
  return async (request) => {
    const fetchToUse = fetchImplementation ?? globalThis.fetch;
    if (typeof fetchToUse !== "function") {
      throw new Error("fetch_unavailable");
    }

    const init: RequestInit = {
      method: request.method
    };

    if (request.signal !== undefined) {
      init.signal = request.signal;
    }

    const response = await fetchToUse(request.url, init);

    return {
      status: response.status,
      json: async () => response.json() as Promise<unknown>
    };
  };
}
