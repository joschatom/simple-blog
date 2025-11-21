import type { AxiosInstance } from "axios";
import { APIError, handleAPIResponse } from "./error.ts";
import axios from "axios";
import z from "zod";
import { User } from "./user.ts";
import { UserData } from "./schemas/user.ts";

export interface APIClient {
  VERSION: string;

  isAuthenticated(): boolean;
  authenticate(token: string): Promise<void>;

  login(username: string, password: string): Promise<void>;
  register(username: string, password: string, email: string): Promise<void>;
  logout(track?: boolean): Promise<boolean>;

  api: AxiosInstance;
  currentUser?: User;
}

export type onTokenChangedHandler = (token: string, newUser: User) => void;

export class WebAPIClient {
  VERSION = "v0.1-web";
  get API_BASE() {
    return `${this.#host}/api/`;
  }

  #host: string;
  token?: string | undefined;
  onTokenChanged?: onTokenChangedHandler;
  currentUser?: User;

  constructor(
    host: string,
    token?: string,
    currentUser?: UserData,
    onTokenChanged?: (tok: string) => void
  ) {
    this.#host = host;
    this.token = token;
    this.onTokenChanged = onTokenChanged;
    this.currentUser = new User(this, currentUser);
  }

  isAuthenticated(): boolean {
    return this.token !== undefined;
  }

  async register(username: string, password: string, email: string): Promise<void>{
   const { token,...userData } = await handleAPIResponse<{ token: string } & UserData>(() =>
      //  throws on error
      this.api.post("/auth/login", {
        username,
        password,
        email
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
    });

    instance.interceptors.request.use((req) => {
      console.debug(`[debug] Fullfilled request to ${req.url}.`);
      return req;
    });

    return instance;
  }

  async login(username: string, password: string): Promise<void> {
    const { token, ...userData} = await handleAPIResponse<{ token: string } & UserData>(() =>
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
