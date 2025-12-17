import { type APIClient, WebAPIClient } from "blog-api";
import { createContext, type ReactNode } from "react";
import { type Dispatch, type SetStateAction } from "react";

export const Client = createContext<APIClient>(new WebAPIClient(""));

export type Notification = {
  type: "error" | "info" | "general";
  text: ReactNode;
  detail?: ReactNode;
};

export const Notifications = createContext<Notification[]>([]);
export const SetNotification = createContext<Dispatch<
  SetStateAction<Notification[]>
> | null>(null);
