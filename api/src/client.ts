import type { AxiosInstance } from "axios";
import { handleAPIResponse } from "./error.ts";
import axios from "axios";

export interface APIClient {
  VERSION: string;

  isAuthenticated(): boolean;
  authenticate(token: string): Promise<void>;
  authenticateUnchecked(token: string): void;

  login(username: string, password: string): Promise<void>;
  logout(track?: boolean): Promise<boolean>;

  api: AxiosInstance;
}

export class WebAPIClient {
  VERSION = "v0.1-web";
  get API_BASE() {
    return `${this.#host}/api/`;
  }

  #host: string;
  #token?: string | undefined;

  /**
   *
   */
  constructor(host: string) {
    this.#host = host;
  }

  isAuthenticated(): boolean {
    return this.#token !== undefined;
  }

  authenticateUnchecked(token: string): void {
    this.#token = token;
  }

  async authenticate(token: string): Promise<void> {
    await handleAPIResponse(() =>
      this.api.get<boolean>("/auth/validate", {
        headers: { Authorization: `Bearer ${token}` },
      })
    );

    this.#token = token;
  }

  public get api() {
    console.debug(
      "[debug] 'WebAPIClient.api' called to created axios instance."
    );

    const instance = axios.create({
      baseURL: this.API_BASE,
      headers: {
        Authorization: this.#token ? `Bearer ${this.#token}` : null,
      },
    });

    instance.interceptors.request.use((req) => {
      console.debug(`[debug] Fullfilled request to ${req.url}.`);
      return req;
    });

    return instance;
  }

  async login(username: string, password: string): Promise<void> {
    const { token } = await handleAPIResponse<{ token: string }>(() =>
      //  throws on error
      this.api.post("/auth/login", {
        username,
        password,
      })
    );

    this.#token = token;
  }

  async logout(_track?: boolean): Promise<boolean> {
    if (!this.isAuthenticated()) return false;

    this.#token = undefined;

    // await this.api.post("/auth/logout");

    return true;
  }
}
