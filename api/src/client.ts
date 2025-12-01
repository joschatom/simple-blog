import type { AxiosInstance, Method } from "axios";
import { handleAPIResponse, parseAPIResponseRaw } from "./error.ts";
import axios from "axios";
import { User } from "./user.ts";
import { UserData } from "./schemas/user.ts";
import { ZodSchema } from "zod/v3";
import z, { ZodType } from "zod";

export interface APIClient {
  VERSION: string;

  isAuthenticated(): boolean;
  authenticate(token: string): Promise<void>;

  login(username: string, password: string): Promise<void>;
  register(username: string, password: string, email: string): Promise<void>;
  logout(track?: boolean): Promise<boolean>;

  sendRequest<T, R extends ZodType>(
    method: Method,
    url: string,
    body: T,
    recv: R
  ): Promise<z.infer<typeof recv>>;

  api: AxiosInstance;
  currentUser?: User;
}

export type onTokenChangedHandler = (token?: string, newUser?: User) => void;


export class WebAPIClient {
  VERSION = "v0.1-web";
  get API_BASE() {
    return `${this.#host}/`;
  }

  #host: string;
  token?: string | undefined;
  onTokenChanged?: onTokenChangedHandler;
  currentUser?: User;

  constructor(
    host: string,
    token?: string,
    currentUser?: UserData,
    onTokenChanged?: onTokenChangedHandler
  ) {
    this.#host = host;
    this.token = token;
    this.onTokenChanged = onTokenChanged;
    if (currentUser != undefined)
      this.currentUser = new User(this, currentUser);
  }

  sendRequest<T, R extends ZodType>(
    method: Method,
    url: string,
    body: T,
    recv: R
  ): Promise<z.infer<typeof recv>> {
    return parseAPIResponseRaw(recv, async () =>
      await fetch(this.API_BASE + url, {
        body: JSON.stringify(body),
        method: method,
        headers: {
          Authorization: this.token ? `Bearer ${this.token}` : undefined,
          "Content-Type": "application/json",
        },
        credentials: "omit",
        cache: "no-cache"
      })
    );
  }

  isAuthenticated(): boolean {
    return this.token !== undefined;
  }

  async register(
    username: string,
    password: string,
    email: string
  ): Promise<void> {
    const { token, ...userData } = await handleAPIResponse<
      { token: string } & UserData
    >(() =>
      //  throws on error
      this.api.post("/auth/login", {
        username,
        password,
        email,
      })
    );

    this.token = token;
    this.currentUser = new User(this, userData);
    if (this.onTokenChanged) this.onTokenChanged(token, this.currentUser);
  }

  /**
   *
   * @param token New token
   *
   * @throws API error if preset.
   * @abstract Replaces token and tries to (re)fetch user data.
   */
  async authenticate(token: string): Promise<void> {
    this.token = token;
    this.currentUser = await User.fetchCurrentUser(this);

    if (this.onTokenChanged) this.onTokenChanged(token, this.currentUser);
  }

  public get api() {
    console.debug(
      "[debug] 'WebAPIClient.api' called to created axios instance."
    );

    const instance = axios.create({
      baseURL: this.API_BASE,
      headers: {
        Authorization: this.token ? `Bearer ${this.token}` : null,
      },
      httpVersion: 1,
    });

    instance.interceptors.request.use((req) => {
      console.debug(`[debug] Fullfilled request to ${req.url}.`);
      return req;
    });

    return instance;
  }

  async login(username: string, password: string): Promise<void> {
    const { token, ...userData } = await handleAPIResponse<
      { token: string } & UserData
    >(() =>
      //  throws on error
      this.api.post("/auth/login", {
        username,
        password,
      })
    );

    this.token = token;
    this.currentUser = new User(this, userData);

    if (this.onTokenChanged) this.onTokenChanged(token, this.currentUser);
  }

  async logout(_track?: boolean): Promise<boolean> {
    if (!this.isAuthenticated()) return false;
    await handleAPIResponse(() => this.api.post("/auth/logout"));

    this.token = undefined;
    this.currentUser = undefined;
    if (this.onTokenChanged) this.onTokenChanged(undefined, undefined);
    return true;
  }
}
