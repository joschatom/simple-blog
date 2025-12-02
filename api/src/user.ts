import z from "zod";
import type { APIClient } from "./client.ts";
import { handleAPIResponse, parseAPIResponse } from "./error.ts";
import { UpdateUserDTO, UserData } from "./schemas/user.ts";
import { zs } from "./schemas/shared.ts";
import { CreatePost, PostData } from "./schemas/post.ts";
import { Post } from "./post.ts";

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


  async delete() {
    await handleAPIResponse(() => this.#client.api.delete(`users/${this.data.id}`));
  }
  async posts(): Promise<Post[]> { 
    return await parseAPIResponse(z.array(PostData), () => this.#client.api.get(`users/${this.data.id}/posts`))
      .then(r => r.map(p => new Post(this.#client, p)))
  }
}