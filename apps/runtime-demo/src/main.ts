import {
  AnchorResolver,
  initializeAdoptaRuntime,
  validateContentBundle
} from "@adopta/runtime-sdk";
import { demoContentBundle } from "./demoContent";

const statusList = document.querySelector("#adopta-demo-status");
const runtimeResult = initializeAdoptaRuntime({
  tenantId: demoContentBundle.tenantId,
  applicationId: demoContentBundle.applicationId,
  environment: demoContentBundle.environment,
  channel: demoContentBundle.channel,
  noOp: true
});
const contentValidation = validateContentBundle(demoContentBundle);
const resolver = new AnchorResolver();
const anchorResults = demoContentBundle.items.map((item) =>
  item.anchor === undefined
    ? { id: item.id, ok: false }
    : { id: item.id, ok: resolver.resolve(item.anchor).ok }
);

const messages = [
  `Runtime initialised: ${runtimeResult.ok ? "yes" : "no"}`,
  `Content valid: ${contentValidation.ok ? "yes" : "no"}`,
  `Anchors resolved: ${anchorResults.filter((result) => result.ok).length}/${anchorResults.length}`
];

if (statusList !== null) {
  statusList.replaceChildren(
    ...messages.map((message) => {
      const item = document.createElement("li");
      item.textContent = message;
      return item;
    })
  );
}
