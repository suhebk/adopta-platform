import type { ContentBundle } from "@adopta/runtime-sdk";

export const demoContentBundle: ContentBundle = {
  bundleId: "runtime-demo-local-bundle",
  tenantId: "runtime-demo-tenant",
  applicationId: "runtime-demo-application",
  environment: "development",
  channel: "preview",
  version: "2026.06.28",
  generatedAtUtc: "2026-06-28T12:00:00Z",
  items: [
    {
      id: "demo-overview",
      type: "callout",
      version: "1.0.0",
      title: "Local overview",
      body: "Confirms the SDK can initialise against a local page.",
      anchor: {
        strategy: "data-adopt-id",
        value: "demo.nav.overview"
      },
      targeting: {
        mode: "placeholder",
        segments: ["local-demo"],
        pageKeys: ["runtime-demo"]
      }
    },
    {
      id: "demo-submit",
      type: "tooltip",
      version: "1.0.0",
      title: "Submit action",
      body: "Confirms a stable action anchor can be resolved.",
      anchor: {
        strategy: "data-adopt-id",
        value: "demo.billing.submit"
      },
      targeting: {
        mode: "placeholder",
        segments: ["local-demo"],
        pageKeys: ["runtime-demo"]
      }
    },
    {
      id: "demo-checklist",
      type: "checklist",
      version: "1.0.0",
      title: "Checklist entry",
      body: "Confirms checklist content can share the same local contract.",
      anchor: {
        strategy: "data-adopt-id",
        value: "demo.checklist.open"
      },
      targeting: {
        mode: "placeholder",
        segments: ["local-demo"],
        pageKeys: ["runtime-demo"]
      }
    },
    {
      id: "demo-walkthrough",
      type: "walkthrough",
      version: "1.0.0",
      title: "Walkthrough entry",
      body: "Confirms walkthrough content remains contract-only.",
      anchor: {
        strategy: "data-adopt-id",
        value: "demo.walkthrough.start"
      },
      targeting: {
        mode: "placeholder",
        segments: ["local-demo"],
        pageKeys: ["runtime-demo"]
      }
    }
  ]
};
