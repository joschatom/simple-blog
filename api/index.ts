import axios, { AxiosError, type Method } from "axios";

export type PostData = {};
export type UserData = {};
export type UserID = string;
export type PostID = string;

function defineAPIActions<
  T extends {
    [_: string]: {
      auth: boolean;
      params: Record<string, unknown> | undefined;
      body: unknown | undefined;
      resp: void | unknown;
    };
  }
>(): T {
  throw "unreachable";
}
export type APIActions = ReturnType<
  typeof defineAPIActions<{
    CreateUser: {
      auth: true;
      params: { abc: number };
      body: {
        username: string;
        email: string;
        password: string;
      };
      resp: string;
    };
    GetUserBy: {
      auth: false;
      params: {
        id?: UserID;
        name?: string;
        post_id?: PostID;
      };
      body: undefined;
      resp: UserData;
    };
    UpdateUser: {
      auth: true;
      params: {
        id?: UserID;
        name?: string;
        post_id?: PostID;
      };
      body: undefined;
      resp: void;
    };
    DeleteUser: {
      auth: true;
      params: {
        id: UserID;
      };
      body: undefined;
      resp: boolean;
    };
  }>
>;

class User {
  #client: APIClient;
  // ...
  constructor(client: APIClient, data: UserData) {
    this.#client = client;
    // ...
  }

  static async getByID(client: APIClient, id: string): Promise<User> {
    const resp = await client.exec("GetUserBy", { id }, undefined);

    if (resp.error != undefined) throw resp.error;
    else return new User(client, resp.success!);
  }
}

interface APIClient {
  VERSION: string;

  isAuthenticated(): boolean;
  authenticate(token: string): Promise<boolean>;

  login(username: string, password: string): Promise<boolean>;
  logout(): Promise<boolean>;

  exec(
    action: keyof APIActions,
    params?: APIActions[typeof action]["params"],
    body?: APIActions[typeof action]["body"]
  ): Promise<{ success?: APIActions[typeof action]["resp"]; error?: unknown }>;
}

class WebAPIClient {
  VERSION = "v0.1-web";
  API_BASE: string = "/api/v1";

  #token?: string | undefined;

  isAuthenticated(): boolean {
    return this.#token !== undefined;
  }
  async authenticate(token: string): Promise<boolean> {
    const oldToken = this.#token;
    this.#token = token;

    const isAuth = await this.#sendRequest<boolean>("GET", "/auth/validate");

    if (!isAuth) this.#token = oldToken; // restore old token.

    return isAuth;
  }

  async #sendRequest<T>(
    method: Method,
    url: string,
    data?: object
  ): Promise<T> {
    return (
      await axios({
        method: method,
        baseURL: this.API_BASE,
        headers: {
          Authorization: this.#token ? `Bearer ${this.#token}` : null,
        },
        data,
      })
    ).data;
  }

  async login(username: string, password: string): Promise<boolean> {
    const resp = await this.#sendRequest<{ token?: string }>(
      "GET",
      "/auth/login",
      {
        username,
        password,
      }
    );

    if (resp.token) this.#token = resp.token;
    return resp.token != undefined;
  }

  // ...
}

