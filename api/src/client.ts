import type { AxiosInstance, Method } from "axios";
import { APIError, handleAPIResponse, parseAPIResponseRaw } from "./error.ts";
import axios from "axios";
import { Muting, User } from "./user.ts";
import { UserData } from "./schemas/user.ts";
import { ZodSchema } from "zod/v3";
import z, { string, ZodType } from "zod";
import { jwtDecode } from "jwt-decode";
import { decodeToken } from "./token.ts";

export interface APIClient {
  VERSION: string;

  isAuthenticated(): boolean;
  authenticate(token: string): Promise<void>;

  login(username: string, password: string): Promise<void>;
  register(username: string, password: string, email: string): Promise<void>;
  logout(track?: boolean): Promise<boolean>;
  refreshToken(): Promise<void>;
  changePassword(newPassword: string): Promise<void>;
  deleteAllPosts(): Promise<void>;

  sendRequest<T, R extends ZodType>(
    method: Method,
    url: string,
    body: T,
    recv: R,
    transformer?: (v: unknown) => undefined
  ): Promise<z.infer<typeof recv>>;

  api: AxiosInstance;
  currentUser?: User;
  muting?: Muting
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

  public toString(): string {
    return `[WebAPI Client ${this.VERSION} (base ${this.API_BASE})]`
  }

  async changePassword(newPassword: string){
    newPassword = z.parse(z.string().min(6).max(255), newPassword);

    await this.sendRequest(
      "POST",
      "auth/change-password",
      { password: newPassword },
      z.void().optional()
    )
  }

  constructor(
    host: string,
    token?: string,
    onTokenChanged?: onTokenChangedHandler
  ) {
    this.#host = host;
    this.token = token;
    try {
      if (token !== undefined) {
        const data = decodeToken(token);
        this.currentUser = new User(this, {
          id: data.userId,
          username: data.username,
        });
      }
    } catch (e) {
      console.error(`failed to decode token: ${e}`);
      this.currentUser = null;
    }
    this.onTokenChanged = onTokenChanged;
  }


  public get muting(): Muting | undefined {
    if (!this.isAuthenticated())
      return undefined;
    return new Muting(this);
  }

  sendRequest<T, R extends ZodType>(
    method: Method,
    url: string,
    body: T,
    recv: R,
    transformer?: (v: unknown) => unknown
  ): Promise<z.infer<typeof recv>> {
    return parseAPIResponseRaw(
      recv,
      async () =>
        await fetch(this.API_BASE + url, {
          body: JSON.stringify(body),
          method: method,
          headers: {
            Authorization: this.token ? `Bearer ${this.token}` : undefined,
            "Content-Type": "application/json",
          },
          credentials: "omit",
          cache: "no-cache",
        }),
      transformer
    );
  }

  isAuthenticated(): boolean {
    return this.token !== undefined;
  }

  async refreshToken() {
       const { token, ...userData } = await handleAPIResponse<
      { token: string } & UserData
    >(() =>
      //  throws on error
      this.api.post("/auth/refresh-token")
    );

    this.token = token;
    this.currentUser = new User(this, userData);
    if (this.onTokenChanged) this.onTokenChanged(token, this.currentUser);
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
      this.api.post("/auth/register", {
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
    if (_track && _track) await handleAPIResponse(() => this.api.post("/auth/logout"));

    this.token = undefined;
    this.currentUser = undefined;
    if (this.onTokenChanged) this.onTokenChanged(undefined, undefined);
    return true;
  }

  async deleteAllPosts(): Promise<void> {
    if(!this.isAuthenticated()) throw new APIError("generic", {
      type: "Client Error",
      status: 403,
      title: "Not logged in",
      detail: "Cannot delete all of the user's post as no user is logged in."
    })

    await this.sendRequest("DELETE", "posts/all", undefined, z.void().optional());
  }
}
