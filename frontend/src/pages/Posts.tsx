import { useContext, useEffect, useState } from "react";
import { Client } from "../contexts";
import { Header } from "../components/Header";
import { Post } from "blog-api/src/post";
import { PostContainer } from "../components/Post";

import "../styles/pages/Posts.css"
import { Footer } from "../components/Footer";

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
      <main>
        {posts.map((p) => (
          <PostContainer post={p} />
        ))}
      </main>
      <Footer/>
    </>
  );
}
