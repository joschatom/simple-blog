import z from "zod";
import type { APIClient } from "./client.ts";
import { handleAPIResponse, parseAPIResponse } from "./error.ts";
import { UserData } from "./schemas/user.js";

export class User {
  #client: APIClient;
  data: UserData;

  constructor(client: APIClient, data: UserData) {
    this.#client = client;
    this.data = data;
  }

  static async fetchAll(client: APIClient): Promise<User[]> {
    return await parseAPIResponse(z.array(UserData), () =>
      client.api.get<UserData[]>(`/users`)
    ).then(users => users.map(u => new User(client, u)))
  }

  static async fetchByID(client: APIClient, id: string): Promise<User> {
    return new User(
      client,
      await parseAPIResponse(UserData, () =>
        client.api.get<UserData>(`/users/${id}`)
      )
    );
  }

  static async fetchByName(client: APIClient, id: string): Promise<User> {
    return new User(
      client,
      await parseAPIResponse(UserData, () =>
        client.api.get<UserData>(`/users/by-name/${id}`)
      )
    );
  }

  static async fetchCurrentUser(client: APIClient): Promise<User> {
    return new User(
      client,
      await parseAPIResponse(UserData, () => client.api.get(`/users/me`))
    );
  }
}
