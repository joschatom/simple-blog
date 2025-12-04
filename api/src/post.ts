import z from "zod";
import type { APIClient } from "./client.ts";
import { APIError, handleAPIResponse, parseAPIResponse } from "./error.ts";
import { CreatePost, PostData, UpdatePost } from "./schemas/post.ts";
import { updated } from "./schemas/user.ts";
import { zs } from "./schemas/shared.ts";
export class Post {
  data: PostData;
  #client: APIClient;

  constructor(client: APIClient, data: PostData) {
    this.data = data;
    this.#client = client;
  }

  static async fetchById(client: APIClient, id: string): Promise<Post> {
    const data = await parseAPIResponse(PostData, () =>
      client.api.get(`posts/${id}`)
    );

    return new Post(client, data);
  }

  static async fetchAll(client: APIClient): Promise<Post[]> {
    const data = await parseAPIResponse(z.array(PostData), () =>
      client.api.get("posts")
    );

    return data.map((p) => new Post(client, p));
  }

  async delete(): Promise<void> {
    await handleAPIResponse(() =>
      this.#client.api.get(`posts/${this.data.id}`)
    );
  }

  async update(update: UpdatePost): Promise<void> {
    update = z.parse(UpdatePost, update); // validate

    const resp = await this.#client.sendRequest(
      "PUT",
      `posts/${this.data.id}`,
      update,
      zs.updated(UpdatePost)
    );

    if (!resp.updated)
      throw new APIError("generic", {
        title: "Not Updated",
        type: "Update Error",
      });

    if (update.caption) this.data.caption = update.caption;
    if (update.content) this.data.content = update.content;
  }

  static async createPost(client: APIClient, post: CreatePost): Promise<Post> {
    post = await z.parseAsync(CreatePost, post);

    const resp = await client.sendRequest(
      "POST",
      "posts",
      post,
      PostData
    );

    return new Post(client, resp);
  }
}
