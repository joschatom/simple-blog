import { use, useContext, useEffect, useState } from "react";
import { Client } from "../client";
import { Header } from "../components/Header";
import { Post } from "blog-api/src/post";
import { PostContainer } from "../components/Post";

export function PostsPage() {
  const client = useContext(Client);
  const [posts, setPosts] = useState<Post[]>([]);

  useEffect(() => {
    const load = async () => setPosts(await Post.fetchAll(client));

    load();
  }, [client]);
  return (
    <>
      <Header />

      {posts.map((p) => (
        <PostContainer post={p} />
      ))}
    </>
  );
}
