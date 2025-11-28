import z from "zod";
import { APIClient } from "./client.ts";
import { parseAPIResponse } from "./error.ts";
import { CreatePost, PostData } from "./schemas/post.ts";
export class Post {
  data: PostData;
  #client: APIClient;

  constructor(client: APIClient, data: PostData) {}

  async fetchById(client: APIClient, id: string): Promise<Post> {
    const data = await parseAPIResponse(PostData, () =>
      client.api.get(`/posts/${id}`)
    );

    return new Post(client, data);
  }
}
