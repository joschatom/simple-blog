import { useContext } from "react";
import { SetNotification, type Notification } from "../contexts";

export function useUserNotify() {
  const setter = useContext(SetNotification);

  if (setter == null)
    return (n: Notification) =>
      console.error("notification send outside of provider", n);
  else return (n: Notification) => setter((ns) => [...ns, n]);
}
