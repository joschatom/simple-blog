import type { APIClient } from "./client.ts";
import { handleAPIResponse } from "./error.ts";
import type { UserData } from "./schemas/user.ts";

export class User {
  #client: APIClient;
  data: UserData;

  constructor(client: APIClient, data: UserData) {
    this.#client = client;
    this.data = { ...data, createdAt: new Date(data.createdAt)};
  }

  static async getByID(client: APIClient, id: string): Promise<User> {
    return new User(
      client,
      await handleAPIResponse(() => client.api.get<UserData>(`/users/${id}`))
    );
  }

  static async getByName(client: APIClient, id: string): Promise<User> {
    return new User(
      client,
      await handleAPIResponse(() =>
        client.api.get<UserData>(`/users/by-name/${id}`)
      )
    );
  }

  static async currentUser(client: APIClient): Promise<User> {
    return new User(
      client,
      await handleAPIResponse(() =>
        client.api.get<UserData>(`/users/me`)
      )
    );
  }
}
