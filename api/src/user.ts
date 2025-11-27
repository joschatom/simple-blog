import z from "zod";
import type { APIClient } from "./client.ts";
import { handleAPIResponse, parseAPIResponse } from "./error.ts";
import { UpdatedUserDTO, UpdateUserDTO, UserData } from "./schemas/user.js";
import { zs } from "./schemas/shared.ts";

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

  async mute(user_id: string): Promise<void> {
    await handleAPIResponse(() => this.#client.api.post(`/users/${user_id}/mute`))
  }

  async update(update: UpdateUserDTO, sync: boolean = true): Promise<keyof UpdateUserDTO[]> {
    var updated = await parseAPIResponse(zs.updated(UpdateUserDTO), 
      () => this.#client.api.put(
        `/users/${this.data.id}`,
        z.parse(UpdateUserDTO, update)
    ));

    if(updated.updated && sync)
      this.data = (await User.fetchByID(this.#client, this.data.id)).data;
    
    return updated.updatedFields as unknown as keyof UpdateUserDTO[];
  }
}
