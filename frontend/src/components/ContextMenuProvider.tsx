import { type ComponentRef, Fragment, useRef, useState } from "react";
import type { ContextMenuActions } from "../helpers/useContextMenu";
import {
  setContextMenuActions,
  contextMenuElement,
} from "../helpers/useContextMenu";

export function ContextMenuProvider({
  children,
}: {
  children: React.ReactNode;
}) {
  const [actions, setActions] = useState<ContextMenuActions>({});
  
  const ref = useRef<ComponentRef<"dialog">>(null);

  return (
    <>
      <dialog className="context-menu" ref={ref} closedby="any">
        {Object.entries(actions).map(([key, item], i) => ( // TODO: Handle more complex actions.
          <button
            aria-selected={false}
            onClick={() => {
              ref.current?.close();
              if (typeof item == "function") item();
              else if (typeof item == "object")
                (item as { handler: () => void }).handler();
            }}
            key={i}
            disabled={
              typeof item == "object"
                ? (item as { disabled?: boolean })?.disabled
                : undefined
            }
          >
            {key}
          </button>
        ))}
      </dialog>

      <setContextMenuActions.Provider value={setActions}>
        <contextMenuElement.Provider value={ref}>
          <Fragment children={children} />
        </contextMenuElement.Provider>
      </setContextMenuActions.Provider>
    </>
  );
}
