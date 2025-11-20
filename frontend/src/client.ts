import { type APIClient, WebAPIClient } from "blog-api";
import { createContext } from "react";

export const Client = createContext<APIClient>(new WebAPIClient("http://localhost:5233"));