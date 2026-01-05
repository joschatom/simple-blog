import { Toast } from "radix-ui";
import {
  type Dispatch,
  Fragment,
  type SetStateAction,
  useEffect,
  useState,
} from "react";
import {
  SetNotification as SetNotifications,
  type Notification,
} from "../contexts";
import Close from "../assets/icons/close.svg?react";

import "../styles/components/Notification.css";

function NotificationToast({
  notification,
  setNotifications,
  index,
}: {
  notification: Notification;
  index: number;
  setNotifications: Dispatch<SetStateAction<Notification[]>>;
}) {
  const [detail, setDetail] = useState(false);

  let level = "neutral";
  switch (notification.type) {
    case "error":
      level = "danger";
      break;
    case "info":
      level = "notice";
      break;    
    case "success":
      level = "success";
      break;
  };

  return (
    <Toast.Root
      onClick={() => setDetail(!detail)}
      className="notification leveled"
      data-level={level}
      open={true}
      onOpenChange={() =>
        setNotifications((ns) => ns.filter((_, i) => i !== index))
      }
    >
      <Toast.Title
        className="notification-title"
        onClick={() => setDetail(!detail)}
        children={notification.text}
      >
      </Toast.Title>
      <Toast.Description
        hidden={!detail || notification.detail == undefined}
        className="notification-desc"
        children={notification.detail}
      >
      </Toast.Description>
      <Toast.Action
        onClick={undefined}
        className="notification-action"
        asChild
        altText="Dismiss notification"
      >
        <button className="notification-dismiss">
          <Close className="leveled" data-level={level}/>
        </button>
      </Toast.Action>
    </Toast.Root>
  );
}

export function NotificationProvider({
  children,
}: {
  children: React.ReactNode;
}) {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  useEffect(() => console.log(notifications), [notifications]);

  return (
    <>
      <Toast.Provider swipeDirection="right">
        {notifications.map((n, i) => (
          <NotificationToast
            notification={n}
            key={i}
            index={i}
            setNotifications={setNotifications}
          />
        ))}
        <Toast.Viewport className="notification-viewport" />
      </Toast.Provider>

      <SetNotifications.Provider value={setNotifications}>
        <Fragment children={children} />
      </SetNotifications.Provider>
    </>
  );
}
