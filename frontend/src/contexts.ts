import { type APIClient, WebAPIClient } from "blog-api";
import { createContext, type Dispatch, type SetStateAction } from "react";

export type Notification = {
    type: "error" | "info" | "general",
    text: string,
    detail?: string
}

export const Client = createContext<APIClient>(new WebAPIClient(""));
export const Notifications = createContext<Notification[]>([]);
export const SetNotification = createContext<Dispatch<SetStateAction<Notification[]>> | null>(null);