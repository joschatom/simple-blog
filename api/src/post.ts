import z from "zod";
import type { APIClient } from "./client.ts";
import { handleAPIResponse, parseAPIResponse } from "./error.ts";
import { CreatePost, PostData } from "./schemas/post.ts";
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
      client.api.get("posts"));

    return data.map(p => new Post(client, p));
  }

  async delete(): Promise<void> {
    await handleAPIResponse(() => this.#client.api.get(`posts/${this.data.id}`))    
  }


  static async createPost(client: APIClient, post: CreatePost): Promise<Post>{
    post = await z.parseAsync(CreatePost, post)

    
    const resp = await client.sendRequest("POST", "posts", post, PostData);

    return new Post(client, resp) 
  }


}
