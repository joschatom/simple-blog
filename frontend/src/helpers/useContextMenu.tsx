import React, {
  createContext,
  useContext,
  useEffect,
  useRef,
  type Dispatch,
  type RefObject,
  type SetStateAction,
} from "react";


export type ContextMenuActions = { 
  [_: string]: {
    handler: () => void,
    disabled?: boolean,
  } | (() => void) 
    | { $node$: React.ReactNode }
    | undefined
};

export const setContextMenuActions = createContext<
  Dispatch<SetStateAction<ContextMenuActions>> | undefined
>(undefined);
export const contextMenuElement = createContext<
  RefObject<HTMLDialogElement | null> | undefined
>(undefined);

export function useContextMenu(
  actions: ContextMenuActions
): RefObject<HTMLElement | null> {
  const ref = useRef<HTMLElement>(null);
  const setActions = useContext(setContextMenuActions);
  const refElmen = useContext(contextMenuElement);

  useEffect(() => {
    if (ref.current == null) return;
    if (setActions == undefined) console.warn("No context menu provideded");

    ref.current.addEventListener("contextmenu", (ev) => {
      ev.stopPropagation();

      console.log(actions, ev);
      setActions!(actions);

      refElmen!.current!.style.top = `${ev.pageY}px`;
      refElmen!.current!.style.left = `${ev.pageX}px`;

      ev.preventDefault();

      refElmen?.current?.show();
    });
  }, [ref, actions, setActions, refElmen]);

  return ref;
}
