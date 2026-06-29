type TrackedListener = Readonly<{
  target: EventTarget;
  type: string;
  listener: EventListener;
}>;

export class RendererContainer {
  private readonly nodes: Node[] = [];
  private readonly listeners: TrackedListener[] = [];
  private unmounted = false;

  public constructor(private readonly domDocument: Document) {}

  public createElement(tagName: string): HTMLElement {
    return this.domDocument.createElement(tagName);
  }

  public appendNode(parent: Node, node: Node): void {
    parent.appendChild(node);
    this.nodes.push(node);
  }

  public addEventListener(
    target: EventTarget,
    type: string,
    listener: EventListener
  ): void {
    target.addEventListener(type, listener);
    this.listeners.push({
      target,
      type,
      listener
    });
  }

  public unmount(): void {
    if (this.unmounted) {
      return;
    }

    this.unmounted = true;

    for (const tracked of [...this.listeners].reverse()) {
      tracked.target.removeEventListener(tracked.type, tracked.listener);
    }

    this.listeners.length = 0;

    for (const node of [...this.nodes].reverse()) {
      node.parentNode?.removeChild(node);
    }

    this.nodes.length = 0;
  }
}
