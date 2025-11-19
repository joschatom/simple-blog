import type { APIClient } from "./client";
import { handleAPIResponse } from "./error";
import type { UserData } from "./schemas/user";

export class User {
  #client: APIClient;
  data: UserData;

  constructor(client: APIClient, data: UserData) {
    this.#client = client;
    this.data = data;
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
}
