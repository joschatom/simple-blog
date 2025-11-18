import axios, {
  Axios,
  AxiosError,
  type AxiosInstance,
  type Method,
} from "axios";

export type PostData = {};
export type UserData = {};
export type UserID = string;
export type PostID = string;

class User {
  #client: APIClient;
  // ...
  constructor(client: APIClient, data: UserData) {
    this.#client = client;
    // ...
  }

  static async getByID(client: APIClient, id: string): Promise<User> {
    const resp = await client.api.get(`/users/${id}`);

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

  api: AxiosInstance;
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

    const isAuth = (await this.api.get<boolean>("/auth/validate")).data;

    if (!isAuth) this.#token = oldToken; // restore old token.

    return isAuth;
  }

  public get api() {
    console.log("API!!!");

    return axios.create({
      baseURL: this.API_BASE,
      headers: {
        Authorization: this.#token ? `Bearer ${this.#token}` : null,
      },
    });
  }

  async login(username: string, password: string): Promise<boolean> {
    const resp = await this.api.post<{ token?: string }>("/auth/login", {
      username,
      password,
    });

    if (resp.data.token) this.#token = resp.data.token;
    return resp.data.token != undefined;
  }

  async logout(): Promise<boolean> {
    if (!this.isAuthenticated()) return false;

    this.#token = undefined;

    await this.api.post("/auth/logout");

    return true;
  }

  // ...
}
